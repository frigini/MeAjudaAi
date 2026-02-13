"use client";

import { cn } from "@/lib/utils";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { getPopularServices, ServiceListDto } from "@/lib/services/service-catalog";

export function ServiceTags() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const [tags, setTags] = useState<ServiceListDto[]>([]);

    useEffect(() => {
        getPopularServices().then(setTags);
    }, []);

    const currentQuery = searchParams.get("q") || "";

    const handleTagClick = (tagLabel: string) => {
        const params = new URLSearchParams(searchParams.toString());
        const isActive = currentQuery.toLowerCase() === tagLabel.toLowerCase();

        if (isActive) {
            params.delete("q"); // Deselect
        } else {
            params.set("q", tagLabel);
        }

        router.push(`/buscar?${params.toString()}`);
    };

    if (tags.length === 0) {
        return null;
    }

    return (
        <div className="w-full overflow-x-auto pb-4 -mx-4 px-4 md:mx-0 md:px-0 no-scrollbar">
            <div className="flex gap-3 min-w-max md:w-full md:justify-center">
                {tags.map((tag) => {
                    const isActive = currentQuery.toLowerCase() === tag.name?.toLowerCase();

                    return (
                        <button
                            key={tag.id}
                            onClick={() => handleTagClick(tag.name || "")}
                            className={cn(
                                "px-4 py-1.5 rounded-full text-xs font-medium transition-colors whitespace-nowrap",
                                isActive
                                    ? "bg-orange-200 text-orange-800 border border-orange-300"
                                    : "bg-orange-100 text-orange-600 hover:bg-orange-200 border border-transparent"
                            )}
                        >
                            {tag.name}
                        </button>
                    );
                })}
            </div>
        </div>
    );
}
