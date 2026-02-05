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
      primary: "bg-secondary text-secondary-foreground hover:bg-secondary-hover",
      secondary: "bg-primary text-primary-foreground hover:bg-primary-hover",
      outline: "border-2 border-secondary text-secondary hover:bg-secondary/10",
      ghost: "text-foreground hover:bg-surface-raised",
      destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90",
    },
    size: {
      sm: "h-9 px-3 text-sm gap-1.5 [&_svg]:size-3.5",
      md: "h-11 px-4 text-base gap-2 [&_svg]:size-4",
      lg: "h-13 px-6 text-lg gap-2.5 [&_svg]:size-5",
    },
  },
  defaultVariants: { variant: "primary", size: "md" },
});

export interface ButtonProps
  extends ComponentProps<"button">,
    VariantProps<typeof buttonVariants> {}

export function Button({
  className,
  variant,
  size,
  disabled,
  children,
  ...props
}: ButtonProps) {
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
