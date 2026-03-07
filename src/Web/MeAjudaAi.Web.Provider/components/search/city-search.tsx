"use client";

import { Search, MapPin } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import { useRouter } from "next/navigation";

export function CitySearch() {
    const router = useRouter();
    const [city, setCity] = useState("");

    const handleSearch = () => {
        if (city) {
            router.push(`/buscar?cidade=${city}`);
        }
    };

    return (
        <div className="bg-white p-1 rounded-lg shadow-lg flex items-center max-w-xl border-2 border-secondary w-full">
            <div className="pl-4 text-foreground-subtle">
                <MapPin className="h-5 w-5" />
            </div>
            <select
                className="flex-1 w-full p-3 bg-transparent border-none text-foreground focus:ring-0 focus:outline-none placeholder:text-foreground-subtle appearance-none cursor-pointer"
                aria-label="Selecionar cidade"
                value={city}
                onChange={(e) => setCity(e.target.value)}
            >
                <option value="" disabled>Selecionar cidade...</option>
                <option value="muriae-mg">Muriaé - MG</option>
                <option value="uba-mg">Ubá - MG</option>
                <option value="cataguases-mg">Cataguases - MG</option>
            </select>
            <Button
                size="md"
                className="h-10 w-10 p-0 bg-secondary hover:bg-secondary-hover text-white rounded-md shrink-0"
                onClick={handleSearch}
                disabled={!city}
                aria-label="Buscar prestadores"
            >
                <Search className="h-5 w-5" />
            </Button>
        </div>
    );
}
