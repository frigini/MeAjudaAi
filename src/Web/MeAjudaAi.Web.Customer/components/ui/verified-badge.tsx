import { BadgeCheck, Ban } from "lucide-react";
import { cn } from "@/lib/utils";
import { VerificationStatus } from "@/types/api/provider";

interface VerifiedBadgeProps {
    status: VerificationStatus;
    className?: string;
    showLabel?: boolean;
    size?: "sm" | "md" | "lg";
}

export function VerifiedBadge({ status, className, showLabel = false, size = "md" }: VerifiedBadgeProps) {
    if (!status) return null;

    const iconSize = size === "sm" ? 14 : size === "md" ? 18 : 24;

    if (status === "Verified") {
        return (
            <div className={cn("flex items-center gap-1 text-blue-500", className)} title="Prestador Verificado">
                <BadgeCheck size={iconSize} fill="currentColor" className="text-white" />
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

    if (status === "Pending") {
        return (
            <div className={cn("flex items-center gap-1 text-yellow-600", className)} title="Pendente de Verificação">
                <BadgeCheck size={iconSize} className="text-yellow-600" />
                {showLabel && <span className="font-medium text-sm">Pendente</span>}
            </div>
        );
    }

    return null;
}
