"use client";

import { useMyProviderProfile } from "@/hooks/use-my-provider-profile";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { EProviderStatus, PROVIDER_STATUS_LABELS, PROVIDER_TIER_LABELS } from "@/types/api/provider";

function getProviderStatusMessage(status: EProviderStatus): string {
    switch (status) {
        case EProviderStatus.PendingBasicInfo:
            return "Complete seu cadastro informando seu endereço e enviando seus documentos.";
        case EProviderStatus.Active:
            return "Seu cadastro está ativo. Você já pode oferecer serviços.";
        case EProviderStatus.Rejected:
            return "Ocorreu um problema com seu cadastro. Verifique os motivos da rejeição e corrija as informações.";
        case EProviderStatus.PendingDocumentVerification:
            return "Seu cadastro está em análise. Aguarde a verificação dos documentos.";
        case EProviderStatus.Suspended:
            return "Seu cadastro está temporariamente suspenso.";
        default:
            return "Aguarde a atualização do seu status.";
    }
}

export default function ProviderProfilePage() {
    const { data: profile, isLoading, error, refetch } = useMyProviderProfile();

    if (isLoading) {
        return (
            <div className="container mx-auto py-10 flex justify-center">
                <div role="status" className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary">
                    <span className="sr-only">Carregando...</span>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="container mx-auto py-10 text-center max-w-md">
                <div className="text-red-500 mb-4 text-xl font-semibold">Erro ao carregar status</div>
                <p className="text-muted-foreground mb-6">Ocorreu um problema ao verificar seu cadastro.</p>
                <Button onClick={() => refetch()} variant="outline">Tentar Novamente</Button>
            </div>
        );
    }

    if (!profile) {
        return (
            <div className="container mx-auto py-20 text-center max-w-md">
                <h1 className="text-2xl font-bold mb-4">Você ainda não é um prestador</h1>
                <p className="text-muted-foreground mb-8">
                    Cadastre-se para começar a oferecer seus serviços na plataforma MeAjudaAi.
                </p>
                <Link href="/cadastro/prestador">
                    <Button size="lg" className="w-full">Quero ser prestador</Button>
                </Link>
            </div>
        );
    }

    const hasAddress = !!profile.businessProfile?.primaryAddress?.street;
    const hasDocuments = (profile.documents?.length ?? 0) > 0;
    const isPendingVerification = profile.status === EProviderStatus.PendingDocumentVerification;

    return (
        <div className="container mx-auto py-10 max-w-4xl">
            <h1 className="text-3xl font-bold mb-8">Processo de Credenciamento</h1>

            <div className="bg-white p-6 rounded-lg shadow-sm border mb-8">
                <div className="flex justify-between items-start mb-4">
                    <div>
                        <h2 className="text-xl font-semibold">Status da Conta</h2>
                        <p className="text-sm text-muted-foreground mt-1">Nível: <span className="font-medium text-primary">{PROVIDER_TIER_LABELS[profile.tier] ?? profile.tier ?? "Desconhecido"}</span></p>
                    </div>
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${profile.status === EProviderStatus.Active ? "bg-green-100 text-green-800" :
                        (profile.status === EProviderStatus.Rejected || profile.status === EProviderStatus.Suspended) ? "bg-red-100 text-red-800" :
                            "bg-amber-100 text-amber-800"
                        }`}>
                        {PROVIDER_STATUS_LABELS[profile.status] ?? profile.status ?? "Desconhecido"}
                    </span>
                </div>

                {profile.status === EProviderStatus.Rejected && profile.rejectionReason && (
                    <div className="mt-4 p-4 bg-red-50 text-red-800 text-sm rounded-md border border-red-200">
                        <strong>Motivo da rejeição:</strong> {profile.rejectionReason}
                    </div>
                )}

                {profile.status === EProviderStatus.Suspended && profile.suspensionReason && (
                    <div className="mt-4 p-4 bg-red-50 text-red-800 text-sm rounded-md border border-red-200">
                        <strong>Motivo da suspensão:</strong> {profile.suspensionReason}
                    </div>
                )}

                <div className="mt-4 pt-4 border-t">
                    <p className="text-muted-foreground">
                        {getProviderStatusMessage(profile.status)}
                    </p>
                </div>
            </div>

            <div className="grid gap-6 md:grid-cols-3">
                <div className="p-6 border rounded-lg bg-green-50/50 border-green-100">
                    <div className="flex items-center gap-2 mb-2">
                        <div className="h-6 w-6 rounded-full bg-green-200 flex items-center justify-center text-green-700 text-sm font-bold">✓</div>
                        <h3 className="font-medium">1. Dados Básicos</h3>
                    </div>
                    <p className="text-sm text-green-700">Concluído</p>
                </div>

                <div className={`p-6 border rounded-lg ${hasAddress || isPendingVerification
                    ? "bg-green-50/50 border-green-100"
                    : "bg-white border-primary/20 shadow-sm ring-1 ring-primary/20"
                    }`}>
                    <div className="flex items-center gap-2 mb-2">
                        <div className={`h-6 w-6 rounded-full flex items-center justify-center text-sm font-bold ${hasAddress || isPendingVerification
                            ? "bg-green-200 text-green-700"
                            : "bg-primary text-white"
                            }`}>
                            {hasAddress || isPendingVerification ? "✓" : "2"}
                        </div>
                        <h3 className="font-medium">2. Endereço</h3>
                    </div>

                    {hasAddress || isPendingVerification ? (
                        <div className="flex flex-col gap-2">
                            <p className="text-sm text-green-700">Concluído</p>
                            <Link href="/cadastro/prestador/perfil/endereco" className="text-xs text-primary underline">
                                Editar
                            </Link>
                        </div>
                    ) : (
                        <Link href="/cadastro/prestador/perfil/endereco">
                            <Button size="sm" className="w-full mt-2">Preencher Endereço</Button>
                        </Link>
                    )}
                </div>

                <div className={`p-6 border rounded-lg ${hasDocuments || isPendingVerification
                    ? "bg-green-50/50 border-green-100" // Completed
                    : hasAddress
                        ? "bg-white border-primary/20 shadow-sm ring-1 ring-primary/20" // Active
                        : "bg-slate-50 opacity-50" // Locked
                    }`}>
                    <div className="flex items-center gap-2 mb-2">
                        <div className={`h-6 w-6 rounded-full flex items-center justify-center text-sm font-bold ${hasDocuments || isPendingVerification ? "bg-green-200 text-green-700" :
                            hasAddress ? "bg-primary text-white" : "bg-slate-200 text-slate-500"
                            }`}>
                            {hasDocuments || isPendingVerification ? "✓" : "3"}
                        </div>
                        <h3 className="font-medium">3. Documentos</h3>
                    </div>

                    {!hasAddress ? (
                        <p className="text-sm text-muted-foreground">Bloqueado</p>
                    ) : (
                        <div>
                            {hasDocuments || isPendingVerification ? (
                                <div className="flex flex-col gap-2">
                                    <p className={`text-sm ${profile.status === EProviderStatus.Active ? "text-green-700" :
                                        profile.status === EProviderStatus.Rejected ? "text-red-700" :
                                            isPendingVerification ? "text-amber-700" : "text-green-700"
                                        }`}>
                                        {profile.status === EProviderStatus.Active ? "Verificados" :
                                            profile.status === EProviderStatus.Rejected ? "Rejeitados" :
                                                isPendingVerification ? "Em Análise" : "Enviados"}
                                    </p>
                                    <Link href="/cadastro/prestador/perfil/documentos" className="text-xs text-primary underline">
                                        Gerenciar
                                    </Link>
                                </div>
                            ) : (
                                <Link href="/cadastro/prestador/perfil/documentos">
                                    <Button size="sm" className="w-full mt-2">Enviar Documentos</Button>
                                </Link>
                            )}
                        </div>
                    )}
                </div>
            </div>

            <div className="mt-12 text-center">
                <Link href="/">
                    <Button variant="link">Voltar para a Home</Button>
                </Link>
            </div>
        </div>
    );
}
