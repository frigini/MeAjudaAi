"use client"

import * as React from "react"
import { Check } from "lucide-react"

import { cn } from "@/lib/utils/cn"

export interface CheckboxProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, "onChange"> {
    onCheckedChange?: (checked: boolean) => void;
    // Radix allows 'indeterminate' but native doesn't easily, we'll ignore it for now or treat as false
    checked?: boolean;
}

const Checkbox = React.forwardRef<HTMLInputElement, CheckboxProps>(
    ({ className, onCheckedChange, checked, ...props }, ref) => {

        // Internal state if uncontrolled, but usually controlled by RHF

        const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
            onCheckedChange?.(e.target.checked);
        };

        return (
            <div className="relative flex items-center justify-center w-4 h-4">
                <input
                    type="checkbox"
                    ref={ref}
                    checked={checked}
                    onChange={handleChange}
                    className={cn(
                        "peer h-4 w-4 shrink-0 appearance-none rounded-sm border border-primary ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 checked:bg-primary checked:text-primary-foreground",
                        className
                    )}
                    {...props}
                />
                <Check
                    className={cn(
                        "pointer-events-none absolute h-3 w-3 text-primary-foreground hidden",
                        checked && "block"
                    )}
                />
            </div>
        )
    }
)
Checkbox.displayName = "Checkbox"

export { Checkbox }
