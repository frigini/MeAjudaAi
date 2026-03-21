"use client";

import { useState } from "react";
import Link from "next/link";
import { Search, Plus, Eye, CheckCircle, XCircle, Trash2, Loader2, ChevronLeft, ChevronRight } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { useProviders } from "@/hooks/admin";
import {
  providerTypeLabels,
  verificationStatusLabels,
  type ProviderDto,
  type VerificationStatus,
} from "@/lib/types";

const ITEMS_PER_PAGE = 10;

const getVerificationBadgeVariant = (status?: VerificationStatus) => {
  switch (status) {
    case 2: return "success" as const;
    case 0: return "warning" as const;
    case 3:
    case 4: return "destructive" as const;
    case 5: return "warning" as const;
    default: return "secondary" as const;
  }
};

const getProviderCity = (provider: ProviderDto): string => {
  return provider.businessProfile?.primaryAddress?.city ?? "-";
};

const getProviderPhone = (provider: ProviderDto): string => {
  const phone = provider.businessProfile?.contactInfo?.phoneNumber;
  return phone ?? "-";
};

export default function ProvidersPage() {
  const { data, isLoading, error } = useProviders();
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const providers: any[] = (data as any)?.items ?? (data as any)?.value ?? (data as any) ?? [];

  const filteredProviders = providers.filter(
    (p: any) =>
      (p.name?.toLowerCase() ?? "").includes(search.toLowerCase()) ||
      (p.businessProfile?.contactInfo?.email?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const totalPages = Math.ceil(filteredProviders.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const paginatedProviders = filteredProviders.slice(startIndex, startIndex + ITEMS_PER_PAGE);

  const handleSearch = (value: string) => {
    setSearch(value);
    setCurrentPage(1);
  };

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Prestadores</h1>
          <p className="text-muted-foreground">Gerencie os prestadores de serviços</p>
        </div>
        <Button disabled aria-disabled="true" className="opacity-50 cursor-not-allowed">
          <Plus className="mr-2 h-4 w-4" />Novo Prestador
        </Button>
      </div>

      <Card className="mb-6">
        <div className="p-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="search-providers"
              aria-label="Buscar por nome ou email"
              placeholder="Buscar por nome ou email..."
              className="pl-10"
              value={search}
              onChange={(e) => handleSearch(e.target.value)}
            />
          </div>
        </div>
      </Card>

      <Card>
        {isLoading && (
          <div className="flex items-center justify-center p-8">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        )}

        {error && (
          <div className="p-8 text-center text-destructive">
            Erro ao carregar prestadores. Tente novamente.
          </div>
        )}

        {!isLoading && !error && (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-border">
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Nome</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Email</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Telefone</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Tipo</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Status</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Cidade</th>
                    <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedProviders.map((provider: any, index: any) => (
                    <tr key={provider.id ?? provider.businessProfile?.contactInfo?.email ?? `provider-${index}`} className="border-b border-border last:border-b-0">
                      <td className="px-4 py-3 text-sm font-medium">
                        <Link href={`/providers/${provider.id}`} className="hover:underline">
                          {provider.name ?? "-"}
                        </Link>
                      </td>
                      <td className="px-4 py-3 text-sm text-muted-foreground">
                        {provider.businessProfile?.contactInfo?.email ?? "-"}
                      </td>
                      <td className="px-4 py-3 text-sm text-muted-foreground">{getProviderPhone(provider)}</td>
                      <td className="px-4 py-3 text-sm text-muted-foreground">
                        {providerTypeLabels[provider.type as keyof typeof providerTypeLabels] ?? "-"}
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant={getVerificationBadgeVariant(provider.verificationStatus)}>
                          {verificationStatusLabels[provider.verificationStatus as keyof typeof verificationStatusLabels] ?? "-"}
                        </Badge>
                      </td>
                      <td className="px-4 py-3 text-sm text-muted-foreground">{getProviderCity(provider)}</td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-2">
                          <Link href={`/providers/${provider.id}`}>
                            <Button variant="ghost" size="icon" aria-label="Visualizar" title="Visualizar"><Eye className="h-4 w-4" /></Button>
                          </Link>

                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t border-border px-4 py-3">
                <p className="text-sm text-muted-foreground">
                  Mostrando {startIndex + 1} - {Math.min(startIndex + ITEMS_PER_PAGE, filteredProviders.length)} de {filteredProviders.length}
                </p>
                <div className="flex items-center gap-2">
                  <Button
                    variant="secondary"
                    size="icon"
                    disabled={currentPage === 1}
                    onClick={() => setCurrentPage((p) => p - 1)}
                    aria-label="Página anterior"
                  >
                    <ChevronLeft className="h-4 w-4" />
                  </Button>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (currentPage <= 3) {
                      pageNum = i + 1;
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = currentPage - 2 + i;
                    }
                    return (
                      <Button
                        key={pageNum}
                        variant={currentPage === pageNum ? "primary" : "secondary"}
                        size="icon"
                        onClick={() => setCurrentPage(pageNum)}
                        aria-label={`Página ${pageNum}`}
                      >
                        {pageNum}
                      </Button>
                    );
                  })}
                  <Button
                    variant="secondary"
                    size="icon"
                    disabled={currentPage === totalPages}
                    onClick={() => setCurrentPage((p) => p + 1)}
                    aria-label="Próxima página"
                  >
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
          </>
        )}

        {!isLoading && !error && filteredProviders.length === 0 && (
          <div className="p-8 text-center text-muted-foreground">Nenhum prestador encontrado</div>
        )}
      </Card>
    </div>
  );
}
