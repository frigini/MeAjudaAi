import { Avatar } from "@/components/ui/avatar";
import { Card, CardContent } from "@/components/ui/card";
import { Rating } from "@/components/ui/rating";
import { formatDistanceToNow } from "date-fns";
import { ptBR } from "date-fns/locale";

export interface Review {
    id: string;
    authorName: string;
    authorAvatar?: string;
    rating: number;
    comment: string;
    createdAt: string | Date;
}

interface ReviewCardProps {
    review: Review;
}

export function ReviewCard({ review }: ReviewCardProps) {
    // Get initials for avatar fallback
    const initials = (review.authorName || "")
        .trim()
        .split(" ")
        .filter(Boolean)
        .map((n) => n[0] || "")
        .slice(0, 2)
        .join("")
        .toUpperCase() || "?";

    return (
        <Card className="mb-4">
            <CardContent className="pt-6">
                <div className="flex items-start gap-4">
                    <Avatar
                        src={review.authorAvatar}
                        alt={review.authorName}
                        size="md"
                        fallback={initials}
                    />

                    <div className="flex-1 space-y-1">
                        <div className="flex items-center justify-between">
                            <h4 className="text-sm font-semibold">{review.authorName}</h4>
                            <span className="text-xs text-muted-foreground">
                                {formatDistanceToNow(new Date(review.createdAt), {
                                    addSuffix: true,
                                    locale: ptBR,
                                })}
                            </span>
                        </div>

                        <Rating value={review.rating} readOnly size="sm" />

                        <p className="text-sm text-foreground mt-2 leading-relaxed">
                            {review.comment}
                        </p>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
