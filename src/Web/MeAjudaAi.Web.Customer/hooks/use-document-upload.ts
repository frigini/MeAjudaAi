import { useState } from "react";
import { useSession } from "next-auth/react";
import { authenticatedFetch } from "@/lib/api/fetch-client";
import { EDocumentType } from "@/types/api/provider";
import { toast } from "sonner";
import { useQueryClient } from "@tanstack/react-query";

interface UploadDocumentResponse {
    documentId: string;
    uploadUrl: string;
    blobName: string;
    expiresAt: string;
}

interface UseDocumentUploadOptions {
    onSuccess?: () => void;
    onError?: (error: Error) => void;
}

export function useDocumentUpload(options?: UseDocumentUploadOptions) {
    const { data: session } = useSession();
    const queryClient = useQueryClient();
    const [isUploading, setIsUploading] = useState(false);
    const [progress, setProgress] = useState(0);

    const uploadDocument = async (file: File, documentType: EDocumentType, providerId: string) => {
        if (!session?.accessToken || !providerId) {
            toast.error("Erro de autenticação ou ID do prestador inválido.");
            return;
        }

        setIsUploading(true);
        // Fake progress for UI feedback since fetch doesn't support progress events easily
        const progressInterval = setInterval(() => {
            setProgress((prev) => (prev >= 90 ? 90 : prev + 10));
        }, 300);

        try {
            // 1. Get SAS Token (Upload URL)
            const uploadResponse = await authenticatedFetch<UploadDocumentResponse>("/api/v1/documents/upload", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: {
                    providerId,
                    documentType,
                    fileName: file.name,
                    contentType: file.type || "application/octet-stream",
                    fileSizeBytes: file.size,
                },
                token: session.accessToken,
            });

            if (!uploadResponse || !uploadResponse.uploadUrl) {
                throw new Error("Falha ao obter URL de upload.");
            }

            // 2. Upload file to Azure Blob Storage using SAS URL
            // Direct fetch to Azure, no auth header needed (handled by SAS)
            const blobResponse = await fetch(uploadResponse.uploadUrl, {
                method: "PUT",
                headers: {
                    "x-ms-blob-type": "BlockBlob",
                    "Content-Type": file.type || "application/octet-stream",
                },
                body: file,
            });

            if (!blobResponse.ok) {
                console.error("Blob upload failed:", blobResponse.status, blobResponse.statusText);
                throw new Error(`Falha no upload do arquivo para o storage: ${blobResponse.statusText}`);
            }

            // 3. Register document in Provider Profile (using UploadMyDocumentEndpoint)
            await authenticatedFetch("/api/v1/providers/me/documents", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: {
                    documentType,
                    fileName: file.name,
                    fileUrl: uploadResponse.blobName, // Storing BlobName as reference
                },
                token: session.accessToken,
            });

            clearInterval(progressInterval);
            setProgress(100);

            // Invalidate queries to refresh profile
            queryClient.invalidateQueries({ queryKey: ["myProviderProfile"] });
            queryClient.invalidateQueries({ queryKey: ["providerStatus"] });

            toast.success("Documento enviado com sucesso!");
            options?.onSuccess?.();
        } catch (error: any) {
            clearInterval(progressInterval);
            setProgress(0);
            console.error("Upload process error:", error);
            const message = error.message || "Erro ao enviar documento.";
            toast.error(message);
            options?.onError?.(error);
        } finally {
            setIsUploading(false);
            clearInterval(progressInterval);
        }
    };

    return {
        uploadDocument,
        isUploading,
        progress
    };
}
