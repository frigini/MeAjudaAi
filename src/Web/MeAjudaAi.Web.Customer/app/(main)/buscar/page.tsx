"use client";

import { Suspense, useMemo } from "react";
import { Search, Loader2 } from "lucide-react";
import { useSearchParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { ServiceCard } from "@/components/service/service-card";
import { AdCard } from "@/components/search/ad-card";
import { ServiceTags } from "@/components/search/service-tags";
import { SearchFilters } from "@/components/search/search-filters";

import { apiProvidersGet4, apiCategoryGet } from "@/lib/api/generated/sdk.gen";
import type { ApiProvidersGet4Data } from "@/lib/api/generated";
import { mapSearchableProviderToProvider } from "@/lib/api/mappers";
import { geocodeCity } from "@/lib/services/geocoding";
import { ProviderDto } from "@/types/api/provider";

function SearchContent() {
    const searchParams = useSearchParams();
    
    const q = searchParams.get("q") || "";
    const city = searchParams.get("city") || "";
    const radiusInKm = searchParams.get("radiusInKm") || process.env.NEXT_PUBLIC_DEFAULT_RADIUS || "50";
    const minRating = searchParams.get("minRating");
    const categoryId = searchParams.get("categoryId");

    const radius = useMemo(() => {
        const val = parseFloat(radiusInKm);
        return Number.isNaN(val) ? 50 : val;
    }, [radiusInKm]);

    const minRatingVal = useMemo(() => {
        if (!minRating) return undefined;
        const val = parseFloat(minRating);
        return Number.isNaN(val) ? undefined : val;
    }, [minRating]);

    // Query for geocoding and provider search
    const { data: searchResults, isLoading, error } = useQuery({
        queryKey: ["search-providers", q, city, radius, minRatingVal, categoryId],
        queryFn: async () => {
            const DEFAULT_LAT = -19.3917;
            const DEFAULT_LNG = -40.0722;
            let latitude = parseFloat(process.env.NEXT_PUBLIC_DEFAULT_LAT || "");
            let longitude = parseFloat(process.env.NEXT_PUBLIC_DEFAULT_LNG || "");
            if (Number.isNaN(latitude)) latitude = DEFAULT_LAT;
            if (Number.isNaN(longitude)) longitude = DEFAULT_LNG;

            if (city) {
                const location = await geocodeCity(city);
                if (location) {
                    latitude = location.latitude;
                    longitude = location.longitude;
                }
            }

            let serviceIds: string[] | undefined = undefined;
            if (categoryId) {
                const { data: catServices } = await apiCategoryGet({
                    path: { categoryId },
                    query: { activeOnly: true }
                });
                if (catServices?.data) {
                    serviceIds = catServices.data
                        .map(s => s.id)
                        .filter((id): id is string => !!id);
                }
            }

            const { data, error } = await apiProvidersGet4({
                query: {
                    latitude,
                    longitude,
                    radiusInKm: radius,
                    term: q,
                    serviceIds,
                    minRating: minRatingVal,
                    page: 1,
                    pageSize: 20,
                } as ApiProvidersGet4Data["query"],
            });

            if (error) throw error;
            return data;
        }
    });

    // Map API DTOs to application types
    const providers = useMemo(() => {
        return (searchResults?.items || []).map(mapSearchableProviderToProvider);
    }, [searchResults]);

    // Insert AdCard logic
    const gridItems = useMemo(() => {
        type GridItem =
            | { type: 'provider'; data: ProviderDto }
            | { type: 'ad'; data: null };

        const items: GridItem[] = [];
        if (providers.length > 0) {
            if (providers.length > 1) {
                items.push({ type: 'provider', data: providers[0] });
                items.push({ type: 'ad', data: null });
                providers.slice(1).forEach(p => {
                    items.push({ type: 'provider', data: p });
                });
            } else {
                items.push({ type: 'provider', data: providers[0] });
            }
        }
        return items;
    }, [providers]);

    if (error) {
        return (
            <div className="container mx-auto px-4 py-16 text-center">
                <h3 className="text-xl font-semibold text-destructive mb-2">Erro na busca</h3>
                <p className="text-muted-foreground">Não foi possível carregar os prestadores. Tente novamente mais tarde.</p>
                <Button onClick={() => window.location.reload()} className="mt-4">Recarregar</Button>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8">
            {/* 1. Search Bar Centered */}
            <div className="max-w-3xl mx-auto mb-8">
                <form action="/buscar" method="get" role="search" className="relative flex items-center gap-2">
                    {city && <input name="city" type="hidden" value={city} />}
                    {radiusInKm && <input type="hidden" name="radiusInKm" value={radiusInKm} />}
                    {minRating && <input type="hidden" name="minRating" value={minRating} />}
                    {categoryId && <input type="hidden" name="categoryId" value={categoryId} />}

                    <div className="relative w-full">
                        <input
                            name="q"
                            type="search"
                            placeholder={city ? `Buscar em ${city}...` : "Buscar serviço"}
                            defaultValue={q}
                            className="w-full pl-6 pr-14 py-4 border border-orange-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-orange-500 text-lg placeholder:text-foreground-subtle"
                        />
                        {city && (
                            <div className="absolute right-14 top-1/2 -translate-y-1/2">
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-8 text-xs text-muted-foreground hover:text-destructive"
                                    asChild
                                >
                                    <a href={`/buscar?q=${encodeURIComponent(q)}`}>Limpar Cidade</a>
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

            <div className="flex flex-col lg:flex-row gap-8">
                {/* Sidebar Filters */}
                <aside className="w-full lg:w-64 shrink-0">
                    <SearchFilters />
                </aside>

                <main className="flex-1">
                    {/* 2. Service Tags - Full Width */}
                    <div className="mb-8">
                        <ServiceTags />
                    </div>

                    {/* 4. Provider Grid */}
                    {isLoading ? (
                        <div className="flex flex-col items-center justify-center py-20">
                            <Loader2 className="h-12 w-12 animate-spin text-primary mb-4" />
                            <p className="text-muted-foreground">Buscando os melhores profissionais...</p>
                        </div>
                    ) : providers.length > 0 ? (
                        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-3">
                            {gridItems.map((item, index) => {
                                if (item.type === 'ad') {
                                    return <AdCard key={`ad-${index}`} />;
                                }

                                const provider = item.data;
                                return (
                                    <ServiceCard
                                        key={provider.id}
                                        id={provider.id}
                                        name={provider.name}
                                        avatarUrl={provider.avatarUrl ?? undefined}
                                        description={provider.description || "Prestador de serviços disponível para te atender."}
                                        services={provider.services.map(s => s.serviceName).filter((s): s is string => !!s)}
                                        rating={provider.averageRating ?? 0}
                                        reviewCount={provider.reviewCount ?? 0}
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
                                Não encontramos prestadores para sua busca no momento{city ? ` em ${city}` : ""}.
                                Tente ajustar os filtros ou buscar por termos mais genéricos.
                            </p>
                        </div>
                    )}
                </main>
            </div>
        </div>
    );
}

export default function SearchPage() {
    return (
        <Suspense fallback={
            <div className="flex items-center justify-center min-h-screen">
                <Loader2 className="h-12 w-12 animate-spin text-primary" />
            </div>
        }>
            <SearchContent />
        </Suspense>
    );
}
