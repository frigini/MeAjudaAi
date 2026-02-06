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

    const handleMouseEnter = (index: number) => {
        if (!readOnly) {
            setHoverValue(index)
        }
    }

    const handleMouseLeave = () => {
        if (!readOnly) {
            setHoverValue(null)
        }
    }

    const handleClick = (index: number) => {
        if (!readOnly && onChange) {
            onChange(index)
        }
    }

    return (
        <div className={cn("flex items-center gap-0.5", className)}>
            {Array.from({ length: max }).map((_, i) => {
                const index = i + 1
                const isFilled = (hoverValue !== null ? hoverValue : value) >= index

                return (
                    <button
                        key={i}
                        type="button"
                        className={cn(
                            "transition-colors focus:outline-none focus:ring-1 focus:ring-ring rounded-sm",
                            readOnly ? "cursor-default" : "cursor-pointer"
                        )}
                        onClick={() => handleClick(index)}
                        onMouseEnter={() => handleMouseEnter(index)}
                        onMouseLeave={handleMouseLeave}
                        disabled={readOnly}
                        aria-label={`${index} ${index === 1 ? 'estrela' : 'estrelas'}`}
                    >
                        <Star
                            className={cn(
                                sizeClasses[size],
                                isFilled ? "fill-primary text-primary" : "fill-none text-muted-foreground",
                                /* Apply partial fill logic if needed, but simple full stars for now */
                            )}
                        />
                    </button>
                )
            })}
        </div>
    )
}
