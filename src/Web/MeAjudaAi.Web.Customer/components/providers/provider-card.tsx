import { MapPin } from "lucide-react";
import { VerifiedBadge } from "@/components/ui/verified-badge";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { ProviderDto, EVerificationStatus } from "@/types/api/provider";
import Link from "next/link";

export interface ProviderCardProps {
    provider: ProviderDto;
}

export function ProviderCard({ provider }: ProviderCardProps) {
    return (
        <Link href={`/prestador/${provider.id}`}>
            <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer h-full">
                <div className="flex items-start gap-4">
                    <Avatar
                        src={provider.avatarUrl}
                        alt={provider.name}
                        size="lg"
                        className="flex-shrink-0"
                    />

                    <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                            <h3 className="font-semibold text-lg text-foreground truncate">
                                {provider.name}
                            </h3>
                            {provider.verificationStatus === EVerificationStatus.Verified && (
                                <VerifiedBadge status={EVerificationStatus.Verified} size="sm" showLabel={false} />
                            )}
                        </div>

                        {/* Rating */}
                        <div className="flex items-center gap-2 mt-1">
                            <Rating value={provider.averageRating ?? 0} size="sm" readOnly={true} />
                            <span className="text-sm text-foreground-subtle">
                                ({provider.reviewCount}{" "}
                                {provider.reviewCount === 1 ? "avaliação" : "avaliações"})
                            </span>
                        </div>

                        {/* Services */}
                        <div className="flex flex-wrap gap-2 mt-3">
                            {provider.services.slice(0, 3).map((service) => (
                                <Badge key={service.serviceId} variant="secondary">
                                    {service.serviceName}
                                </Badge>
                            ))}
                            {provider.services.length > 3 && (
                                <Badge variant="default">
                                    +{provider.services.length - 3}
                                </Badge>
                            )}
                        </div>

                        {/* Location */}
                        <div className="flex items-center gap-1 mt-3 text-sm text-foreground-subtle">
                            <MapPin className="size-4 flex-shrink-0" />
                            <span className="truncate">
                                {provider.city}, {provider.state}
                            </span>
                        </div>
                    </div>
                </div>
            </Card>
        </Link>
    );
}
