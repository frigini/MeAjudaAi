import { auth } from "@/auth";
import { redirect, unstable_rethrow } from "next/navigation";
import DashboardClient from "@/components/providers/dashboard-client";
import { ProviderDto, EVerificationStatus } from "@/types/api/provider";

export default async function DashboardPage() {
    const session = await auth();
    if (!session?.accessToken) {
        redirect("/api/auth/signin");
    }

    const apiUrl = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002';

    let provider: ProviderDto | null = null;
    let error: Error | null = null;
    let notFound = false;

    try {
        const res = await fetch(`${apiUrl}/api/v1/providers/me`, {
            headers: {
                "Authorization": `Bearer ${session.accessToken}`
            },
            cache: "no-store"
        });

        if (res.status === 401) {
            redirect("/api/auth/signin");
        }

        if (res.status === 404) {
            notFound = true;
        } else if (!res.ok) {
            throw new Error(`Failed to fetch provider profile: ${res.status}`);
        } else {
            const json = await res.json();
            
            if ("value" in json && json.value != null) {
                provider = json.value;
            } else {
                provider = json;
            }

            if (!provider || !provider.id) {
                throw new Error("Provider data is missing or invalid");
            }

            if (provider.verificationStatus && typeof provider.verificationStatus === 'string') {
                const statusStr = (provider.verificationStatus as unknown as string).toLowerCase();
                if (statusStr === 'verified') provider.verificationStatus = EVerificationStatus.Verified;
                else if (statusStr === 'rejected') provider.verificationStatus = EVerificationStatus.Rejected;
                else provider.verificationStatus = EVerificationStatus.Pending;
            }
        }
    } catch (err) {
        unstable_rethrow(err);
        console.error("Dashboard Error:", err);
        error = err instanceof Error ? err : new Error("Unknown error");
    }

    if (notFound) {
        return (
            <div className="container mx-auto py-12 text-center">
                <h1 className="text-2xl font-bold mb-4">Perfil de Prestador não encontrado</h1>
                <p>Parece que você ainda não completou seu cadastro como prestador.</p>
            </div>
        );
    }

    if (error || !provider) {
        return (
            <div className="container mx-auto py-12 text-center text-red-500">
                <h1 className="text-2xl font-bold mb-4">Erro ao carregar painel</h1>
                <p>Tente recarregar a página.</p>
            </div>
        );
    }

    return <DashboardClient provider={provider} />;
}
