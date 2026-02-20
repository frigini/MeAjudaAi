import { twMerge } from "tailwind-merge";
import Image, { ImageProps } from "next/image";

export interface AvatarProps extends Omit<ImageProps, "src" | "alt" | "width" | "height" | "fill"> {
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

const sizePx: Record<NonNullable<AvatarProps["size"]>, number> = {
    sm: 32,
    md: 40,
    lg: 48,
    xl: 64,
};

export function Avatar({
    src,
    alt,
    size = "md",
    fallback,
    className,
    style, // Capture style for container
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
                sizeClasses[size],
                className
            )}
            style={style}
        >
            {src ? (
                <Image
                    src={src}
                    alt={alt}
                    width={sizePx[size]}
                    height={sizePx[size]}
                    className="size-full object-cover"
                    {...rest}
                />
            ) : (
                <span className="text-sm select-none">{initials}</span>
            )}
        </div>
    );
}
