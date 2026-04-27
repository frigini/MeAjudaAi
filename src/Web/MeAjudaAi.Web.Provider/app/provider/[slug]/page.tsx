"use client";

import { use } from "react";
import { useQuery } from "@tanstack/react-query";
import { notFound } from "next/navigation";
import { MapPin, Phone, Mail, Star, CheckCircle, Clock, AlertCircle } from "lucide-react";
import { apiPublicGet } from "@/lib/api/generated";

interface PageProps {
  params: Promise<{ slug: string }>;
}

function getVerificationLabel(status?: number): { label: string; color: string } {
  switch (status) {
    case 1: return { label: "Pendente", color: "text-yellow-500" };
    case 2: return { label: "Em análise", color: "text-blue-500" };
    case 3: return { label: "Aprovado", color: "text-emerald-500" };
    case 4: return { label: "Rejeitado", color: "text-red-500" };
    case 5: return { label: "Em correção", color: "text-orange-500" };
    default: return { label: "Não verificado", color: "text-gray-500" };
  }
}

function getTypeLabel(type?: number): string {
  switch (type) {
    case 1: return "Pessoa Física";
    case 2: return "Empresa";
    case 3: return "Cooperativa";
    case 4: return "Freelancer";
    default: return "Prestador";
  }
}

export default function ProviderPublicPage({ params }: PageProps) {
  const resolvedParams = use(params);
  const slug = resolvedParams.slug;

  const { data: response, isLoading, error } = useQuery({
    queryKey: ["providerPublic", slug],
    queryFn: () => apiPublicGet({ path: { idOrSlug: slug } }),
    enabled: !!slug,
  });

  if (isLoading) {
    return (
      <div className="container mx-auto max-w-4xl py-8 px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="animate-pulse text-muted-foreground">Carregando perfil...</div>
        </div>
      </div>
    );
  }

  if (error || !response?.data?.data) {
    return notFound();
  }

  const provider = response.data.data;
  const verification = getVerificationLabel(provider.verificationStatus);
  const isVerified = provider.verificationStatus === 3;
  const displayName = provider.fantasyName || provider.name || "Prestador";

  return (
    <div className="container mx-auto max-w-4xl py-8 px-4 sm:px-6 lg:px-8">
      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10">
        <div className="flex flex-col gap-6 md:flex-row md:items-start">
          <div className="flex w-full md:w-64 flex-col items-center gap-3">
            <div className="relative flex h-32 w-32 shrink-0 items-center justify-center overflow-hidden rounded-full bg-muted">
              <img src="https://i.pravatar.cc/150" alt={displayName} className="h-full w-full object-cover" />
            </div>
            
            {provider.rating && provider.rating > 0 && (
              <div className="flex flex-col items-center gap-1">
                <div className="flex text-primary">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <Star
                      key={i}
                      className={`h-5 w-5 ${i < Math.floor(provider.rating || 0) ? "fill-current text-primary" : "text-muted-foreground"}`}
                    />
                  ))}
                </div>
                <span className="text-sm text-foreground-subtle">
                  {provider.rating.toFixed(1)} ({provider.reviewCount || 0} avaliações)
                </span>
              </div>
            )}
            
            <div className="mt-2 flex flex-col items-center gap-1 text-center text-xs">
              <span className="rounded-full bg-secondary px-2 py-1 text-muted-foreground">
                {getTypeLabel(provider.type)}
              </span>
              <span className={`flex items-center gap-1 ${verification.color}`}>
                {isVerified ? (
                  <CheckCircle className="h-3 w-3" />
                ) : provider.verificationStatus === 1 ? (
                  <Clock className="h-3 w-3" />
                ) : (
                  <AlertCircle className="h-3 w-3" />
                )}
                {verification.label}
              </span>
            </div>
          </div>

          <div className="flex flex-1 flex-col">
            <div className="flex flex-col">
              <h1 className="text-2xl font-bold tracking-tight text-foreground">
                {displayName}
              </h1>
              {provider.fantasyName && provider.name && (
                <p className="mt-1 text-sm text-foreground-subtle">{provider.name}</p>
              )}
              
              {(provider.city || provider.state) && (
                <div className="mt-2 flex items-center gap-1 text-sm text-muted-foreground">
                  <MapPin className="h-4 w-4" />
                  <span>{[provider.city, provider.state].filter(Boolean).join(", ")}</span>
                </div>
              )}
            </div>

            <div className="mt-6 flex flex-col gap-3">
              {provider.phoneNumbers && provider.phoneNumbers.length > 0 && (
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-secondary">
                    <Phone className="h-5 w-5 text-primary" />
                  </div>
                  <div className="flex flex-col">
                    <span className="text-xs font-medium text-foreground-subtle">Telefone</span>
                    <a href={`tel:${provider.phoneNumbers[0]}`} className="text-sm text-primary hover:underline">
                      {provider.phoneNumbers[0]}
                    </a>
                  </div>
                </div>
              )}

              {provider.email && (
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-secondary">
                    <Mail className="h-5 w-5 text-primary" />
                  </div>
                  <div className="flex flex-col">
                    <span className="text-xs font-medium text-foreground-subtle">E-mail</span>
                    <a href={`mailto:${provider.email}`} className="text-sm text-primary hover:underline">
                      {provider.email}
                    </a>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {provider.description && (
          <div className="mt-8">
            <h2 className="mb-2 text-base font-bold text-foreground">Sobre</h2>
            <p className="text-sm leading-relaxed text-foreground-subtle">
              {provider.description}
            </p>
          </div>
        )}

        {provider.services && provider.services.length > 0 && (
          <div className="mt-8">
            <h2 className="mb-4 text-base font-bold text-foreground">Serviços</h2>
            <div className="flex flex-wrap gap-2">
              {provider.services.map((service) => (
                <span
                  key={service.id}
                  className="flex items-center rounded-md bg-primary px-3 py-1 text-xs font-medium text-primary-foreground"
                >
                  {service.name}
                </span>
              ))}
            </div>
          </div>
        )}

        {!provider.isActive && (
          <div className="mt-8 rounded-lg border border-destructive/50 bg-destructive/10 p-4">
            <p className="text-sm text-destructive">
              Este perfil está temporariamente desativado e não está aparecendo nas buscas.
            </p>
          </div>
        )}
      </main>
    </div>
  );
}
