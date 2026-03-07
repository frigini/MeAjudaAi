"use client";

import * as React from "react";
import { Check, ChevronsUpDown, Search } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";

interface ServiceDto {
    serviceId: string;
    name: string;
    categoryId?: string;
    category?: { name?: string }; // Adjust based on API response structure if needed
}

interface ServiceSelectorProps {
    value?: string;
    onSelect: (serviceId: string) => void;
    disabled?: boolean;
}

export function ServiceSelector({ value, onSelect, disabled }: ServiceSelectorProps) {
    const [open, setOpen] = React.useState(false);
    const [services, setServices] = React.useState<ServiceDto[]>([]);
    const [loading, setLoading] = React.useState(false);
    const [search, setSearch] = React.useState("");
    const [error, setError] = React.useState<string | null>(null);

    React.useEffect(() => {
        // Only fetch when opening for the first time or reuse
        // But for simplicity, fetch on mount
        const fetchServices = async () => {
            setLoading(true);
            setError(null);
            try {
                // Fetch active services
                const res = await fetch("/api/v1/service-catalogs/services?activeOnly=true");
                if (res.ok) {
                    const data = await res.json();
                    setServices(data);
                } else {
                    setError("Failed to load services");
                }
            } catch (error) {
                console.error("Failed to fetch services", error);
                setError("Error loading services");
            } finally {
                setLoading(false);
            }
        };
        fetchServices();
    }, []);

    const filteredServices = services.filter(service =>
        service.name.toLowerCase().includes(search.toLowerCase())
    );

    const selectedService = services.find(s => s.serviceId === value);

    const handleSelect = (id: string) => {
        onSelect(id);
        setOpen(false);
        setSearch("");
    };

    return (
        <DropdownMenu open={open} onOpenChange={setOpen}>
            <DropdownMenuTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={open}
                    className="w-full justify-between"
                    disabled={disabled || loading}
                >
                    {value ? (selectedService?.name || "Serviço selecionado") : "Selecionar serviço..."}
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-[300px] p-0" align="start">
                <div className="flex items-center border-b px-3">
                    <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
                    <Input
                        placeholder="Buscar serviço..."
                        className="flex h-11 w-full rounded-md bg-transparent py-3 text-sm outline-none placeholder:text-muted-foreground border-none focus-visible:ring-0 shadow-none"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        // Prevent dropdown from handling keys meant for input
                        onKeyDown={(e) => e.stopPropagation()}
                    />
                </div>
                <div className="max-h-[300px] overflow-y-auto p-1">
                    {loading && (
                        <div className="py-6 text-center text-sm text-muted-foreground">
                            Carregando...
                        </div>
                    )}
                    {error && (
                        <div className="py-6 text-center text-sm text-red-500">
                            {error}
                        </div>
                    )}
                    {!loading && !error && filteredServices.length === 0 && (
                        <div className="py-6 text-center text-sm text-muted-foreground">
                            Nenhum serviço encontrado.
                        </div>
                    )}
                    {!loading && !error && filteredServices.map((service) => (
                        <DropdownMenuItem
                            key={service.serviceId}
                            onSelect={() => handleSelect(service.serviceId)}
                        >
                            <Check
                                className={cn(
                                    "mr-2 h-4 w-4",
                                    value === service.serviceId ? "opacity-100" : "opacity-0"
                                )}
                            />
                            <div className="flex flex-col">
                                <span>{service.name}</span>
                                {service.category && (
                                    <span className="text-xs text-muted-foreground">{service.category.name}</span>
                                )}
                            </div>
                        </DropdownMenuItem>
                    ))}
                </div>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
