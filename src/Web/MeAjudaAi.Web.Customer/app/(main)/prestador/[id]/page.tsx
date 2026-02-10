import { notFound } from "next/navigation";
import { Metadata } from "next";
import { cache } from "react";
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { createClient, createConfig } from "@/lib/api/generated/client";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";
import { Badge } from "@/components/ui/badge";
import { MessageCircle, ArrowUpDown } from "lucide-react";
import { MeAjudaAiContractsFunctionalError } from "@/lib/api/generated/types.gen";

// Initialize client directly
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

// Deduplicate requests with React cache
const getCachedProvider = cache(async (id: string): Promise<PublicProviderData | null> => {
    try {
        const response = await client.get<{ data: PublicProviderData }>({
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

        return response.data || null;
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

    // Fetch real data
    const providerData = await getCachedProvider(id);

    if (!providerData) {
        notFound();
    }

    const displayName = providerData.fantasyName || providerData.name || "Prestador";
    const description = providerData.description || "Este prestador ainda não adicionou uma descrição.";

    // Using data from PublicProviderDto with safe fallbacks
    const rating = providerData.rating ?? 0;
    const reviewCount = providerData.reviewCount ?? 0;

    // Ensure phones and services are arrays
    const phones = (providerData.phoneNumbers && providerData.phoneNumbers.length > 0)
        ? providerData.phoneNumbers
        : ["(00) 0 0000-0000"];

    const services = (providerData.services && providerData.services.length > 0)
        ? providerData.services
        : ["Serviço Geral"];

    // Format phone for WhatsApp link (remove non-digits)
    const getWhatsappLink = (phone: string) => {
        const cleanPhone = phone.replace(/\D/g, "");
        return `https://wa.me/55${cleanPhone}`;
    };

    return (
        <div className="container mx-auto px-4 py-8 max-w-6xl space-y-12">

            {/* Top Section: Centralized Profile Info */}
            <div className="grid grid-cols-1 md:grid-cols-12 gap-8 items-start">

                {/* Left Column: Avatar, Rating, Phones */}
                <div className="md:col-span-3 flex flex-col items-center md:items-end space-y-6 text-center md:text-right">
                    {/* Avatar */}
                    <Avatar
                        src={undefined}
                        alt={displayName}
                        fallback={displayName.substring(0, 2).toUpperCase()}
                        className="h-40 w-40 border-4 border-white shadow-md text-4xl font-bold rounded-full overflow-hidden"
                    />

                    {/* Rating */}
                    <div className="flex flex-col items-center md:items-end gap-1">
                        <Rating value={rating} readOnly size="md" className="text-[#E0702B]" />
                    </div>

                    {/* Phones */}
                    <div className="w-full flex flex-col items-center md:items-end space-y-2">
                        {phones.map((phone: string, i: number) => (
                            <div key={i} className="flex items-center gap-2 text-gray-600 justify-end">
                                <span className="text-sm font-medium whitespace-nowrap">{phone}</span>
                                <a
                                    href={getWhatsappLink(phone)}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="text-green-500 hover:text-green-600 transition-colors"
                                    title="Chamar no WhatsApp"
                                >
                                    <MessageCircle className="w-4 h-4" />
                                </a>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Right Column: Name, Email, Description, Services */}
                <div className="md:col-span-9 space-y-8">

                    {/* Header Info */}
                    <div className="space-y-1 text-center md:text-left">
                        <h1 className="text-3xl md:text-4xl font-bold text-[#E0702B]">{displayName}</h1>
                        {providerData.email && (
                            <p className="text-gray-500 font-medium lowercase">{providerData.email}</p>
                        )}
                    </div>

                    {/* Description */}
                    <div className="prose prose-stone max-w-none text-gray-600 leading-relaxed whitespace-pre-wrap text-justify md:text-left">
                        <p>{description}</p>
                    </div>

                    {/* Services */}
                    <div>
                        <h2 className="text-xl font-bold text-gray-900 mb-4">Serviços</h2>
                        <div className="flex flex-wrap gap-3">
                            {services.map((service: string, index: number) => (
                                <Badge
                                    key={index}
                                    className="px-4 py-1.5 text-sm font-medium bg-[#E0702B] hover:bg-[#c56226] text-white border-none transition-colors rounded-md"
                                >
                                    {service}
                                </Badge>
                            ))}
                        </div>
                    </div>
                </div>
            </div>

            {/* Bottom Section: Reviews (Full Width) */}
            <div className="pt-8 border-t border-gray-100 w-full">
                <div className="flex items-center justify-between mb-8">
                    <div className="flex items-center gap-2">
                        <h2 className="text-2xl font-bold text-gray-900">Comentários</h2>
                        <ArrowUpDown className="w-5 h-5 text-gray-400" />
                    </div>
                </div>

                {/* Review Form & List - Spanning full width */}
                <div className="space-y-12">
                    {/* We can potentially move the form to a modal or keep it here depending on UX preferences 
                        For now, keeping it here but full width */}
                    {/* <div className="max-w-2xl"> 
                         <ReviewForm providerId={id} />
                    </div> */}

                    <ReviewList providerId={id} />
                </div>
            </div>
        </div>
    );
}
