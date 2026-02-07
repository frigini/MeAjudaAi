"use client";

import { cn } from "@/lib/utils";
import { useRouter, useSearchParams } from "next/navigation";

// Mock tags for initial implementation - Top 10 most popular
// Ideally this should come from the API (Catalog Categories/Services) with a 'popularity' rank
const TAGS = [
    { id: "pedreiro", label: "Pedreiro" },
    { id: "marceneiro", label: "Marceneiro" },
    { id: "eletricista", label: "Eletricista" },
    { id: "faxineira", label: "Faxineira" },
    { id: "mudanca", label: "Mudança" },
    { id: "pintor", label: "Pintor" },
    { id: "encanador", label: "Encanador" },
    { id: "montador-moveis", label: "Montador de Móveis" },
    { id: "ar-condicionado", label: "Ar Condicionado" },
    { id: "frete", label: "Frete" },
];

export function ServiceTags() {
    const router = useRouter();
    const searchParams = useSearchParams();

    // In a real implementation, we might support multiple tags. 
    // For now, let's treat it as a single "q" param or a dedicated "tag" param.
    // The design shows single selection behavior or "quick filters".
    // I'll map it to the 'q' param for simplicity as per existing search logic.
    const currentQuery = searchParams.get("q") || "";

    const handleTagClick = (tag: string) => {
        const params = new URLSearchParams(searchParams.toString());

        if (currentQuery === tag) {
            params.delete("q"); // Deselect
        } else {
            params.set("q", tag);
        }

        // Preserve city if it exists
        const city = searchParams.get("city");
        if (city) {
            params.set("city", city);
        }

        router.push(`/buscar?${params.toString()}`);
    };

    return (
        <div className="w-full overflow-x-auto pb-4 -mx-4 px-4 md:mx-0 md:px-0 no-scrollbar">
            <div className="flex gap-3 min-w-max md:w-full md:justify-center">
                {TAGS.map((tag) => {
                    const isActive = currentQuery.toLowerCase() === tag.label.toLowerCase() || currentQuery === tag.id;

                    return (
                        <button
                            key={tag.id}
                            onClick={() => handleTagClick(tag.label)}
                            className={cn(
                                "px-4 py-1.5 rounded-full text-xs font-medium transition-colors whitespace-nowrap",
                                isActive
                                    ? "bg-orange-200 text-orange-800 border border-orange-300"
                                    : "bg-orange-100 text-orange-600 hover:bg-orange-200 border border-transparent"
                            )}
                        >
                            {tag.label}
                        </button>
                    );
                })}
            </div>
        </div>
    );
}
