import { notFound } from "next/navigation";
import { Metadata } from "next";
import { cache } from "react";
import { Avatar } from "@/components/ui/avatar";
import { apiProvidersGet2 } from "@/lib/api/generated/sdk.gen";
import { Rating } from "@/components/ui/rating";
import { ReviewList } from "@/components/reviews/review-list";
import { Badge } from "@/components/ui/badge";

// Deduplicate requests with React cache
const getCachedProvider = cache(async (id: string) => {
    try {
        const { data } = await apiProvidersGet2({
            path: { id },
        });
        return data?.data;
    } catch (error) {
        console.error("Error fetching provider:", error);
        return null;
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

    const businessProfile = provider.businessProfile;
    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || provider.name || "Prestador";
    const description = businessProfile?.description?.slice(0, 160) || `Confira o perfil de ${displayName} no MeAjudaAi.`;

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

    // Fetch real data
    const providerData = await getCachedProvider(id);

    if (!providerData) {
        notFound();
    }

    // Prepare display data (mixing real + mock where missing)
    const businessProfile = providerData.businessProfile;
    const contactInfo = businessProfile?.contactInfo;

    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || providerData.name || "Prestador";
    const description = businessProfile?.description || "Este prestador ainda não adicionou uma descrição.";
    const email = contactInfo?.email || "Email não disponível";

    // Mocked data for UI Refinement requirements
    const mockRating = 4.8;
    const mockPhones = contactInfo?.phoneNumber ? [contactInfo.phoneNumber] : ["(00) 0 0000 - 0000"];
    const mockServices = ["Serviço com nome grande", "Serviço 3", "Serviço 3", "Serviço 3", "Serviço 3", "Serviço com nome grande"];

    return (
        <div className="container mx-auto px-4 py-8 max-w-5xl">
            {/* Header Section */}
            <div className="flex flex-col items-center text-center mb-12">
                <Avatar
                    size="xl"
                    className="h-32 w-32 border-4 border-white shadow-sm mb-4"
                    src={undefined} // No avatar URL in DTO yet
                    alt={displayName}
                    fallback={displayName.substring(0, 2).toUpperCase()}
                />

                <h1 className="text-3xl font-bold text-[#E0702B] mb-1">{displayName}</h1>
                <p className="text-muted-foreground text-sm mb-6">{email}</p>

                <div className="max-w-3xl mb-6">
                    <p className="text-sm leading-relaxed text-foreground-subtle text-justify">
                        {description}
                    </p>
                </div>

                <div className="flex flex-col items-center gap-2 mb-6">
                    <div className="flex gap-1">
                        <Rating value={mockRating} readOnly size="md" />
                    </div>
                </div>

                <div className="space-y-1">
                    {mockPhones.map((phone, i) => (
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
                    {mockServices.map((service, index) => (
                        <Badge
                            key={index}
                            className="bg-[#E0702B] hover:bg-[#c56226] text-white font-normal px-4 py-1.5 text-sm rounded-md border-none"
                        >
                            {service}
                        </Badge>
                    ))}
                </div>
            </div>

            {/* Reviews Section */}
            <div className="mb-12">
                <ReviewList providerId={id} />
            </div>
        </div>
    );
}
