"use client";

import { useState } from "react";
import { Button } from "../../../components/ui/button";
import { FileUpload } from "../../../components/ui/file-upload";
import { useRouter } from "next/navigation";

export default function DocumentsPage() {
  const router = useRouter();
  const [identityFile, setIdentityFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  // Exemplo de integração arquitetural com SAS Token:
  const handleUploadAndSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!identityFile) return;

    setIsUploading(true);

    try {
      // 1. Requisitar SAS Token (Presigned URL)
      // const sasResponse = await fetch("/api/v1/documents/sas-token", { method: "POST" });
      // const { uploadUrl, fileUrl } = await sasResponse.json();

      // 2. Fazer PUT direto para o storage do Azure com as bordalas do arquivo
      // await fetch(uploadUrl, { method: "PUT", body: identityFile, headers: { "x-ms-blob-type": "BlockBlob" }});

      // 3. Confirmar anexo no backend associando a URI pública salva
      // await fetch("/api/v1/providers/me/documents", {
      //   method: "POST",
      //   body: JSON.stringify({ 
      //     documentType: "Identity", 
      //     number: "simulated-number", 
      //     fileName: identityFile.name,
      //     fileUrl: fileUrl 
      //   })
      // });

      // Simulação de espera
      await new Promise((r) => setTimeout(r, 1500));
      
      router.push("/"); // Volta pro Dashboard
    } catch (error) {
      console.error("Erro no upload", error);
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <form onSubmit={handleUploadAndSave} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Documentos</h2>
        <p className="mt-1 text-sm text-foreground-subtle">
          Para garantir a segurança da plataforma, precisamos validar sua identidade de forma segura.
        </p>
      </div>

      <div className="flex flex-col gap-6 border-t border-border pt-6">
        <FileUpload
          label="Documento de Identidade (Frente e Verso)"
          description="Faça o upload do seu RG, CNH. Formatos aceitos: .jpg, .png, .pdf."
          required
          onFileSelect={setIdentityFile}
        />
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()} disabled={isUploading}>
          Voltar
        </Button>
        <Button type="submit" disabled={!identityFile || isUploading}>
          {isUploading ? "Enviando para Azure..." : "Concluir Onboarding"}
        </Button>
      </div>
    </form>
  );
}
