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
    return (
        <Card className="mb-4 bg-white rounded-2xl shadow-sm border border-border/50">
            <CardContent className="p-6">
                <div className="flex flex-col gap-4">
                    {/* Rating Stars */}
                    <div>
                        <Rating value={review.rating} readOnly size="md" />
                    </div>

                    {/* Review Text */}
                    <p className="text-foreground text-base leading-relaxed">
                        {review.comment}
                    </p>

                    {/* Author & Date - Right Aligned */}
                    <div className="flex flex-col items-end mt-2">
                        <div className="flex items-center gap-1">
                            <span className="font-semibold text-orange-500 text-base">
                                {review.authorName}
                            </span>
                            {/* Verified checkmark if needed, image shows a green check */}
                            <span className="text-green-500">✓</span>
                        </div>
                        <span className="text-sm text-muted-foreground">
                            {/* Hardcoded format to match image "dd/MM/yyyy" or use date-fns */}
                            {new Date(review.createdAt).toLocaleDateString('pt-BR')}
                        </span>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
