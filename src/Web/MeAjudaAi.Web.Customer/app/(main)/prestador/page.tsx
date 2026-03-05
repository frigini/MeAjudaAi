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

    try {
        const res = await fetch(`${apiUrl}/api/v1/providers/me`, {
            headers: {
                "Authorization": `Bearer ${session.accessToken}`
            },
            cache: "no-store" // Ensure fresh data on every visit
        });

        if (res.status === 401) {
            redirect("/api/auth/signin");
        }

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
        if ("value" in json && json.value != null) {
            provider = json.value;
        } else {
            provider = json;
        }

        // Validate provider existence before rendering
        if (!provider || !provider.id) {
            throw new Error("Provider data is missing or invalid");
        }

        // Normalize verificationStatus (API might return lowercase or string)
        if (provider.verificationStatus && typeof provider.verificationStatus === 'string') {
            const statusStr = (provider.verificationStatus as unknown as string).toLowerCase();
            if (statusStr === 'verified') provider.verificationStatus = EVerificationStatus.Verified;
            else if (statusStr === 'rejected') provider.verificationStatus = EVerificationStatus.Rejected;
            else provider.verificationStatus = EVerificationStatus.Pending;
        }

        return <DashboardClient provider={provider} />;

    } catch (error) {
        // Allow Next.js redirects to bubble up
        unstable_rethrow(error);

        console.error("Dashboard Error:", error);
        return (
            <div className="container mx-auto py-12 text-center text-red-500">
                <h1 className="text-2xl font-bold mb-4">Erro ao carregar painel</h1>
                <p>Tente recarregar a página.</p>
            </div>
        );
    }
}
