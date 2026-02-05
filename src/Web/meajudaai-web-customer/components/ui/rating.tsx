import { Star } from "lucide-react";
import { twMerge } from "tailwind-merge";

export interface RatingProps {
    value: number;
    max?: number;
    size?: "sm" | "md" | "lg";
    showValue?: boolean;
    className?: string;
}

const sizeClasses = {
    sm: "size-3",
    md: "size-4",
    lg: "size-5",
};

export function Rating({
    value,
    max = 5,
    size = "md",
    showValue = false,
    className,
}: RatingProps) {
    const stars = Array.from({ length: max }, (_, i) => i + 1);
    const filled = Math.floor(value);

    return (
        <div className={twMerge("flex items-center gap-1", className)}>
            {stars.map((star) => (
                <Star
                    key={star}
                    className={twMerge(
                        sizeClasses[size],
                        star <= filled
                            ? "fill-secondary text-secondary"
                            : "text-gray-300"
                    )}
                />
            ))}
            {showValue && (
                <span className="ml-1 text-sm font-medium text-foreground-subtle">
                    {value.toFixed(1)}
                </span>
            )}
        </div>
    );
}
