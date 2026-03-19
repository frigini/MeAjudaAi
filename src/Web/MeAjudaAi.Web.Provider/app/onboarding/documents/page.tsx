"use client";

import { useState } from "react";
import { Button } from "../../../components/ui/button";
import { FileUpload } from "../../../components/ui/file-upload";
import { useRouter } from "next/navigation";
import { z } from "zod";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

const documentSchema = z.object({
  identityFile: z
    .custom<File>((val) => val instanceof File, "O documento de identidade é obrigatório")
    .refine((file) => file.size <= 5 * 1024 * 1024, "O tamanho máximo é 5MB")
    .refine(
      (file) => ["image/jpeg", "image/png", "application/pdf"].includes(file.type),
      "Formato inválido. Aceitamos JPG, PNG ou PDF."
    ),
  certificateFile: z
    .custom<File>((val) => val === undefined || val instanceof File, "Opcional")
    .refine((file) => !file || file.size <= 5 * 1024 * 1024, "O tamanho máximo é 5MB")
    .optional(),
});

type DocumentFormData = z.infer<typeof documentSchema>;

export default function DocumentsPage() {
  const router = useRouter();
  const [isUploading, setIsUploading] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<DocumentFormData>({
    resolver: zodResolver(documentSchema),
    mode: "onChange",
  });

  const onSubmit = async (data: DocumentFormData) => {
    setIsUploading(true);

    try {
      // Exemplo de integração arquitetural com SAS Token
      console.log("Arquivos prontos para upload:", {
        identity: data.identityFile.name,
        certificate: data.certificateFile?.name,
      });
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
                label="Certificado Profissional (Opcional)"
                description="Envie certificados ou qualificações relevantes para aumentar sua credibilidade."
                onFileSelect={(file) => onChange(file)}
              />
              {errors.certificateFile && (
                <p className="mt-1 text-sm text-destructive">{errors.certificateFile.message as string}</p>
              )}
            </div>
          )}
        />
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()} disabled={isUploading}>
          Voltar
        </Button>
        <Button type="submit" disabled={!isValid || isUploading}>
          {isUploading ? "Enviando para Azure..." : "Concluir Onboarding"}
        </Button>
      </div>
    </form>
  );
}
