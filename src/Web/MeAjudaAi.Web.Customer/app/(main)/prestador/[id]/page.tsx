import { notFound } from "next/navigation";
import { Metadata } from "next";
import { cache } from "react";
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";
import { Badge } from "@/components/ui/badge";
import { MessageCircle } from "lucide-react";
import { VerifiedBadge } from "@/components/ui/verified-badge";
import { z } from "zod";

import { EVerificationStatus, EProviderType } from "@/types/api/provider";

// Zod Schema for Runtime Validation
const PublicProviderSchema = z.object({
    id: z.string().uuid(),
    name: z.string(),
    type: z.preprocess((val) => {
        if (typeof val === 'string') {
            const lower = val.toLowerCase();
            if (lower === 'none') return EProviderType.None;
            if (lower === 'individual' || lower === 'pessoafisica') return EProviderType.Individual;
            if (lower === 'company' || lower === 'pessoajuridica') return EProviderType.Company;
            if (lower === 'freelancer' || lower === 'autonomo') return EProviderType.Freelancer;
            if (lower === 'cooperative' || lower === 'cooperativa') return EProviderType.Cooperative;

            const parsed = parseInt(val, 10);
            if (!isNaN(parsed)) return parsed;
        }
        return val;
    }, z.nativeEnum(EProviderType).optional()),
    fantasyName: z.string().optional().nullable(),
    description: z.string().optional().nullable(),
    city: z.string().optional().nullable(),
    state: z.string().optional().nullable(),
    rating: z.number().optional().nullable(),
    reviewCount: z.number().optional().nullable(),
    phoneNumbers: z.array(z.string()).optional().nullable(),
    services: z.array(z.string()).optional().nullable(),
    email: z.string().email().optional().nullable(),
    verificationStatus: z.preprocess((val) => {
        if (typeof val === 'string') {
            const lower = val.toLowerCase();
            if (lower === 'verified') return EVerificationStatus.Verified;
            if (lower === 'rejected') return EVerificationStatus.Rejected;
            if (lower === 'inprogress' || lower === 'in_progress') return EVerificationStatus.InProgress;
            if (lower === 'suspended') return EVerificationStatus.Suspended;
            if (lower === 'none') return EVerificationStatus.None;
            return EVerificationStatus.Pending;
        }
        return val;
    }, z.nativeEnum(EVerificationStatus).optional().nullable())
});

type PublicProviderData = z.infer<typeof PublicProviderSchema>;

const getCachedProvider = cache(async (id: string): Promise<PublicProviderData | null> => {
    try {
        const apiUrl = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL;
        if (!apiUrl) {
            throw new Error("Missing API_URL or NEXT_PUBLIC_API_URL environment variable.");
        }

        const res = await fetch(`${apiUrl}/api/v1/providers/${id}/public`, {
            next: { revalidate: 60 } // Cache for 60 seconds
        });

        if (res.status === 404) return null;
        if (!res.ok) {
            throw new Error(`Failed to fetch provider: ${res.statusText}`);
        }

        const json = await res.json();

        // API returns Result<PublicProviderDto> usually: { isSuccess: true, value: {...} }
        // We need to extract the value and validate it.
        let dataToValidate = json;
        if (json && typeof json === 'object' && 'value' in json) {
            dataToValidate = json.value;
        }

        // Validate with Zod
        const result = PublicProviderSchema.safeParse(dataToValidate);

        if (!result.success) {
            console.error(`Validation Error for provider ${id}:`, result.error);
            // Fail fast or return null? "fail fast with a thrown error if validation fails"
            throw new Error(`Invalid provider data received: ${result.error.message}`);
        }

        return result.data;

    } catch (error) {
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

    const services = providerData.services ?? [];

    const getWhatsappLink = (phone: string) => {
        let cleanPhone = phone.replace(/\D/g, "");
        // If it starts with 55 and has enough digits to be DDI(2)+DDD(2)+Phone(8-9), assume DDI exists
        if (cleanPhone.startsWith("55") && cleanPhone.length >= 12) {
            cleanPhone = cleanPhone.substring(2);
        }

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
                            containerClassName="h-32 w-32 border-4 border-white shadow-md text-3xl font-bold"
                        />

                        {/* Rating */}
                        <div className="flex items-center gap-2">
                            <Rating value={rating} className="text-[#E0702B]" />
                            {reviewCount > 0 && (
                                <span className="text-sm text-gray-600">({reviewCount} avaliações)</span>
                            )}
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

                        {/* Name & Badge */}
                        <div className="flex items-center gap-3">
                            <h1 className="text-3xl md:text-4xl font-bold text-[#E0702B]">{displayName}</h1>
                            <VerifiedBadge status={providerData.verificationStatus ?? EVerificationStatus.Pending} size="lg" />
                        </div>

                        {/* Email */}
                        {providerData.email && (
                            <p className="text-gray-500 font-medium text-sm lowercase">{providerData.email}</p>
                        )}

                        {/* Description */}
                        <div className="text-gray-600 leading-relaxed text-justify">
                            <p>{description}</p>
                        </div>

                        {/* Services */}
                        {services.length > 0 && (
                            <div className="pt-4">
                                <h2 className="text-lg font-bold text-gray-900 mb-3">Serviços</h2>
                                <div className="flex flex-wrap gap-2">
                                    {services.map((service: string, i: number) => (
                                        <Badge
                                            key={i}
                                            className="px-3 py-1 bg-[#E0702B] text-white text-sm rounded-full"
                                        >
                                            {service}
                                        </Badge>
                                    ))}
                                </div>
                            </div>
                        )}
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
