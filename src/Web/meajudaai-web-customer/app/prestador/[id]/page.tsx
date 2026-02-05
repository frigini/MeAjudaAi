import { Star, MapPin, Phone, Mail } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Rating } from "@/components/ui/rating";
import type { ProviderDto, ReviewDto } from "@/types/api/provider";
import { notFound } from "next/navigation";

// Mock data (will be replaced with API call)
const mockProvider: ProviderDto = {
    id: "1",
    name: "José Silva",
    email: "jose.silva@example.com",
    avatarUrl: null,
    averageRating: 4.5,
    reviewCount: 12,
    services: [
        { id: "1", name: "Pedreiro", category: "Construção" },
        { id: "2", name: "Pintura", category: "Construção" },
        { id: "3", name: "Reforma", category: "Construção" },
    ],
    city: "São Paulo",
    state: "SP",
    description:
        "Profissional com mais de 15 anos de experiência em construção civil. Especializado em reformas residenciais e comerciais. Trabalho com qualidade e pontualidade.",
    phone: "(11) 98765-4321",
    providerType: "Individual",
};

const mockReviews: ReviewDto[] = [
    {
        id: "1",
        providerId: "1",
        customerName: "Maria Santos",
        rating: 5,
        comment:
            "Excelente profissional! Fez a reforma da minha casa com muito capricho e dentro do prazo combinado.",
        createdAt: "2026-01-15T10:00:00Z",
    },
    {
        id: "2",
        providerId: "1",
        customerName: "João Oliveira",
        rating: 4,
        comment:
            "Bom trabalho, mas poderia ter sido um pouco mais rápido. No geral, recomendo.",
        createdAt: "2026-01-10T14:30:00Z",
    },
    {
        id: "3",
        providerId: "1",
        customerName: "Ana Costa",
        rating: 5,
        comment:
            "Muito profissional e atencioso. Tirou todas as minhas dúvidas e fez um trabalho impecável.",
        createdAt: "2026-01-05T09:15:00Z",
    },
];

interface ProviderProfilePageProps {
    params: {
        id: string;
    };
}

export default function ProviderProfilePage({
    params,
}: ProviderProfilePageProps) {
    // TODO: Replace with actual API call
    const provider = mockProvider;
    const reviews = mockReviews;

    if (!provider) {
        notFound();
    }

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Provider Header */}
            <Card padding="lg" className="mb-8">
                <div className="flex flex-col md:flex-row items-start gap-6">
                    <Avatar
                        src={provider.avatarUrl}
                        alt={provider.name}
                        size="xl"
                        className="flex-shrink-0"
                    />

                    <div className="flex-1">
                        <h1 className="text-3xl font-bold text-foreground">
                            {provider.name}
                        </h1>

                        {/* Rating */}
                        <div className="flex items-center gap-2 mt-2">
                            <Rating value={provider.averageRating} size="md" showValue />
                            <span className="text-foreground-subtle">
                                ({provider.reviewCount}{" "}
                                {provider.reviewCount === 1 ? "avaliação" : "avaliações"})
                            </span>
                        </div>

                        {/* Services */}
                        <div className="flex flex-wrap gap-2 mt-4">
                            {provider.services.map((service) => (
                                <Badge key={service.id} variant="secondary">
                                    {service.name}
                                </Badge>
                            ))}
                        </div>

                        {/* Location */}
                        <div className="flex items-center gap-2 mt-4 text-foreground-subtle">
                            <MapPin className="size-4" />
                            <span>
                                {provider.city}, {provider.state}
                            </span>
                        </div>

                        {/* Contact Buttons */}
                        <div className="flex flex-wrap gap-3 mt-6">
                            <Button variant="primary" size="lg">
                                Solicitar Serviço
                            </Button>
                            {provider.phone && (
                                <Button variant="outline" size="lg" asChild>
                                    <a href={`tel:${provider.phone}`}>
                                        <Phone className="size-4" />
                                        {provider.phone}
                                    </a>
                                </Button>
                            )}
                            {provider.email && (
                                <Button variant="outline" size="lg" asChild>
                                    <a href={`mailto:${provider.email}`}>
                                        <Mail className="size-4" />
                                        Enviar Email
                                    </a>
                                </Button>
                            )}
                        </div>
                    </div>
                </div>

                {/* About */}
                {provider.description && (
                    <div className="mt-8 pt-8 border-t border-border">
                        <h2 className="text-xl font-semibold mb-3 text-foreground">
                            Sobre
                        </h2>
                        <p className="text-foreground-subtle leading-relaxed">
                            {provider.description}
                        </p>
                    </div>
                )}
            </Card>

            {/* Reviews */}
            <Card padding="lg">
                <CardHeader>
                    <CardTitle>Avaliações ({reviews.length})</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="space-y-6">
                        {reviews.map((review) => (
                            <div key={review.id} className="border-b border-border pb-6 last:border-0 last:pb-0">
                                <div className="flex items-start justify-between mb-2">
                                    <div>
                                        <p className="font-medium text-foreground">
                                            {review.customerName}
                                        </p>
                                        <p className="text-sm text-foreground-subtle">
                                            {new Date(review.createdAt).toLocaleDateString("pt-BR", {
                                                day: "2-digit",
                                                month: "long",
                                                year: "numeric",
                                            })}
                                        </p>
                                    </div>
                                    <Rating value={review.rating} size="sm" />
                                </div>
                                <p className="text-foreground-subtle leading-relaxed">
                                    {review.comment}
                                </p>
                            </div>
                        ))}
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
