import { tv, type VariantProps } from "tailwind-variants";
import { twMerge } from "tailwind-merge";
import type { ComponentProps } from "react";

export const badgeVariants = tv({
    base: "inline-flex items-center rounded-md px-2.5 py-0.5 text-xs font-medium transition-colors",
    variants: {
        variant: {
            default: "bg-surface-raised text-foreground border border-border",
            primary: "bg-primary/10 text-primary border border-primary/20",
            secondary: "bg-secondary/10 text-secondary border border-secondary/20",
            success: "bg-green-50 text-green-700 border border-green-200",
            warning: "bg-yellow-50 text-yellow-700 border border-yellow-200",
            destructive: "bg-red-50 text-red-700 border border-red-200",
        },
    },
    defaultVariants: {
        variant: "secondary",
    },
});

export interface BadgeProps
    extends ComponentProps<"span">,
    VariantProps<typeof badgeVariants> { }

export function Badge({ className, variant, ...props }: BadgeProps) {
    return (
        <span
            data-slot="badge"
            className={twMerge(badgeVariants({ variant }), className)}
            {...props}
        />
    );
}
