import { twMerge } from "tailwind-merge";
import type { ComponentProps } from "react";

export interface AvatarProps extends Omit<ComponentProps<"img">, "src"> {
    src?: string | null;
    alt: string;
    size?: "sm" | "md" | "lg" | "xl";
    fallback?: string;
}

const sizeClasses = {
    sm: "size-8",
    md: "size-10",
    lg: "size-12",
    xl: "size-16",
};

export function Avatar({
    src,
    alt,
    size = "md",
    fallback,
    className,
    ...props
}: AvatarProps) {
    const initials =
        fallback ||
        (alt.trim()
            ? alt
                .split(" ")
                .filter((n) => n.length > 0)
                .map((n) => n[0])
                .join("")
                .toUpperCase()
                .slice(0, 2)
            : "");

    return (
        <div
            className={twMerge(
                "relative inline-flex items-center justify-center rounded-full bg-primary text-primary-foreground font-medium overflow-hidden",
                sizeClasses[size],
                className
            )}
        >
            {src ? (
                <img
                    src={src}
                    alt={alt}
                    className="size-full object-cover"
                    {...props}
                />
            ) : (
                <span className="text-sm">{initials}</span>
            )}
        </div>
    );
}
