"use client";

import { Search, MapPin } from "lucide-react";
import { Button } from "@/components/ui/button";

export function CitySearch() {
    return (
        <div className="bg-white p-1 rounded-lg shadow-lg flex items-center max-w-xl border-2 border-secondary w-full">
            <div className="pl-4 text-foreground-subtle">
                <MapPin className="h-5 w-5" />
            </div>
            <select
                className="flex-1 w-full p-3 bg-transparent border-none text-foreground focus:ring-0 focus:outline-none placeholder:text-foreground-subtle appearance-none cursor-pointer"
                aria-label="Selecionar cidade"
                defaultValue=""
            >
                <option value="" disabled>Selecionar cidade...</option>
                <option value="muriae-mg">Muriaé - MG</option>
                <option value="uba-mg">Ubá - MG</option>
                <option value="cataguases-mg">Cataguases - MG</option>
            </select>
            <Button size="md" className="h-10 w-10 p-0 bg-secondary hover:bg-secondary-hover text-white rounded-md shrink-0">
                <Search className="h-5 w-5" />
            </Button>
        </div>
    );
}
