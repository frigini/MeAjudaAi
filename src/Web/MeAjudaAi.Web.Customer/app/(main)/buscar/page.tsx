import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ServiceCard } from "@/components/service/service-card";
import { AdCard } from "@/components/search/ad-card";
import { ServiceTags } from "@/components/search/service-tags";

import { apiProvidersGet3 } from "@/lib/api/generated/sdk.gen";
import { mapSearchableProviderToProvider } from "@/lib/api/mappers";
import { geocodeCity } from "@/lib/services/geocoding";
import { getAuthHeaders } from "@/lib/api/auth-headers";
import { ProviderDto } from "@/types/api/provider";

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

    // Default coordinates (Linhares - ES) for development, with environment overrides
    let latitude = parseFloat(process.env.NEXT_PUBLIC_DEFAULT_LAT || "-19.3917");
    let longitude = parseFloat(process.env.NEXT_PUBLIC_DEFAULT_LNG || "-40.0722");
    const defaultRadius = parseInt(process.env.NEXT_PUBLIC_DEFAULT_RADIUS || "50");

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

    // Fetch providers from API
    const headers = await getAuthHeaders();
    const { data, error } = await apiProvidersGet3({
        query: {
            latitude,
            longitude,
            radiusInKm: defaultRadius,
            term: searchQuery,
            page: 1,
            pageSize: 20,
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } as any,
        headers,
    });

    if (error) {
        console.error("SearchPage: Failed to fetch providers", error);
        throw new Error('Failed to fetch providers. Please try again later.');
    }

    // Map API DTOs to application types
    const providers = (data?.items || []).map(mapSearchableProviderToProvider);

    // Insert AdCard at index 1 (2nd position) if we have enough items
    type GridItem =
        | { type: 'provider'; data: ProviderDto }
        | { type: 'ad'; data: null };

    const gridItems: GridItem[] = [];
    if (providers.length > 0) {
        gridItems.push({ type: 'provider', data: providers[0] });
        gridItems.push({ type: 'ad', data: null });
        providers.slice(1).forEach(p => {
            gridItems.push({ type: 'provider', data: p });
        });
    } else {
        // Even if no providers, show AdCard? Maybe not.
        // If empty, standard empty state.
    }

    return (
        <div className="container mx-auto px-4 py-8">
            {/* 1. Search Bar Centered */}
            <div className="max-w-3xl mx-auto mb-8">
                <form action="/buscar" method="get" role="search" className="relative flex items-center gap-2">
                    {/* Expose city input or allow clearing it */}
                    {cityFilter && (
                        <input
                            name="city"
                            type="hidden"
                            value={cityFilter}
                        />
                    )}

                    <div className="relative w-full">
                        <input
                            name="q"
                            type="search"
                            placeholder={cityFilter ? `Buscar em ${cityFilter}...` : "Buscar serviço"}
                            defaultValue={searchQuery}
                            className="w-full pl-6 pr-14 py-4 border border-orange-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-orange-500 text-lg placeholder:text-foreground-subtle"
                        />
                        {cityFilter && (
                            <div className="absolute right-14 top-1/2 -translate-y-1/2">
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-8 text-xs text-muted-foreground hover:text-destructive"
                                    asChild
                                >
                                    <a href={`/buscar?q=${searchQuery}`}>Limpar Cidade</a>
                                </Button>
                            </div>
                        )}
                        <Button
                            type="submit"
                            size="icon"
                            className="absolute right-2 top-1/2 -translate-y-1/2 bg-[#E0702B] hover:bg-[#c56226] h-10 w-10 rounded-md"
                            aria-label="Buscar"
                        >
                            <Search className="h-5 w-5 text-white" />
                        </Button>
                    </div>
                </form>


            </div>

            {/* 2. Service Tags - Full Width */}
            <div className="mb-8">
                <ServiceTags />
            </div>



            {/* 4. Provider Grid */}
            {providers.length > 0 ? (
                <div className="grid gap-6 sm:grid-cols-2">
                    {gridItems.map((item, index) => {
                        if (item.type === 'ad') {
                            return <AdCard key={`ad-${index}`} />;
                        }

                        const provider = item.data; // Type narrowed automatically
                        return (
                            <ServiceCard
                                key={provider.id}
                                id={provider.id}
                                name={provider.name}
                                avatarUrl={provider.avatarUrl ?? undefined}
                                description={provider.description || "Prestador de serviços disponível para te atender."}
                                services={provider.services.map(s => s.name)}
                                rating={provider.averageRating}
                                reviewCount={provider.reviewCount}
                            />
                        );
                    })}
                </div>
            ) : (
                <div className="text-center py-16 bg-gray-50 rounded-xl border border-dashed border-gray-200">
                    <Search className="mx-auto mb-4 h-16 w-16 text-gray-300" />
                    <h3 className="text-xl font-semibold text-foreground mb-2">
                        Nenhum prestador encontrado
                    </h3>
                    <p className="text-foreground-subtle max-w-md mx-auto">
                        Não encontramos prestadores para &quot;<strong>{searchQuery}</strong>&quot;{cityFilter && ` em ${cityFilter}`}.
                        Tente buscar por termos mais genéricos como &quot;Pedreiro&quot; ou &quot;Eletricista&quot;.
                    </p>
                </div>
            )}
        </div>
    );
}
