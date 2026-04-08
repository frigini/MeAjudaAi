import { Star, ArrowDownAZ } from "lucide-react";

interface Review {
  id: string;
  rating: number;
  text: string;
  author: string;
  date: string;
}

interface ProfileReviewsProps {
  reviews: Review[];
}

export function ProfileReviews({ reviews }: ProfileReviewsProps) {
  return (
    <div className="mt-12 border-t border-border pt-8">
      <div className="mb-6 flex items-center gap-2">
        <h2 className="text-base font-bold text-foreground">Minhas avaliações</h2>
        <button className="text-foreground-subtle hover:text-foreground">
          <ArrowDownAZ className="h-4 w-4" />
          <span className="sr-only">Ordenar avaliações</span>
        </button>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {reviews.map((review) => (
          <div key={review.id} className="rounded-lg border border-border bg-surface p-6 shadow-sm">
            <div className="mb-3 flex text-primary">
              {Array.from({ length: 5 }).map((_, i) => (
                <Star
                  key={i}
                  data-testid="star-icon"
                  className={`h-4 w-4 ${
                    i < review.rating ? "fill-current text-primary" : "text-muted-foreground"
                  }`}
                />
              ))}
            </div>
            <p className="mb-4 text-xs leading-relaxed text-foreground-subtle">
              {review.text}
            </p>
            <div className="flex flex-col items-end text-xs">
              <span className="font-semibold text-primary">{review.author}</span>
              <span className="text-muted-foreground">{review.date}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
