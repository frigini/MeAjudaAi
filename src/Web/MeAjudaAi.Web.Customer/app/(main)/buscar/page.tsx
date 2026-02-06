import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ProviderCard } from "@/components/providers/provider-card";
import { apiProvidersGet3 } from "@/lib/api/generated";
import { mapSearchableProviderToProvider } from "@/lib/api/mappers";

import { geocodeCity } from "@/lib/services/geocoding";

interface SearchPageProps {
    searchParams: Promise<{
        q?: string;
        city?: string;
    }>;
}

export default async function SearchPage({ searchParams }: SearchPageProps) {
    const { q, city } = await searchParams;
    const searchQuery = q || "";
    const cityFilter = city || "";

    // Default coordinates (São Paulo)
    let latitude = -23.5505;
    let longitude = -46.6333;
    const defaultRadius = 50; // 50km radius

    // Attempt to geocode if city is provided
    if (cityFilter) {
        const location = await geocodeCity(cityFilter);
        if (location) {
            latitude = location.latitude;
            longitude = location.longitude;
        } else {
            console.warn(`Could not geocode '${cityFilter}', using default location.`);
        }
    }

    // Fetch providers from API using dynamic or default coordinates
    const { data, error } = await apiProvidersGet3({
        query: {
            latitude,
            longitude,
            radiusInKm: defaultRadius,
            page: 1,
            pageSize: 20,
            // TODO: Map searchQuery (text) to serviceIds if possible, or request API update to support text query
        },
    });

    if (error) {
        console.error("SearchPage: Failed to fetch providers", error);
        throw new Error('Failed to fetch providers. Please try again later.');
    }

    // Map API DTOs to application types
    const providers = (data?.items || []).map(mapSearchableProviderToProvider);

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Search Header */}
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-foreground mb-6">
                    {searchQuery ? `Resultados para "${searchQuery}"` : "Buscar Prestadores"}
                </h1>

                {/* Search Form */}
                <form action="/buscar" method="get" role="search" aria-label="Buscar prestadores" className="flex flex-col md:flex-row gap-4">
                    <div className="flex-1">
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-foreground-subtle" />
                            <input
                                name="q"
                                type="search"
                                placeholder="Buscar serviço..."
                                defaultValue={searchQuery}
                                className="w-full pl-10 pr-4 py-3 border border-input rounded-lg focus:outline-none focus:ring-2 focus:ring-ring"
                            />
                        </div>
                    </div>
                    <div className="w-full md:w-64">
                        <input
                            name="city"
                            type="text"
                            placeholder="Cidade..."
                            defaultValue={cityFilter}
                            className="w-full px-4 py-3 border border-input rounded-lg focus:outline-none focus:ring-2 focus:ring-ring"
                        />
                    </div>
                    <Button variant="primary" size="lg" type="submit">
                        Buscar
                    </Button>
                </form>
            </div>

            {/* Results Count */}
            <div className="mb-6">
                <p className="text-foreground-subtle">
                    {providers.length} prestador{providers.length !== 1 ? 'es' : ''} encontrado{providers.length !== 1 ? 's' : ''}
                    {cityFilter && ` em ${cityFilter}`}
                </p>
            </div>

            {/* Provider Grid */}
            {providers.length > 0 ? (
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {providers.map((provider, index) => (
                        <ProviderCard
                            key={`${provider.id || 'provider'}-${index}`}
                            provider={provider}
                        />
                    ))}
                </div>
            ) : (
                <div className="text-center py-16">
                    <Search className="mx-auto mb-4 h-16 w-16 text-foreground-subtle" />
                    <h3 className="text-xl font-semibold text-foreground mb-2">
                        Nenhum prestador encontrado
                    </h3>
                    <p className="text-foreground-subtle">
                        Tente ajustar os filtros de busca ou buscar por outro serviço.
                    </p>
                </div>
            )}
        </div>
    );
}
