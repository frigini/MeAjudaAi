import { ProviderCard } from "./provider-card";
import type { ProviderDto } from "@/types/api/provider";

export interface ProviderGridProps {
    providers: ProviderDto[];
    emptyMessage?: string;
}

export function ProviderGrid({
    providers,
    emptyMessage = "Nenhum prestador encontrado.",
}: ProviderGridProps) {
    if (providers.length === 0) {
        return (
            <div className="text-center py-12">
                <p className="text-foreground-subtle text-lg">{emptyMessage}</p>
            </div>
        );
    }

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {providers.map((provider) => (
                <ProviderCard key={provider.id} provider={provider} />
            ))}
        </div>
    );
}
