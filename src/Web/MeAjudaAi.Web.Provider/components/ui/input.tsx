import { twMerge } from "tailwind-merge";
import { type ComponentProps, forwardRef } from "react";

export interface InputProps extends ComponentProps<"input"> {
  error?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, error, type, ...props }, ref) => {
    return (
      <input
        type={type}
        className={twMerge(
          "flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm text-foreground shadow-sm transition-colors",
          "file:border-0 file:bg-transparent file:text-sm file:font-medium",
          "placeholder:text-muted-foreground",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
          "disabled:cursor-not-allowed disabled:opacity-50",
          error && "border-destructive focus-visible:ring-destructive",
          className
        )}
        ref={ref}
        {...props}
      />
    );
  }
);
Input.displayName = "Input";
