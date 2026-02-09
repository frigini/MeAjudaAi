import { notFound } from "next/navigation";
import { Metadata } from "next";
import { cache } from "react";
import { Card } from "@/components/ui/card";
import { Avatar } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Mail, MapPin, Phone } from "lucide-react";
import { apiProvidersGet2 } from "@/lib/api/generated/sdk.gen";
import { Rating } from "@/components/ui/rating";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";

// Deduplicate requests with React cache
const getCachedProvider = cache(async (id: string) => {
    const { data } = await apiProvidersGet2({
        path: { id },
    });
    return data?.data;
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
    const provider = await getCachedProvider(id);

    if (!provider) {
        notFound();
    }

    const businessProfile = provider.businessProfile;
    const contactInfo = businessProfile?.contactInfo;
    const address = businessProfile?.primaryAddress;

    // Display name: fantasy name or legal name or "Prestador"
    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || provider.name || "Prestador";

    // Mock rating for display until API supports it
    const mockRating = 4.8;
    const mockReviewCount: number = 12;

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Provider Header */}
            <Card className="mb-8 p-6">
                <div className="flex flex-col md:flex-row items-start gap-6">
                    <Avatar
                        src={undefined}
                        alt={displayName}
                        size="xl"
                        className="flex-shrink-0"
                    />

                    <div className="flex-1">
                        <h1 className="text-3xl font-bold text-foreground">
                            {displayName}
                        </h1>

                        {/* Mock rating for display until API supports it */}
                        {/* TODO: Replace with real API data (Rating/Reviews) when available. Tracked for future sprint. */}
                        <div className="flex items-center gap-2 mb-4">
                            <Rating value={mockRating} size="md" readOnly={true} />
                            <span className="text-lg font-semibold">{mockRating}</span>
                            <span className="text-foreground-subtle">
                                ({mockReviewCount}{" "}
                                {mockReviewCount === 1 ? "avaliação" : "avaliações"})
                            </span>
                        </div>

                        {/* Location */}

                        {address && (address.city || address.state) ? (
                            <div className="flex items-center gap-2 mt-4 text-foreground-subtle">
                                <MapPin className="size-4" />
                                <span>
                                    {[address.city, address.state].filter(Boolean).join(", ")}
                                </span>
                            </div>
                        ) : null}

                        {/* Contact Buttons */}
                        <div className="flex flex-wrap gap-3 mt-6">
                            {contactInfo?.email && (
                                <Button size="lg" asChild>
                                    <a href={`mailto:${encodeURIComponent(contactInfo.email)}`}>
                                        <Mail className="mr-2 size-4" />
                                        Enviar mensagem
                                    </a>
                                </Button>
                            )}
                            {contactInfo?.phoneNumber && (
                                <Button variant="outline" size="lg" asChild>
                                    <a href={`tel:${encodeURIComponent(contactInfo.phoneNumber)}`}>
                                        <Phone className="mr-2 size-4" />
                                        Ligar
                                    </a>
                                </Button>
                            )}
                        </div>
                    </div>
                </div>
            </Card>

            {/* About Section */}
            {
                businessProfile?.description && (
                    <Card className="mb-8 p-6">
                        <h2 className="text-2xl font-bold text-foreground mb-4">Sobre</h2>
                        <p className="text-foreground-subtle whitespace-pre-wrap leading-relaxed">
                            {businessProfile.description}
                        </p>
                    </Card>
                )
            }

            {/* Reviews Section */}
            <Card className="p-6">
                <h2 className="text-2xl font-bold text-foreground mb-6">Avaliações</h2>

                <ReviewForm providerId={id} />

                <div className="mt-8">
                    <ReviewList providerId={id} />
                </div>
            </Card>
        </div>
    );
}
