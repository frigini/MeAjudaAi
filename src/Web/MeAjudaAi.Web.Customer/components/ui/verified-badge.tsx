import { BadgeCheck, AlertTriangle, Ban } from "lucide-react";
import { cn } from "@/lib/utils";

export type VerificationStatus = "Pending" | "Verified" | "Rejected" | "Suspended";

interface VerifiedBadgeProps {
    status: string; // Using string to accommodate API response
    className?: string;
    showLabel?: boolean;
    size?: "sm" | "md" | "lg";
}

export function VerifiedBadge({ status, className, showLabel = false, size = "md" }: VerifiedBadgeProps) {
    if (!status || status === "Pending") return null;

    const iconSize = size === "sm" ? 14 : size === "md" ? 18 : 24;

    if (status === "Verified") {
        return (
            <div className={cn("flex items-center gap-1 text-blue-500", className)} title="Prestador Verificado">
                <BadgeCheck size={iconSize} fill="currentColor" className="text-white bg-blue-500 rounded-full" />
                {showLabel && <span className="font-medium text-sm">Verificado</span>}
            </div>
        );
    }


    if (status === "Suspended") {
        return (
            <div className={cn("flex items-center gap-1 text-red-500", className)} title="Prestador Suspenso">
                <Ban size={iconSize} />
                {showLabel && <span className="font-medium text-sm">Suspenso</span>}
            </div>
        );
    }

    return null;
}
