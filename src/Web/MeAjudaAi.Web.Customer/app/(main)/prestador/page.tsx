import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { DashboardClient } from "@/components/providers/dashboard-client";

interface ProviderDto {
    id: string;
    userId: string;
    name: string;
    type: string;
    businessProfile: {
        legalName: string;
        fantasyName?: string;
        description?: string;
        contactInfo: {
            email: string;
            phoneNumber: string;
            website?: string;
        };
        primaryAddress: {
            street: string;
            number: string;
            complement?: string;
            neighborhood: string;
            city: string;
            state: string;
            zipCode: string;
            country: string;
        };
    };
    status: string;
    verificationStatus: string;
    services: Array<{ serviceId: string; serviceName: string }>;
}

export default async function DashboardPage() {
    const session = await auth();
    if (!session?.accessToken) {
        redirect("/api/auth/signin");
    }

    try {
        const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002'}/api/v1/providers/me`, {
            headers: {
                "Authorization": `Bearer ${session.accessToken}`
            },
            cache: "no-store" // Ensure fresh data on every visit
        });

        if (res.status === 401) redirect("/api/auth/signin");
        if (res.status === 404) {
            // User is logged in but not a provider? 
            // Or provider profile not created?
            // Should redirect to onboarding or show "Become a Provider"
            return (
                <div className="container mx-auto py-12 text-center">
                    <h1 className="text-2xl font-bold mb-4">Perfil de Prestador não encontrado</h1>
                    <p>Parece que você ainda não completou seu cadastro como prestador.</p>
                </div>
            );
        }

        if (!res.ok) {
            throw new Error(`Failed to fetch provider profile: ${res.status}`);
        }

        const json = await res.json();
        // API returns Result<ProviderDto>.
        // Check if json.value exists or if it returns direct object.
        // BaseEndpoint usually returns Result.

        let provider: ProviderDto;
        if (json.value) {
            provider = json.value;
        } else {
            provider = json;
        }

        return <DashboardClient provider={provider} accessToken={session.accessToken} />;

    } catch (error) {
        console.error("Dashboard Error:", error);
        return (
            <div className="container mx-auto py-12 text-center text-red-500">
                <h1 className="text-2xl font-bold mb-4">Erro ao carregar painel</h1>
                <p>Tente recarregar a página.</p>
            </div>
        );
    }
}
