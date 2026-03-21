"use client";

import { UploadCloud, File, X } from "lucide-react";
import { useCallback, useState } from "react";
import { twMerge } from "tailwind-merge";
import { Label } from "./label";

export interface FileUploadProps {
  label: string;
  description?: string;
  accept?: string;
  onFileSelect: (file: File) => void;
  className?: string;
  required?: boolean;
}

export function FileUpload({
  label,
  description,
  accept = "image/*,application/pdf",
  onFileSelect,
  className,
  required,
}: FileUploadProps) {
  const [isDragging, setIsDragging] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setIsDragging(true);
    } else if (e.type === "dragleave") {
      setIsDragging(false);
    }
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragging(false);
      
      if (e.dataTransfer.files && e.dataTransfer.files[0]) {
        const file = e.dataTransfer.files[0];
        setSelectedFile(file);
        onFileSelect(file);
      }
    },
    [onFileSelect]
  );

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      e.preventDefault();
      if (e.target.files && e.target.files[0]) {
        const file = e.target.files[0];
        setSelectedFile(file);
        onFileSelect(file);
      }
    },
    [onFileSelect]
  );

  const removeFile = useCallback(() => {
    setSelectedFile(null);
  }, []);

  return (
    <div className={twMerge("flex flex-col gap-2", className)}>
      <Label required={required}>{label}</Label>
      {description && <p className="text-xs text-muted-foreground">{description}</p>}

      {!selectedFile ? (
        <div
          className={twMerge(
            "relative flex cursor-pointer flex-col items-center justify-center gap-4 rounded-xl border-2 border-dashed border-border bg-surface p-8 transition-colors hover:bg-surface-raised",
            isDragging && "border-primary bg-secondary"
          )}
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
        >
          <input
            type="file"
            accept={accept}
            className="absolute inset-0 z-50 h-full w-full cursor-pointer opacity-0"
            onChange={handleChange}
          />
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-secondary text-primary">
            <UploadCloud className="size-6" />
          </div>
          <div className="text-center">
            <p className="text-sm font-medium">Clique para enviar ou arraste o arquivo</p>
            <p className="mt-1 text-xs text-muted-foreground">
              Formatos aceitos: PDF, JPG, PNG (Max 5MB)
            </p>
          </div>
        </div>
      ) : (
        <div className="flex items-center justify-between rounded-xl border border-border bg-surface p-4">
          <div className="flex items-center gap-3 overflow-hidden">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-secondary text-primary">
              <File className="size-5" />
            </div>
            <div className="flex flex-col overflow-hidden">
              <p className="truncate text-sm font-medium">{selectedFile.name}</p>
              <p className="text-xs text-muted-foreground">
                {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={removeFile}
            className="rounded-full p-2 text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
          >
            <X className="size-4" />
          </button>
        </div>
      )}
    </div>
  );
}
