"use client";

import { useProviderStatus } from "@/hooks/use-provider-status";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { EProviderStatus, PROVIDER_STATUS_LABELS, PROVIDER_TIER_LABELS } from "@/types/provider";

export default function ProviderProfilePage() {
    const { data: providerStatus, isLoading, error } = useProviderStatus();

    if (isLoading) {
        return (
            <div className="container mx-auto py-10 flex justify-center">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="container mx-auto py-10 text-center max-w-md">
                <div className="text-red-500 mb-4 text-xl font-semibold">Erro ao carregar status</div>
                <p className="text-muted-foreground mb-6">Ocorreu um problema ao verificar seu cadastro.</p>
                <Button onClick={() => window.location.reload()} variant="outline">Tentar Novamente</Button>
            </div>
        );
    }

    if (!providerStatus) {
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

    return (
        <div className="container mx-auto py-10 max-w-4xl">
            <h1 className="text-3xl font-bold mb-8">Processo de Credenciamento</h1>

            <div className="bg-white p-6 rounded-lg shadow-sm border mb-8">
                <div className="flex justify-between items-start mb-4">
                    <div>
                        <h2 className="text-xl font-semibold">Status da Conta</h2>
                        <p className="text-sm text-muted-foreground mt-1">Nível: <span className="font-medium text-primary">{PROVIDER_TIER_LABELS[providerStatus.tier]}</span></p>
                    </div>
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${providerStatus.status === EProviderStatus.Active ? "bg-green-100 text-green-800" :
                        providerStatus.status === EProviderStatus.Rejected ? "bg-red-100 text-red-800" :
                            "bg-amber-100 text-amber-800"
                        }`}>
                        {PROVIDER_STATUS_LABELS[providerStatus.status]}
                    </span>
                </div>

                {providerStatus.rejectionReason && (
                    <div className="bg-red-50 p-4 rounded-md mb-4 border border-red-200">
                        <h4 className="text-red-800 font-medium mb-1">Motivo da Rejeição:</h4>
                        <p className="text-red-700 text-sm">{providerStatus.rejectionReason}</p>
                    </div>
                )}

                <div className="mt-4 pt-4 border-t">
                    <p className="text-muted-foreground">
                        {providerStatus.status === EProviderStatus.PendingBasicInfo
                            ? "Complete seu cadastro informando seu endereço e serviços."
                            : "Seu cadastro está em análise. Aguarde a verificação dos documentos."}
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

                <div className={`p-6 border rounded-lg ${providerStatus.status === EProviderStatus.PendingBasicInfo
                    || providerStatus.status === EProviderStatus.PendingDocumentVerification
                    || providerStatus.status === EProviderStatus.Active
                    ? "bg-white border-primary/20 shadow-sm ring-1 ring-primary/20"
                    : "bg-slate-50 opacity-70"
                    }`}>
                    <div className="flex items-center gap-2 mb-2">
                        <div className={`h-6 w-6 rounded-full flex items-center justify-center text-sm font-bold ${providerStatus.status === EProviderStatus.PendingBasicInfo
                            || providerStatus.status === EProviderStatus.PendingDocumentVerification
                            || providerStatus.status === EProviderStatus.Active
                            ? "bg-primary text-white"
                            : "bg-slate-200 text-slate-500"
                            }`}>2</div>
                        <h3 className="font-medium">2. Endereço</h3>
                    </div>
                    {providerStatus.status === EProviderStatus.PendingBasicInfo ? (
                        // Se apenas Info Básica está ok, Endereço é o próximo passo (ativo) - mas aqui estava "Em breve" (disabled)
                        // Se a intenção é que Endereço seja preenchido, deve ser um botão/link ativo.
                        // Ajustando conforme feedback para não 'dim', mas mantendo lógica de botão se 'Address' for o estado
                        // Como não temos EProviderStatus.PendingAddress, assumimos que PendingBasicInfo avança para Address.
                        <Link href="/cadastro/prestador/perfil/endereco">
                            <Button size="sm" className="w-full mt-2">Preencher Endereço</Button>
                        </Link>
                    ) : (
                        // Se já passou dessa fase (Verificação ou Ativo), mostra como Concluído ou Pendente de Validação?
                        // O feedback diz "treat as completed/accessible".
                        <p className="text-sm text-green-700">Concluído</p>
                    )}
                </div>

                <div className="p-6 border rounded-lg bg-slate-50 opacity-50">
                    <div className="flex items-center gap-2 mb-2">
                        <div className="h-6 w-6 rounded-full bg-slate-200 flex items-center justify-center text-slate-500 text-sm font-bold">3</div>
                        <h3 className="font-medium">Serviços e Docs</h3>
                    </div>
                    <p className="text-sm text-muted-foreground">Bloqueado</p>
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
