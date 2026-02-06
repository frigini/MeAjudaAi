import { notFound } from "next/navigation";
import { Card } from "@/components/ui/card";
import { Avatar } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Mail, MapPin, Phone } from "lucide-react";
import { apiProvidersGet2 } from "@/lib/api/generated";
import { Rating } from "@/components/ui/rating";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";

interface ProviderProfilePageProps {
    params: Promise<{
        id: string;
    }>;
}

export default async function ProviderProfilePage({
    params,
}: ProviderProfilePageProps) {
    const { id } = await params;

    // Fetch provider from API
    const { data, error } = await apiProvidersGet2({
        path: {
            id,
        },
    });

    if (error || !data?.data) {
        notFound();
    }

    const provider = data.data;
    const businessProfile = provider.businessProfile;
    const contactInfo = businessProfile?.contactInfo;
    const address = businessProfile?.primaryAddress;

    // Display name: fantasy name or legal name or "Prestador"
    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || provider.name || "Prestador";

    // Mock rating for display until API supports it
    const mockRating = 4.8;
    const mockReviewCount = 12;

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Provider Header */}
            <Card className="mb-8 p-6">
                <div className="flex flex-col md:flex-row items-start gap-6">
                    <Avatar
                        src={null}
                        alt={displayName}
                        size="xl"
                        className="flex-shrink-0"
                    />

                    <div className="flex-1">
                        <h1 className="text-3xl font-bold text-foreground">
                            {displayName}
                        </h1>

                        <div className="flex items-center gap-2 mt-2">
                            <Rating value={Math.round(mockRating)} readOnly size="md" />
                            <span className="font-medium text-foreground">{mockRating}</span>
                            <span className="text-foreground-subtle">
                                ({mockReviewCount} avaliações)
                            </span>
                        </div>

                        {/* Location */}
                        {address && (address.city || address.state) && (
                            <div className="flex items-center gap-2 mt-4 text-foreground-subtle">
                                <MapPin className="size-4" />
                                <span>
                                    {address.city && address.state
                                        ? `${address.city}, ${address.state}`
                                        : address.city || address.state}
                                </span>
                            </div>
                        )}

                        {/* Contact Buttons */}
                        <div className="flex flex-wrap gap-3 mt-6">
                            {contactInfo?.email && (
                                <Button size="lg" asChild>
                                    <a href={`mailto:${contactInfo.email}`}>
                                        <Mail className="mr-2 size-4" />
                                        Enviar mensagem
                                    </a>
                                </Button>
                            )}
                            {contactInfo?.phoneNumber && (
                                <Button variant="outline" size="lg" asChild>
                                    <a href={`tel:${contactInfo.phoneNumber}`}>
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
            {businessProfile?.description && (
                <Card className="mb-8 p-6">
                    <h2 className="text-2xl font-bold text-foreground mb-4">Sobre</h2>
                    <p className="text-foreground-subtle whitespace-pre-wrap leading-relaxed">
                        {businessProfile.description}
                    </p>
                </Card>
            )}

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
