import { tv, type VariantProps } from "tailwind-variants";
import { twMerge } from "tailwind-merge";
import type { ComponentProps } from "react";
import { useId } from "react";

export const inputVariants = tv({
    base: [
        "flex w-full rounded-lg border border-input bg-surface px-3 py-2 text-sm",
        "placeholder:text-foreground-subtle",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        "data-[disabled]:cursor-not-allowed data-[disabled]:opacity-50",
    ],
    variants: {
        variant: {
            default: "",
            error: "border-destructive focus-visible:ring-destructive",
        },
    },
    defaultVariants: {
        variant: "default",
    },
});

export interface InputProps
    extends ComponentProps<"input">,
    VariantProps<typeof inputVariants> {
    label?: string;
    error?: string;
}

export function Input({
    className,
    variant,
    label,
    error,
    disabled,
    id: providedId,
    ...props
}: InputProps) {
    const generatedId = useId();
    const id = providedId || generatedId;

    return (
        <div className="flex flex-col gap-1.5">
            {label && (
                <label htmlFor={id} className="text-sm font-medium text-foreground">{label}</label>
            )}
            <input
                id={id}
                data-slot="input"
                data-disabled={disabled ? "" : undefined}
                className={twMerge(
                    inputVariants({ variant: error ? "error" : variant }),
                    className
                )}
                disabled={disabled}
                {...props}
            />
            {error && <span className="text-xs text-destructive">{error}</span>}
        </div>
    );
}
