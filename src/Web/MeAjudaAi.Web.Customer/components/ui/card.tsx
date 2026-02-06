import { twMerge } from "tailwind-merge";
import { twMerge as merge } from "tailwind-merge";
import type { ComponentProps } from "react";

export const cardVariants = {
    base: "bg-surface flex flex-col rounded-xl border border-border shadow-sm",
    padding: {
        none: "",
        sm: "p-4",
        md: "p-6",
        lg: "p-8",
    },
};

export interface CardProps extends ComponentProps<"div"> {
    padding?: keyof typeof cardVariants.padding;
}

export function Card({ className, padding = "md", ...props }: CardProps) {
    return (
        <div
            data-slot="card"
            className={twMerge(
                cardVariants.base,
                cardVariants.padding[padding],
                className
            )}
            {...props}
        />
    );
}

export function CardHeader({ className, ...props }: ComponentProps<"div">) {
    return (
        <div
            data-slot="card-header"
            className={twMerge("flex flex-col gap-1.5", className)}
            {...props}
        />
    );
}

export function CardTitle({ className, ...props }: ComponentProps<"h3">) {
    return (
        <h3
            data-slot="card-title"
            className={twMerge("text-lg font-semibold text-foreground", className)}
            {...props}
        />
    );
}

export function CardContent({ className, ...props }: ComponentProps<"div">) {
    return <div data-slot="card-content" className={className} {...props} />;
}

export function CardDescription({ className, ...props }: ComponentProps<"p">) {
    return (
        <p
            data-slot="card-description"
            className={twMerge("text-sm text-muted-foreground", className)}
            {...props}
        />
    );
}
