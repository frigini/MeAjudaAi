"use client";

import { useState } from "react";
import { Button } from "../../../components/ui/button";
import { FileUpload } from "../../../components/ui/file-upload";
import { useRouter } from "next/navigation";
import { z } from "zod";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { apiUploadPost, apiMeGet } from "@/lib/api/generated";
import { toast } from "sonner";

const documentSchema = z.object({
  identityFile: z
    .custom<File>((val) => val instanceof File, "O documento de identidade é obrigatório")
    .refine((file) => file.size <= 5 * 1024 * 1024, "O tamanho máximo é 5MB")
    .refine(
      (file) => ["image/jpeg", "image/png", "application/pdf"].includes(file.type),
      "Formato inválido. Aceitamos JPG, PNG ou PDF."
    ),
  certificateFile: z
    .instanceof(File)
    .refine((file) => !file || file.size <= 5 * 1024 * 1024, "O tamanho máximo é 5MB")
    .refine(
      (file) => !file || ["image/jpeg", "image/png", "application/pdf"].includes(file.type),
      "Formato inválido. Aceitamos JPG, PNG ou PDF."
    )
    .optional(),
});

type DocumentFormData = z.infer<typeof documentSchema>;

const DOCUMENT_TYPES = {
  identity: 1 as const,
  proofOfResidence: 2 as const,
  criminalRecord: 3 as const,
  other: 99 as const,
};

export default function DocumentsPage() {
  const router = useRouter();
  const [uploadProgress, setUploadProgress] = useState<string>("");

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<DocumentFormData>({
    resolver: zodResolver(documentSchema),
    mode: "onChange",
  });

  const uploadMutation = useMutation({
    mutationFn: async ({ file, documentType }: { file: File; documentType: 1 | 2 | 3 | 99 }) => {
      const uploadResponse = await apiUploadPost({
        body: {
          documentType,
          fileName: file.name,
          contentType: file.type,
          fileSizeBytes: file.size,
        },
      });

      if (!uploadResponse.data?.uploadUrl) {
        throw new Error("Falha ao obter URL de upload");
      }

      const { uploadUrl, documentId } = uploadResponse.data;

      setUploadProgress(`Enviando ${file.name}...`);

      await fetch(uploadUrl, {
        method: "PUT",
        body: file,
        headers: {
          "Content-Type": file.type,
        },
      });

      return { documentId, fileName: file.name };
    },
    onSuccess: () => {
      toast.success("Documento enviado com sucesso!");
    },
    onError: (error) => {
      console.error("Erro no upload", error);
      toast.error("Erro ao enviar documento. Tente novamente.");
    },
  });

  const onSubmit = async (data: DocumentFormData) => {
    setUploadProgress("");

    try {
      await uploadMutation.mutateAsync({
        file: data.identityFile,
        documentType: DOCUMENT_TYPES.identity,
      });

      if (data.certificateFile) {
        await uploadMutation.mutateAsync({
          file: data.certificateFile,
          documentType: DOCUMENT_TYPES.proofOfResidence,
        });
      }

      setUploadProgress("Concluindo...");
      router.push("/");
    } catch (error) {
      console.error("Erro no processo de upload", error);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Documentos</h2>
        <p className="mt-1 text-sm text-foreground-subtle">
          Para garantir a segurança da plataforma, precisamos validar sua identidade de forma segura.
        </p>
      </div>

      <div className="flex flex-col gap-8 border-t border-border pt-6">
        <Controller
          name="identityFile"
          control={control}
          render={({ field: { onChange } }) => (
            <div>
              <FileUpload
                label="Documento de Identidade (Frente e Verso)"
                description="Faça o upload do seu RG ou CNH. Formatos aceitos: .jpg, .png, .pdf."
                required
                onFileSelect={(file) => onChange(file)}
              />
              {errors.identityFile && (
                <p className="mt-1 text-sm text-destructive">{errors.identityFile.message as string}</p>
              )}
            </div>
          )}
        />

        <Controller
          name="certificateFile"
          control={control}
          render={({ field: { onChange } }) => (
            <div>
              <FileUpload
                label="Comprovante de Residência (Opcional)"
                description="Envie um comprovante de residência recente para verificar seu endereço."
                onFileSelect={(file) => onChange(file)}
              />
              {errors.certificateFile && (
                <p className="mt-1 text-sm text-destructive">{errors.certificateFile.message as string}</p>
              )}
            </div>
          )}
        />
      </div>

      {uploadProgress && (
        <div className="rounded-lg bg-secondary p-3 text-sm text-muted-foreground">
          {uploadProgress}
        </div>
      )}

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()} disabled={uploadMutation.isPending}>
          Voltar
        </Button>
        <Button type="submit" disabled={!isValid || uploadMutation.isPending}>
          {uploadMutation.isPending ? "Enviando para Azure..." : "Concluir Onboarding"}
        </Button>
      </div>
    </form>
  );
}
