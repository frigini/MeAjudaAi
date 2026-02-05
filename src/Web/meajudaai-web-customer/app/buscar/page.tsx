import { Search } from "lucide-react";
import { ProviderGrid } from "@/components/providers/provider-grid";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import type { ProviderDto } from "@/types/api/provider";

// Mock data for demonstration (will be replaced with API call)
const mockProviders: ProviderDto[] = [
    {
        id: "1",
        name: "José Silva",
        email: "jose@example.com",
        avatarUrl: null,
        averageRating: 4.5,
        reviewCount: 12,
        services: [
            { id: "1", name: "Pedreiro", category: "Construção" },
            { id: "2", name: "Pintura", category: "Construção" },
        ],
        city: "São Paulo",
        state: "SP",
        providerType: "Individual",
    },
    {
        id: "2",
        name: "Maria Santos",
        email: "maria@example.com",
        avatarUrl: null,
        averageRating: 5.0,
        reviewCount: 8,
        services: [
            { id: "3", name: "Eletricista", category: "Elétrica" },
        ],
        city: "Rio de Janeiro",
        state: "RJ",
        providerType: "Individual",
    },
    {
        id: "3",
        name: "TechFix Informática",
        email: "contato@techfix.com",
        avatarUrl: null,
        averageRating: 4.8,
        reviewCount: 25,
        services: [
            { id: "4", name: "Manutenção de Computadores", category: "Informática" },
            { id: "5", name: "Instalação de Redes", category: "Informática" },
            { id: "6", name: "Suporte Técnico", category: "Informática" },
        ],
        city: "Belo Horizonte",
        state: "MG",
        providerType: "Company",
    },
];

interface SearchPageProps {
    searchParams: {
        q?: string;
        city?: string;
    };
}

export default function SearchPage({ searchParams }: SearchPageProps) {
    const query = searchParams.q || "";
    const city = searchParams.city || "";

    // TODO: Replace with actual API call
    const providers = mockProviders;

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Search Header */}
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-foreground mb-6">
                    {query ? `Resultados para "${query}"` : "Buscar Prestadores"}
                </h1>

                {/* Search Form */}
                <div className="flex flex-col md:flex-row gap-4">
                    <div className="flex-1">
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-foreground-subtle" />
                            <input
                                type="search"
                                placeholder="Buscar serviço..."
                                defaultValue={query}
                                className="w-full pl-10 pr-4 py-3 border border-input rounded-lg focus:outline-none focus:ring-2 focus:ring-ring"
                            />
                        </div>
                    </div>
                    <div className="w-full md:w-64">
                        <input
                            type="text"
                            placeholder="Cidade..."
                            defaultValue={city}
                            className="w-full px-4 py-3 border border-input rounded-lg focus:outline-none focus:ring-2 focus:ring-ring"
                        />
                    </div>
                    <Button variant="primary" size="lg">
                        Buscar
                    </Button>
                </div>
            </div>

            {/* Results Count */}
            <div className="mb-6">
                <p className="text-foreground-subtle">
                    {providers.length}{" "}
                    {providers.length === 1 ? "prestador encontrado" : "prestadores encontrados"}
                </p>
            </div>

            {/* Provider Grid */}
            <ProviderGrid providers={providers} />
        </div>
    );
}
