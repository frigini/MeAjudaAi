import { notFound } from "next/navigation";
import { Metadata } from "next";
import { cache } from "react";
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { createClient, createConfig } from "@/lib/api/generated/client";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";
import { Badge } from "@/components/ui/badge";
import { MeAjudaAiContractsFunctionalError } from "@/lib/api/generated/types.gen";

// Initialize client directly to avoid potential circular dependency issues with generated barrel files
const client = createClient(createConfig({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002'
}));

const BRAND_COLOR = "#E0702B";
const BRAND_HOVER = "#c56226";

/**
 * Public Data DTO as returned by the API
 */
interface PublicProviderData {
    id: string;
    name: string;
    type: string;
    fantasyName?: string;
    description?: string;
    city?: string;
    state?: string;
    rating?: number;
    phoneNumbers?: string[];
    services?: string[];
}

// Deduplicate requests with React cache
const getCachedProvider = cache(async (id: string): Promise<PublicProviderData | null> => {
    try {
        const response = await client.get<{ data: PublicProviderData }>({
            url: `/api/v1/providers/${id}/public`
        });

        if (response.error) {
            const apiError = response.error as MeAjudaAiContractsFunctionalError;
            // Handle 404 explicitly as "not found"
            if (apiError.statusCode === 404) {
                return null;
            }

            // Other transient errors should be captured by error boundaries
            console.error(`API Error fetching provider ${id}:`, apiError);
            throw new Error(apiError.message || 'Erro ao carregar perfil do prestador');
        }

        return response.data || null;
    } catch (error) {
        // Fallback for network errors that look like 404s
        if (error instanceof Error && error.message.includes("404")) return null;
        console.error(`Exception fetching public provider ${id}:`, error);
        throw error;
    }
});

interface ProviderProfilePageProps {
    params: Promise<{
        id: string;
    }>;
}

export async function generateMetadata({
    params,
}: ProviderProfilePageProps): Promise<Metadata> {
    const { id } = await params;
    const provider = await getCachedProvider(id);

    if (!provider) {
        return {
            title: "Prestador não encontrado | MeAjudaAi",
        };
    }

    const displayName = provider.fantasyName || provider.name || "Prestador";
    const description = provider.description || `Confira o perfil de ${displayName} no MeAjudaAi.`;

    return {
        title: `${displayName} | MeAjudaAi`,
        description,
        openGraph: {
            title: `${displayName} | MeAjudaAi`,
            description,
        },
    };
}

export default async function ProviderProfilePage({
    params,
}: ProviderProfilePageProps) {
    const { id } = await params;

    // Fetch real data (public endpoint)
    const providerData = await getCachedProvider(id);

    if (!providerData) {
        notFound();
    }

    // Prepare display data from PublicProviderDto
    const displayName = providerData.fantasyName || providerData.name || "Prestador";
    const description = providerData.description || "Este prestador ainda não adicionou uma descrição.";
    const cityState = providerData.city && providerData.state ? `${providerData.city} - ${providerData.state}` : "";

    // Using data from PublicProviderDto with safe fallbacks
    // Use ?? instead of || so 0 rating is preserved if possible (though improbable in this context)
    const rating = providerData.rating ?? 4.8;
    const phones = (providerData.phoneNumbers && providerData.phoneNumbers.length > 0) ? providerData.phoneNumbers : ["(00) 0 0000 - 0000"];
    const services = (providerData.services && providerData.services.length > 0) ? providerData.services : ["Serviço Geral"];

    return (
        <div className="container mx-auto px-4 py-8 max-w-5xl">
            {/* Header Section */}
            <div className="flex flex-col items-center text-center mb-12">
                <Avatar
                    size="xl"
                    className="h-32 w-32 border-4 border-white shadow-sm mb-4"
                    src={undefined}
                    alt={displayName}
                    fallback={displayName.substring(0, 2).toUpperCase()}
                />

                <h1 className="text-3xl font-bold mb-1" style={{ color: BRAND_COLOR }}>{displayName}</h1>
                <p className="text-muted-foreground text-sm mb-6">{cityState}</p>

                <div className="max-w-3xl mb-6">
                    <p className="text-sm leading-relaxed text-foreground-subtle text-justify">
                        {description}
                    </p>
                </div>

                <div className="flex flex-col items-center gap-2 mb-6">
                    <div className="flex gap-1">
                        <Rating value={rating} readOnly size="md" />
                    </div>
                </div>

                <div className="space-y-1">
                    {phones.map((phone: string, i: number) => (
                        <div key={i} className="flex items-center justify-center gap-2 text-sm text-foreground-subtle">
                            {phone} <span className="text-green-500">📱</span>
                        </div>
                    ))}
                </div>
            </div>

            {/* Services Section */}
            <div className="mb-12">
                <h2 className="text-xl font-bold mb-4">Serviços</h2>
                <div className="flex flex-wrap gap-2">
                    {services.map((service: string, index: number) => (
                        <Badge
                            key={index}
                            className="text-white font-normal px-4 py-1.5 text-sm rounded-md border-none"
                            style={{ backgroundColor: BRAND_COLOR }}
                        >
                            {service}
                        </Badge>
                    ))}
                </div>
            </div>

            {/* Reviews Section */}
            <div className="mb-12">
                <ReviewForm providerId={id} />
                <ReviewList />
            </div>
        </div>
    );
}
