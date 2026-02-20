import { twMerge } from "tailwind-merge";
import Image, { ImageProps } from "next/image";
import type { CSSProperties } from "react";

export interface AvatarProps extends Omit<ImageProps, "src" | "alt" | "width" | "height" | "fill" | "style"> {
    src?: string | null;
    alt: string;
    size?: "sm" | "md" | "lg" | "xl";
    fallback?: string;
    containerStyle?: CSSProperties;
}

const SIZE_CONFIG = {
    sm: { classes: "size-8", px: 32 },
    md: { classes: "size-10", px: 40 },
    lg: { classes: "size-12", px: 48 },
    xl: { classes: "size-16", px: 64 },
} as const;

export function Avatar({
    src,
    alt,
    size = "md",
    fallback,
    className,
    containerStyle,
    ...rest // Forward remaining props (img props) to Image
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
                SIZE_CONFIG[size].classes,
                className
            )}
            style={containerStyle}
        >
            {src ? (
                <Image
                    src={src}
                    alt={alt}
                    width={SIZE_CONFIG[size].px}
                    height={SIZE_CONFIG[size].px}
                    className="size-full object-cover"
                    {...rest}
                />
            ) : (
                <span className="text-sm select-none">{initials}</span>
            )}
        </div>
    );
}
