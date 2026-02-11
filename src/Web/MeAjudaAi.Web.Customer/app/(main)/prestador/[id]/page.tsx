import { notFound } from "next/navigation";
import { Metadata } from "next";
import { cache } from "react";
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { createClient, createConfig } from "@/lib/api/generated/client";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";
import { Badge } from "@/components/ui/badge";
import { MessageCircle } from "lucide-react";
import { MeAjudaAiContractsFunctionalError } from "@/lib/api/generated/types.gen";

const client = createClient(createConfig({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002'
}));

interface PublicProviderData {
    id: string;
    name: string;
    type: string;
    fantasyName?: string;
    description?: string;
    city?: string;
    state?: string;
    rating?: number;
    reviewCount?: number;
    phoneNumbers?: string[];
    services?: string[];
    email?: string;
}

const getCachedProvider = cache(async (id: string): Promise<PublicProviderData | null> => {
    try {
        const response = await client.get({
            url: `/api/v1/providers/${id}/public`
        });

        if (response.error) {
            const apiError = response.error as MeAjudaAiContractsFunctionalError;
            if (apiError.statusCode === 404) {
                return null;
            }
            console.error(`API Error fetching provider ${id}:`, apiError);
            throw new Error(apiError.message || 'Erro ao carregar perfil do prestador');
        }

        // API returns Result pattern: { isSuccess: true, value: {...} }
        const result = response.data as { isSuccess: boolean; value: PublicProviderData } | null;
        if (result?.isSuccess && result?.value) {
            return result.value;
        }

        return null;
    } catch (error: any) {
        if (error.status === 404 || error.statusCode === 404 || (error instanceof Error && error.message.includes("404"))) return null;
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

    const providerData = await getCachedProvider(id);

    if (!providerData) {
        notFound();
    }

    const displayName = providerData.fantasyName || providerData.name || "Prestador";
    const description = providerData.description || "Este prestador ainda não adicionou uma descrição.";

    const rating = providerData.rating ?? 0;
    const reviewCount = providerData.reviewCount ?? 0;

    const phones = providerData.phoneNumbers || [];

    const services = (providerData.services && providerData.services.length > 0)
        ? providerData.services
        : ["Serviço Geral"];

    const getWhatsappLink = (phone: string) => {
        const cleanPhone = phone.replace(/\D/g, "");
        // Validate: Brazilian phone should have at least 10 digits (DDD + number)
        return cleanPhone.length >= 10 ? `https://wa.me/55${cleanPhone}` : null;
    };

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Centered Profile Container */}
            <div className="max-w-4xl mx-auto">

                {/* Profile Section - Matching Figma Layout */}
                <div className="grid grid-cols-1 md:grid-cols-12 gap-8 mb-12">

                    {/* Left Column: Avatar, Rating, Phones */}
                    <div className="md:col-span-3 flex flex-col items-center space-y-4">
                        {/* Avatar */}
                        <Avatar
                            src={undefined}
                            alt={displayName}
                            fallback={displayName.substring(0, 2).toUpperCase()}
                            className="h-32 w-32 border-4 border-white shadow-md text-3xl font-bold"
                        />

                        {/* Rating */}
                        <div className="flex items-center">
                            <Rating value={rating} readOnly size="md" className="text-[#E0702B]" />
                        </div>

                        {/* Phones */}
                        {phones.length > 0 && (
                            <div className="w-full space-y-2">
                                {phones.map((phone: string, i: number) => {
                                    const whatsappLink = getWhatsappLink(phone);
                                    return (
                                        <div key={i} className="flex items-center gap-2 text-gray-600 text-sm">
                                            <span className="font-medium">{phone}</span>
                                            {whatsappLink && (
                                                <a
                                                    href={whatsappLink}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="text-green-500 hover:text-green-600 transition-colors"
                                                    title="Chamar no WhatsApp"
                                                >
                                                    <MessageCircle className="w-4 h-4" />
                                                </a>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    {/* Right Column: Name, Email, Description, Services */}
                    <div className="md:col-span-9 space-y-4">

                        {/* Name */}
                        <h1 className="text-3xl md:text-4xl font-bold text-[#E0702B]">{displayName}</h1>

                        {/* Email */}
                        {providerData.email && (
                            <p className="text-gray-500 font-medium text-sm lowercase">{providerData.email}</p>
                        )}

                        {/* Description */}
                        <div className="text-gray-600 leading-relaxed text-justify">
                            <p>{description}</p>
                        </div>

                        {/* Services */}
                        <div className="pt-4">
                            <h2 className="text-lg font-bold text-gray-900 mb-3">Serviços</h2>
                            <div className="flex flex-wrap gap-2">
                                {services.map((service: string, index: number) => (
                                    <Badge
                                        key={index}
                                        className="px-3 py-1 text-sm font-medium bg-[#E0702B] hover:bg-[#c56226] text-white border-none transition-colors rounded-md"
                                    >
                                        {service}
                                    </Badge>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Comments Section - Full Width (outside centered container) */}
            <div className="pt-8 border-t border-gray-200">
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-xl font-bold text-gray-900">Comentários</h2>
                </div>

                <div className="space-y-6">
                    <div className="bg-gray-50 p-3 rounded-lg">
                        <ReviewForm providerId={id} />
                    </div>

                    <ReviewList providerId={id} />
                </div>
            </div>
        </div>
    );
}
