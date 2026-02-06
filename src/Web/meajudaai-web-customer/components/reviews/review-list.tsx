"use client"

import { useState } from "react";
import { Review, ReviewCard } from "./review-card";
import { Button } from "@/components/ui/button";

interface ReviewListProps {
    providerId: string;
}

// Mock data generator
const generateMockReviews = (count: number): Review[] => {
    return Array.from({ length: count }).map((_, i) => ({
        id: `review-${i}`,
        authorName: `Usuário ${i + 1}`,
        rating: 4 + (i % 2), // 4 or 5 stars mostly
        comment: "Ótimo profissional! Recomendo muito o serviço. Atendeu todas as expectativas e foi super pontual.",
        createdAt: new Date(Date.now() - i * 86400000 * 5), // spaced by 5 days
    }));
};

export function ReviewList({ providerId }: ReviewListProps) {
    const [reviews, setReviews] = useState<Review[]>(generateMockReviews(3));
    const [page, setPage] = useState(1);

    const loadMore = () => {
        // Mock loading more
        const newReviews = generateMockReviews(3).map(r => ({
            ...r,
            id: `review-${reviews.length + parseInt(r.id.split('-')[1])}`
        }));
        setReviews([...reviews, ...newReviews]);
        setPage(page + 1);
    };

    return (
        <div className="space-y-6">
            {reviews.map((review) => (
                <ReviewCard key={review.id} review={review} />
            ))}

            <div className="text-center">
                <Button variant="outline" onClick={loadMore}>
                    Carregar mais avaliações
                </Button>
            </div>
        </div>
    );
}
