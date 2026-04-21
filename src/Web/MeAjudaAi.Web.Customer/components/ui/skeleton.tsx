import { twMerge } from "tailwind-merge";
import type { ComponentProps } from "react";

export function Skeleton({ className, ...props }: ComponentProps<"div">) {
    return (
        <div
            data-slot="skeleton"
            className={twMerge("bg-muted animate-pulse rounded-md", className)}
            {...props}
        />
    );
}
