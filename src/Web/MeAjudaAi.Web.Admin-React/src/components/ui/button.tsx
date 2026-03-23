import { tv, type VariantProps } from "tailwind-variants";
import { twMerge } from "tailwind-merge";
import type { ComponentProps } from "react";

export const buttonVariants = tv({
  base: [
    "inline-flex cursor-pointer items-center justify-center font-medium rounded-lg transition-colors",
    "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
    "data-[disabled]:pointer-events-none data-[disabled]:opacity-50",
  ],
  variants: {
    variant: {
      primary: "bg-primary text-primary-foreground hover:bg-primary-hover",
      secondary: "bg-secondary text-secondary-foreground hover:bg-secondary-hover",
      ghost: "border-transparent bg-transparent text-muted-foreground hover:text-foreground hover:bg-muted",
      destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90",
    },
    size: {
      sm: "h-9 px-3 text-sm",
      md: "h-10 px-4 text-sm",
      lg: "h-11 px-6 text-base",
      icon: "h-10 w-10",
    },
  },
  defaultVariants: { variant: "primary", size: "md" },
});

export interface ButtonProps extends ComponentProps<"button">, VariantProps<typeof buttonVariants> {}

export function Button({ className, variant, size, disabled, children, ...props }: ButtonProps) {
  return (
    <button
      type="button"
      data-slot="button"
      data-disabled={disabled ? "" : undefined}
      className={twMerge(buttonVariants({ variant, size }), className)}
      disabled={disabled}
      {...props}
    >
      {children}
    </button>
  );
}
