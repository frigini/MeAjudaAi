"use client"

import { Star } from "lucide-react"
import { cn } from "@/lib/utils"
import { useState } from "react"

interface RatingProps {
    value: number
    max?: number
    onChange?: (value: number) => void
    readOnly?: boolean
    className?: string
    size?: "sm" | "md" | "lg"
}

const sizeClasses = {
    sm: "w-3 h-3",
    md: "w-4 h-4",
    lg: "w-6 h-6",
}

export function Rating({
    value,
    max = 5,
    onChange,
    readOnly = false,
    className,
    size = "md",
}: RatingProps) {
    const [hoverValue, setHoverValue] = useState<number | null>(null)

    return (
        <div
            className={cn("flex items-center gap-0.5", className)}
            role={readOnly ? "img" : undefined}
            aria-label={readOnly ? `${value} de ${max} estrelas` : undefined}
        >
            {Array.from({ length: max }).map((_, i) => {
                const index = i + 1
                const effectiveValue = hoverValue ?? value
                const isFilled = effectiveValue >= index
                const isHalfFilled = effectiveValue >= index - 0.5 && effectiveValue < index

                return (
                    <span
                        key={index}
                        className={cn(
                            "transition-colors focus:outline-none focus:ring-1 focus:ring-ring rounded-sm",
                            readOnly ? "cursor-default" : "cursor-pointer"
                        )}
                        onMouseEnter={() => !readOnly && setHoverValue(index)}
                        onMouseLeave={() => !readOnly && setHoverValue(null)}
                        onClick={() => !readOnly && onChange?.(index)}
                        role={readOnly ? "presentation" : "button"}
                        aria-label={!readOnly ? `${index} de ${max} estrelas` : undefined}
                    >
                        <Star
                            className={cn(
                                sizeClasses[size],
                                "transition-all",
                                isFilled ? "fill-orange-500 text-orange-500" :
                                    isHalfFilled ? "fill-orange-500 text-orange-500" : // Lucide Star doesn't support half-fill visually, so treating half as full for now
                                        "fill-transparent text-gray-300",
                            )}
                        />
                    </span>
                )
            })}
        </div>
    )
}
