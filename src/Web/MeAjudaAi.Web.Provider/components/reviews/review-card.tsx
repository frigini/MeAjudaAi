import { CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { Rating } from "@/components/ui/rating";

export interface Review {
    id: string;
    authorName: string;
    rating: number;
    comment: string;
    verified: boolean;
    createdAt: Date;
}

interface ReviewCardProps {
    review: Review;
    className?: string;
}

export function ReviewCard({ review, className }: ReviewCardProps) {
    return (
        <div className={cn("bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col gap-4", className)}>
            {/* Header: Rating */}
            <div className="flex gap-1">
                <Rating value={review.rating} readOnly size="sm" />
            </div>

            {/* Comment */}
            <p className="text-foreground-subtle text-sm leading-relaxed flex-grow">
                {review.comment}
            </p>

            {/* Footer: Author & Date */}
            <div className="flex items-center justify-end gap-2 text-xs">
                <span className="font-medium text-[#E0702B] flex items-center gap-1">
                    {review.authorName}
                    {review.verified && <CheckCircle2 className="size-3" />}
                </span>
                <span className="text-muted-foreground">
                    {new Date(review.createdAt).toLocaleDateString('pt-BR')}
                </span>
            </div>
        </div>
    );
}
