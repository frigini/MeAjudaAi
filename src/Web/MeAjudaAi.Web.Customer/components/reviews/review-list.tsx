"use client"

import { useState } from "react";
import { Review, ReviewCard } from "./review-card";
import { Button } from "@/components/ui/button";
import { ArrowUpDown } from "lucide-react";


// Mock data generator
const generateMockReviews = (count: number): Review[] => {
    return Array.from({ length: count }).map((_, i) => ({
        id: `review-${i}`,
        authorName: `Usuário ${i + 1}`,
        rating: 4 + (i % 2), // 4 or 5 stars mostly
        comment: "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
        verified: i % 3 === 0, // Mock verification status
        createdAt: new Date(Date.now() - i * 86400000 * 5), // spaced by 5 days
    }));
};

export function ReviewList() {
    const [reviews, setReviews] = useState<Review[]>(generateMockReviews(4));

    const loadMore = () => {
        // Mock loading more
        // TODO: Mudar para API real e usar providerId
        setReviews(prev => {
            const newReviews = generateMockReviews(4).map(r => ({
                ...r,
                id: `review-${prev.length + parseInt(r.id.split('-')[1])}`
            }));
            return [...prev, ...newReviews];
        });
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center gap-2 mb-4">
                <h3 className="text-xl font-bold">Comentários</h3>
                <Button variant="ghost" size="sm" className="h-8 w-8 p-0" onClick={() => setReviews([...reviews].reverse())}>
                    <ArrowUpDown className="h-4 w-4" />
                    <span className="sr-only">Ordenar comentários</span>
                </Button>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
                {reviews.map((review) => (
                    <ReviewCard key={review.id} review={review} />
                ))}
            </div>

            <div className="text-center mt-8">
                <Button variant="outline" onClick={loadMore}>
                    Carregar mais
                </Button>
            </div>
        </div>
    );
}
