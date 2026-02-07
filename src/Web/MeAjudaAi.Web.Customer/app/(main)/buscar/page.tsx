import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ServiceCard } from "@/components/service/service-card";
import { AdCard } from "@/components/search/ad-card";
import { ServiceTags } from "@/components/search/service-tags";

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

    // Fetch providers from API
    const { data, error } = await apiProvidersGet3({
        query: {
            latitude,
            longitude,
            radiusInKm: defaultRadius,
            term: searchQuery,
            page: 1,
            pageSize: 20,
        } as any,
    });

    if (error) {
        console.error("SearchPage: Failed to fetch providers", error);
        throw new Error('Failed to fetch providers. Please try again later.');
    }

    // Map API DTOs to application types
    const providers = (data?.items || []).map(mapSearchableProviderToProvider);

    // Insert AdCard at index 1 (2nd position) if we have enough items
    const gridItems = [];
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
                <form action="/buscar" method="get" role="search" className="relative flex items-center">
                    <input
                        name="city"
                        type="hidden"
                        value={cityFilter}
                    />
                    <input
                        name="q"
                        type="search"
                        placeholder="Buscar serviço"
                        defaultValue={searchQuery}
                        className="w-full pl-6 pr-14 py-4 border border-orange-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-orange-500 text-lg placeholder:text-foreground-subtle"
                    />
                    <Button
                        type="submit"
                        size="icon"
                        className="absolute right-2 top-1/2 -translate-y-1/2 bg-[#E0702B] hover:bg-[#c56226] h-10 w-10 rounded-md"
                        aria-label="Buscar"
                    >
                        <Search className="h-5 w-5 text-white" />
                    </Button>
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

                        const provider = item.data!;
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
                        Não encontramos prestadores para "<strong>{searchQuery}</strong>"{cityFilter && ` em ${cityFilter}`}.
                        Tente buscar por termos mais genéricos como "Pedreiro" ou "Eletricista".
                    </p>
                </div>
            )}
        </div>
    );
}
