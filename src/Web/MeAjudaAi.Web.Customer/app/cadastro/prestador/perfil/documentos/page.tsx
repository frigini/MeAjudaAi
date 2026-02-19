"use client";

import { useState } from "react";
import { useMyProviderProfile } from "@/hooks/use-my-provider-profile";
import { useDocumentUpload } from "@/hooks/use-document-upload";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { EDocumentStatus, EDocumentType } from "@/types/api/provider";
import { Loader2, Upload, FileText, CheckCircle, AlertCircle, Clock } from "lucide-react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";

export default function ProviderDocumentsPage() {
    const router = useRouter();
    const { data: profile, isLoading: isLoadingProfile } = useMyProviderProfile();
    const { uploadDocument, isUploading } = useDocumentUpload();
    const [uploadingType, setUploadingType] = useState<EDocumentType | null>(null);

    // Filter documents by type from profile
    const getDocument = (type: EDocumentType) => {
        return profile?.documents?.find(d => d.documentType === type);
    };

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>, type: EDocumentType) => {
        const file = e.target.files?.[0];
        if (!file) return;

        if (file.size > 5 * 1024 * 1024) { // 5MB limit
            toast.error("O arquivo deve ter no máximo 5MB.");
            return;
        }

        setUploadingType(type);
        try {
            await uploadDocument(file, type, profile?.id || "");
        } finally {
            setUploadingType(null);
            // Reset file input
            e.target.value = "";
        }
    };

    if (isLoadingProfile) {
        return (
            <div className="container mx-auto py-20 flex justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (!profile) {
        return (
            <div className="container mx-auto py-20 text-center">
                <p>Perfil não encontrado.</p>
                <Button onClick={() => window.location.reload()} variant="outline" className="mt-4">
                    Tentar Novamente
                </Button>
            </div>
        );
    }



    // Note: EDocumentType.CPF and EDocumentType.CNPJ are specific. 
    // Usually Identity covers RG/CNH. 
    // Is CPF separate? In Brazil, usually yes if RG doesn't have it.
    // Let's use Identity as primary ID.
    // And if type is Company, maybe CNPJ card?
    // For now, let's just list a standard set.

    // Actually, looking at types/api/provider.ts:
    // None = 0, CPF = 1, CNPJ = 2, RG = 3, CNH = 4, Passport = 5, Other = 99
    // So there is NO "Identity" generic type.
    // I should probably ask for "Identidade (RG, CNH ou Passaporte)" and map to RG or CNH based on user selection or just use "RG" as generic?
    // Or allow user to select type.
    // For MVP, let's ask for specific types or map "Identity" UI to a dropdown?
    // Let's simplify: Ask for "Documento de Identificação" and handle type selection later or default to RG/CNH?
    // Or just list types:
    // 1. Identificação (RG, CNH ou Passaporte)
    // 2. CPF (se não constar na identificação) - optional?
    // 3. Comprovante de Residência (Type not in enum? Just "Other" or mapped?)

    // Wait, EDocumentType enum in simple version is:
    // CPF, CNPJ, RG, CNH, Passport, Other.
    // There is NO "ProofOfResidence" in the enum I saw in grep?
    // Let's check grep again.
    // The grep output for UploadDocumentEndpoint comments mentioned: "- ProofOfResidence".
    // But verify the file provider.ts again.

    // provider.ts content:
    // export enum EDocumentType {
    //     None = 0,
    //     CPF = 1,
    //     CNPJ = 2,
    //     RG = 3,
    //     CNH = 4,
    //     Passport = 5,
    //     Other = 99
    // }

    // So "ProofOfResidence" is MISSING from provider.ts enum!
    // But backend UploadDocumentEndpoint logic supports it (comments say so).
    // This means frontend definitions are OUT OF SYNC with backend if backend supports it.
    // Or backend comments are aspirational.
    // I should check backend `EDocumentType.cs`.
    // But for now, I'll stick to what frontend supports: "Other" for residence?
    // Or just asking for CNH/RG.

    // Let's stick to asking for CNH or RG (Identidade).
    // And maybe Other for Residence?

    // Re-mapped required documents based on available ENUMS:


    const getStatusBadge = (status: EDocumentStatus) => {
        switch (status) {
            case EDocumentStatus.Verified:
                return <Badge variant="success"><CheckCircle className="w-3 h-3 mr-1" /> Verificado</Badge>;
            case EDocumentStatus.PendingVerification:
                return <Badge variant="warning"><Clock className="w-3 h-3 mr-1" /> Em Análise</Badge>;
            case EDocumentStatus.Uploaded:
                return <Badge variant="primary"><Upload className="w-3 h-3 mr-1" /> Enviado</Badge>;
            case EDocumentStatus.Rejected:
                return <Badge variant="destructive"><AlertCircle className="w-3 h-3 mr-1" /> Rejeitado</Badge>;
            default:
                return <Badge variant="secondary">Pendente</Badge>;
        }
    };

    return (
        <div className="container mx-auto py-10 max-w-2xl">
            <h1 className="text-2xl font-bold mb-6">Documentação</h1>
            <p className="text-muted-foreground mb-8">
                Envie os documentos necessários para validação do seu cadastro.
            </p>

            <div className="space-y-6">
                {/* Identity Section */}
                <Card>
                    <CardHeader>
                        <CardTitle className="text-lg">Documento de Identificação</CardTitle>
                        <CardDescription>
                            Envie seu RG, CNH ou Passaporte.
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        {/* Check if any identity doc is uploaded */}
                        {(() => {
                            const uploadedIdentity = profile.documents?.find(d =>
                                [EDocumentType.RG, EDocumentType.CNH, EDocumentType.Passport].includes(d.documentType)
                            );

                            if (uploadedIdentity) {
                                return (
                                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg border">
                                        <div className="flex items-center gap-3">
                                            <FileText className="h-8 w-8 text-primary/50" />
                                            <div>
                                                <p className="font-medium text-sm truncate max-w-[200px]">{uploadedIdentity.fileName}</p>
                                                <p className="text-xs text-muted-foreground">
                                                    Tipo: {uploadedIdentity.documentType === EDocumentType.RG ? "RG" :
                                                        uploadedIdentity.documentType === EDocumentType.CNH ? "CNH" : "Passaporte"}
                                                </p>
                                            </div>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            {getStatusBadge(uploadedIdentity.status)}
                                            {uploadedIdentity.status === EDocumentStatus.Rejected && (
                                                <div className="relative">
                                                    <input
                                                        type="file"
                                                        className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                                        accept=".pdf,.jpg,.jpeg,.png"
                                                        onChange={(e) => handleFileChange(e, uploadedIdentity.documentType)}
                                                        disabled={isUploading}
                                                    />
                                                    <Button variant="outline" size="sm">Reenviar</Button>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                );
                            }

                            return (
                                <div className="grid grid-cols-2 gap-4">
                                    <div className="col-span-2 md:col-span-1">
                                        <div className="relative">
                                            <input
                                                type="file"
                                                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                                accept=".pdf,.jpg,.jpeg,.png"
                                                onChange={(e) => handleFileChange(e, EDocumentType.CNH)}
                                                disabled={isUploading}
                                            />
                                            <Button variant="outline" className="w-full h-24 flex flex-col gap-2" disabled={isUploading}>
                                                {isUploading && uploadingType === EDocumentType.CNH ? (
                                                    <Loader2 className="h-6 w-6 animate-spin" />
                                                ) : (
                                                    <Upload className="h-6 w-6" />
                                                )}
                                                <span>Enviar CNH</span>
                                            </Button>
                                        </div>
                                    </div>
                                    <div className="col-span-2 md:col-span-1">
                                        <div className="relative">
                                            <input
                                                type="file"
                                                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                                accept=".pdf,.jpg,.jpeg,.png"
                                                onChange={(e) => handleFileChange(e, EDocumentType.RG)}
                                                disabled={isUploading}
                                            />
                                            <Button variant="outline" className="w-full h-24 flex flex-col gap-2" disabled={isUploading}>
                                                {isUploading && uploadingType === EDocumentType.RG ? (
                                                    <Loader2 className="h-6 w-6 animate-spin" />
                                                ) : (
                                                    <FileText className="h-6 w-6" />
                                                )}
                                                <span>Enviar RG</span>
                                            </Button>
                                        </div>
                                    </div>
                                </div>
                            );
                        })()}
                    </CardContent>
                </Card>

                {/* CPF Section (Optional if in Identity) */}
                <Card>
                    <CardHeader>
                        <CardTitle className="text-lg">CPF</CardTitle>
                        <CardDescription>
                            Necessário apenas se não constar no documento de identificação.
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        {(() => {
                            const cpf = getDocument(EDocumentType.CPF);

                            if (cpf) {
                                return (
                                    <div className="flex items-center justify-between p-4 bg-slate-50 rounded-lg border">
                                        <div className="flex items-center gap-3">
                                            <FileText className="h-8 w-8 text-primary/50" />
                                            <div>
                                                <p className="font-medium text-sm truncate max-w-[200px]">{cpf.fileName}</p>
                                            </div>
                                        </div>
                                        {getStatusBadge(cpf.status)}
                                    </div>
                                );
                            }

                            return (
                                <div className="relative">
                                    <input
                                        type="file"
                                        className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                        accept=".pdf,.jpg,.jpeg,.png"
                                        onChange={(e) => handleFileChange(e, EDocumentType.CPF)}
                                        disabled={isUploading}
                                    />
                                    <Button variant="outline" className="w-full h-16 flex items-center justify-center gap-2" disabled={isUploading}>
                                        {isUploading && uploadingType === EDocumentType.CPF ? (
                                            <Loader2 className="h-4 w-4 animate-spin" />
                                        ) : (
                                            <Upload className="h-4 w-4" />
                                        )}
                                        <span>Enviar Comprovante de CPF</span>
                                    </Button>
                                </div>
                            );
                        })()}
                    </CardContent>
                </Card>
            </div>

            <div className="mt-8 flex justify-end gap-4">
                <Button variant="outline" onClick={() => router.push("/cadastro/prestador/perfil")}>
                    Voltar
                </Button>
                <Button
                    onClick={() => router.push("/cadastro/prestador/perfil")}
                    variant={profile.status === 2 ? "primary" : "secondary"}
                >
                    Concluir
                </Button>
            </div>
        </div>
    );
}
