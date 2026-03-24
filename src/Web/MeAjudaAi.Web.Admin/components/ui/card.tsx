import { twMerge } from "tailwind-merge";
import type { ComponentProps } from "react";

export interface CardProps extends ComponentProps<"div"> {}

export function Card({ className, ...props }: CardProps) {
  return (
    <div
      data-slot="card"
      className={twMerge(
        "rounded-xl border border-border bg-surface p-6 shadow-sm",
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
      className={twMerge("text-lg font-semibold", className)}
      {...props}
    />
  );
}

export function CardContent({ className, ...props }: ComponentProps<"div">) {
  return (
    <div
      data-slot="card-content"
      className={twMerge("pt-2", className)}
      {...props}
    />
  );
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
