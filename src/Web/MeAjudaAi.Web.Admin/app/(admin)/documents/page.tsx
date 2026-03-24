"use client";
/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable react-hooks/exhaustive-deps */

import { useState, useMemo } from "react";
import Link from "next/link";
import { FileText, Info, Search, Eye, CheckCircle, XCircle, Loader2, ChevronLeft, ChevronRight } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { useProviders } from "@/hooks/admin";
import type { ProviderDto } from "@/lib/types";

const ITEMS_PER_PAGE = 10;

const getProviderDocumentStatus = (provider: ProviderDto): "complete" | "pending" | "missing" => {
  const hasCnpj = (provider.businessProfile as any)?.cnpj;
  const hasDocuments = provider.documents && provider.documents.length > 0;
  
  if (hasCnpj && hasDocuments) return "complete";
  if (hasDocuments || hasCnpj) return "pending";
  return "missing";
};

const getDocumentStatusBadge = (status: "complete" | "pending" | "missing") => {
  switch (status) {
    case "complete":
      return <Badge variant="success">Completo</Badge>;
    case "pending":
      return <Badge variant="warning">Pendente</Badge>;
    case "missing":
      return <Badge variant="destructive">Ausente</Badge>;
  }
};

export default function DocumentsPage() {
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const { data, isLoading, isFetching, error } = useProviders();

  const providers = data?.data ?? [];

  const filteredProviders = providers.filter(
    (p: any) => (p.name?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const unfilteredTotalPages = Math.ceil(filteredProviders.length / ITEMS_PER_PAGE);
  const totalPages = Math.max(1, unfilteredTotalPages);
  const safePage = Math.min(currentPage, totalPages);
  const startIndex = (safePage - 1) * ITEMS_PER_PAGE;
  const paginatedProviders = filteredProviders.slice(startIndex, startIndex + ITEMS_PER_PAGE);

  const kpis = useMemo(() => {
    let complete = 0, pending = 0, missing = 0;
    providers.forEach((p: any) => {
      const status = getProviderDocumentStatus(p);
      if (status === "complete") complete++;
      else if (status === "pending") pending++;
      else missing++;
    });
    return { complete, pending, missing };
  }, [providers]);

  const handleSearch = (value: string) => {
    setSearch(value);
    setCurrentPage(1);
  };

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Documentos</h1>
          <p className="text-muted-foreground">Gerencie documentos dos prestadores</p>
        </div>
      </div>

      <Card className="mb-6">
        <div className="p-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar por nome do prestador..."
              className="pl-10"
              value={search}
              onChange={(e) => handleSearch(e.target.value)}
              aria-label="Buscar prestador por nome"
            />
          </div>
        </div>
      </Card>

      <Card className="mb-6">
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><FileText className="h-5 w-5" />Documentos por Prestador</CardTitle>
        </CardHeader>
        <CardContent>
          {(providers.length > 0 || isLoading || isFetching) && (
            <div className="mb-6 grid gap-4 md:grid-cols-3">
              <div className="rounded-lg border border-border bg-muted/50 p-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-green-100">
                    <CheckCircle className="h-5 w-5 text-green-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold">{kpis.complete}</p>
                    <p className="text-sm text-muted-foreground">Documentação Completa</p>
                  </div>
                </div>
              </div>
              <div className="rounded-lg border border-border bg-muted/50 p-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-yellow-100">
                    <Info className="h-5 w-5 text-yellow-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold">{kpis.pending}</p>
                    <p className="text-sm text-muted-foreground">Documentação Pendente</p>
                  </div>
                </div>
              </div>
              <div className="rounded-lg border border-border bg-muted/50 p-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-100">
                    <XCircle className="h-5 w-5 text-red-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold">{kpis.missing}</p>
                    <p className="text-sm text-muted-foreground">Documentação Ausente</p>
                  </div>
                </div>
              </div>
            </div>
          )}

          <p className="text-sm text-muted-foreground mb-4">
            Clique em um prestador para visualizar e gerenciar seus documentos.
          </p>
        </CardContent>
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
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Prestador</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Email</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">CNPJ</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Status</th>
                    <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedProviders.map((provider: any) => {
                    const docStatus = getProviderDocumentStatus(provider);
                    return (
                      <tr key={provider.id} className="border-b border-border last:border-b-0">
                        <td className="px-4 py-3 text-sm font-medium">{provider.name ?? "-"}</td>
                        <td className="px-4 py-3 text-sm text-muted-foreground">
                          {(provider.businessProfile as any)?.contactInfo?.email ?? "-"}
                        </td>
                        <td className="px-4 py-3 text-sm text-muted-foreground">
                          {(provider.businessProfile as any)?.cnpj ?? "Não informado"}
                        </td>
                        <td className="px-4 py-3">
                          {getDocumentStatusBadge(docStatus)}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center justify-end gap-2">
                            <Link href={`/providers/${provider.id}`} className="inline-flex h-10 w-10 items-center justify-center rounded-md text-muted-foreground hover:text-foreground hover:bg-muted transition-colors">
                              <Eye className="h-4 w-4" />
                            </Link>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {unfilteredTotalPages > 1 && (
              <div className="flex items-center justify-between border-t border-border px-4 py-3">
                <p className="text-sm text-muted-foreground">
                  Mostrando {startIndex + 1} - {Math.min(startIndex + ITEMS_PER_PAGE, filteredProviders.length)} de {filteredProviders.length}
                </p>
                <div className="flex items-center gap-2">
                  <Button
                    variant="secondary"
                    size="icon"
                    disabled={safePage === 1}
                    onClick={() => setCurrentPage((p) => p - 1)}
                    aria-label="Página anterior"
                  >
                    <ChevronLeft className="h-4 w-4" />
                  </Button>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (safePage <= 3) {
                      pageNum = i + 1;
                    } else if (safePage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = safePage - 2 + i;
                    }
                    return (
                      <Button
                        key={pageNum}
                        variant={safePage === pageNum ? "primary" : "secondary"}
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
                    disabled={safePage === totalPages}
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
