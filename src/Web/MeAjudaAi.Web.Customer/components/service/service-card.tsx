import { Card } from "@/components/ui/card";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Rating } from "@/components/ui/rating";
import Link from "next/link";
import { cn } from "@/lib/utils";

export interface ServiceCardProps {
    id: string;
    name: string;
    avatarUrl?: string; // Should be nullable/optional
    description: string;
    services: string[]; // List of service names
    rating: number;
    reviewCount: number;
    className?: string;
}

export function ServiceCard({
    id,
    name,
    avatarUrl,
    description,
    services,
    rating,
    reviewCount,
    className
}: ServiceCardProps) {
    return (
        <Link href={`/prestador/${id}`} className="block h-full">
            <Card className={cn("p-6 hover:shadow-lg transition-shadow bg-[#F5F5F7] border-none rounded-3xl h-full", className)}>
                <div className="flex flex-col md:flex-row gap-6">
                    {/* Avatar */}
                    <div className="flex-shrink-0">
                        <Avatar
                            src={avatarUrl}
                            alt={name}
                            containerClassName="h-24 w-24 md:h-32 md:w-32 rounded-full border-4 border-white shadow-sm"
                            className="object-cover"
                            fallback={name.substring(0, 2).toUpperCase()}
                        />
                    </div>

                    {/* Content */}
                    <div className="flex-1 flex flex-col justify-between min-h-[128px]">
                        <div>
                            <h3 className="text-xl font-bold text-foreground mb-2">
                                {name}
                            </h3>
                            <p
                                className="text-muted-foreground text-sm leading-relaxed mb-4 line-clamp-3"
                                title={description} // Show full description on hover
                            >
                                {description}
                            </p>

                            {/* Service Tags */}
                            <div className="flex flex-wrap gap-2 mb-4">
                                {services.map((service) => (
                                    <Badge
                                        key={service}
                                        className="bg-[#E0702B] hover:bg-[#c56226] text-white font-normal px-4 py-1 text-sm rounded-full border-none"
                                    >
                                        {service}
                                    </Badge>
                                ))}
                            </div>
                        </div>

                        {/* Footer: Rating & Reviews */}
                        <div className="flex flex-col gap-1 mt-auto">
                            <div className="flex items-center gap-1">
                                <Rating value={rating} readOnly size="lg" />
                            </div>
                            <span className="text-sm font-medium text-foreground">
                                {reviewCount} {reviewCount === 1 ? 'comentário' : 'comentários'}
                            </span>
                        </div>
                    </div>
                </div>
            </Card>
        </Link>
    );
}
