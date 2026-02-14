"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiCategoriesGet } from "@/lib/api/generated/sdk.gen";
import { MeAjudaAiModulesServiceCatalogsApplicationDtosServiceCategoryDto as ServiceCategoryDto } from "@/lib/api/generated/types.gen";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

export function SearchFilters() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const [categories, setCategories] = useState<ServiceCategoryDto[]>([]);

    // Default values
    const currentRadius = searchParams.get("radiusInKm") || "50";
    const [radiusDraft, setRadiusDraft] = useState(currentRadius);
    const currentMinRating = searchParams.get("minRating") || "";
    const activeCategory = searchParams.get("categoryId");

    useEffect(() => {
        setRadiusDraft(currentRadius);
    }, [currentRadius]);

    useEffect(() => {
        apiCategoriesGet({ query: { activeOnly: true } })
            .then(res => setCategories(res.data?.data || []))
            .catch(err => {
                console.error("Failed to load categories:", err);
                setCategories([]);
            });
    }, []);

    const updateFilter = (key: string, value: string | null) => {
        const params = new URLSearchParams(searchParams.toString());
        if (value != null) {
            params.set(key, value);
        } else {
            params.delete(key);
        }

        // Reset page when filtering
        params.delete("page");

        router.push(`/buscar?${params.toString()}`);
    };

    return (
        <div className="space-y-8 bg-white p-5 rounded-xl border border-gray-100 shadow-sm sticky top-24">
            <div>
                <div className="flex justify-between items-center mb-3">
                    <h3 className="font-semibold text-sm uppercase text-gray-500 tracking-wider">Distância</h3>
                    <span className="text-sm font-medium text-orange-600">{radiusDraft} km</span>
                </div>
                <div className="space-y-4">
                    <input
                        type="range"
                        min="5"
                        max="100"
                        step="5"
                        value={radiusDraft}
                        className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-orange-600 hover:accent-orange-700 transition-all"
                        onChange={(e) => setRadiusDraft(e.target.value)}
                        onMouseUp={(e) => updateFilter("radiusInKm", (e.target as HTMLInputElement).value)}
                        onTouchEnd={(e) => updateFilter("radiusInKm", (e.target as HTMLInputElement).value)}
                        onKeyUp={(e) => updateFilter("radiusInKm", (e.target as HTMLInputElement).value)}
                    />
                    <div className="flex justify-between text-xs text-gray-400 font-medium">
                        <span>5km</span>
                        <span>100km</span>
                    </div>
                </div>
            </div>

            <div className="border-t border-gray-100 pt-6">
                <h3 className="font-semibold mb-3 text-sm uppercase text-gray-500 tracking-wider">Avaliação</h3>
                <div className="space-y-3">
                    <div className="flex items-center space-x-2">
                        <input
                            type="radio"
                            id="r-all"
                            name="rating"
                            checked={!currentMinRating}
                            onChange={() => updateFilter("minRating", null)}
                            className="text-orange-600 focus:ring-orange-500 border-gray-300"
                        />
                        <Label htmlFor="r-all" className={cn("cursor-pointer", !currentMinRating && "font-medium text-orange-900")}>Qualquer avaliação</Label>
                    </div>
                    {[4, 3].map((rating) => (
                        <div key={rating} className="flex items-center space-x-2">
                            <input
                                type="radio"
                                id={`r-${rating}`}
                                name="rating"
                                checked={currentMinRating === rating.toString()}
                                onChange={() => updateFilter("minRating", rating.toString())}
                                className="text-orange-600 focus:ring-orange-500 border-gray-300"
                            />
                            <Label htmlFor={`r-${rating}`} className={cn("cursor-pointer flex items-center gap-1", currentMinRating === rating.toString() && "font-medium text-orange-900")}>
                                {rating}+
                                <span className="text-yellow-400">★</span>
                            </Label>
                        </div>
                    ))}
                </div>
            </div>

            <div className="border-t border-gray-100 pt-6">
                <h3 className="font-semibold mb-3 text-sm uppercase text-gray-500 tracking-wider">Categorias</h3>
                <div className="space-y-2 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                    <div className="flex items-center space-x-2">
                        <input
                            type="radio"
                            id="c-all"
                            name="category"
                            checked={!activeCategory}
                            onChange={() => updateFilter("categoryId", null)}
                            className="text-orange-600 focus:ring-orange-500 border-gray-300"
                        />
                        <Label htmlFor="c-all" className={cn("cursor-pointer text-sm", !activeCategory && "font-medium text-orange-900")}>Todas</Label>
                    </div>
                    {categories.map(cat => (
                        <div key={cat.id} className="flex items-center space-x-2 hover:bg-orange-50 p-1 rounded transition-colors -ml-1">
                            <input
                                type="radio"
                                id={`c-${cat.id}`}
                                name="category"
                                checked={activeCategory === cat.id}
                                onChange={() => updateFilter("categoryId", cat.id || "")}
                                className="text-orange-600 focus:ring-orange-500 border-gray-300"
                            />
                            <Label htmlFor={`c-${cat.id}`} className={cn("cursor-pointer text-sm flex-1", activeCategory === cat.id && "font-medium text-orange-900")}>{cat.name}</Label>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
