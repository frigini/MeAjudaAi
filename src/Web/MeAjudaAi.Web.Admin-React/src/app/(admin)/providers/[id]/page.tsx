"use client";

import { use, useState } from "react";
import Link from "next/link";
import { ArrowLeft, Mail, Phone, MapPin, FileText, CheckCircle, XCircle, Loader2 } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { useProviderById, useActivateProvider, useDeactivateProvider } from "@/hooks/admin";
import {
  providerTypeLabels,
  providerStatusLabels,
  verificationStatusLabels,
  providerTierLabels,
  type VerificationStatus,
  type ProviderTier,
} from "@/lib/types";
import { getVerificationBadgeVariant } from "@/lib/utils";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default function ProviderDetailPage({ params }: PageProps) {
  const { id } = use(params);
  const [isApproveOpen, setIsApproveOpen] = useState(false);
  const [isRejectOpen, setIsRejectOpen] = useState(false);

  const { data: provider, isLoading, error } = useProviderById(id);
  const activateMutation = useActivateProvider();
  const deactivateMutation = useDeactivateProvider();

  const handleApprove = async () => {
    try {
      await activateMutation.mutateAsync(id);
      toast.success("Prestador aprovado com sucesso");
      setIsApproveOpen(false);
    } catch {
      toast.error("Erro ao aprovar prestador");
    }
  };

  const handleReject = async () => {
    try {
      await deactivateMutation.mutateAsync(id);
      toast.success("Prestador suspenso com sucesso");
      setIsRejectOpen(false);
    } catch {
      toast.error("Erro ao suspender prestador");
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-200px)]">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error || !provider) {
    return (
      <div className="p-8 text-center">
        <p className="text-destructive">Erro ao carregar prestador. Tente novamente.</p>
        <Link href="/providers">
          <Button variant="ghost" className="mt-4">Voltar para lista</Button>
        </Link>
      </div>
    );
  }

  const businessProfile = provider.businessProfile;
  const contact = businessProfile?.contactInfo;
  const address = businessProfile?.primaryAddress;

  return (
    <div className="p-8">
      <div className="mb-8">
        <Link href="/providers" className="mb-4 inline-flex items-center text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Voltar para lista
        </Link>
        <div className="mt-4 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-foreground">{provider.name ?? "Prestador"}</h1>
            <div className="mt-2 flex items-center gap-2">
              <Badge variant={getVerificationBadgeVariant(provider.verificationStatus)}>
                {verificationStatusLabels[provider.verificationStatus as keyof typeof verificationStatusLabels] ?? "-"}
              </Badge>
              <Badge variant="secondary">
                {providerTypeLabels[provider.type as keyof typeof providerTypeLabels] ?? "-"}
              </Badge>
              <Badge variant="secondary">
                {providerTierLabels[provider.tier as keyof typeof providerTierLabels] ?? "-"}
              </Badge>
            </div>
          </div>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setIsApproveOpen(true)}>
              <CheckCircle className="mr-2 h-4 w-4 text-green-500" />
              Aprovar
            </Button>
            <Button variant="secondary" onClick={() => setIsRejectOpen(true)}>
              <XCircle className="mr-2 h-4 w-4 text-red-500" />
              Suspender
            </Button>
          </div>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Mail className="h-5 w-5" />Informações de Contato
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Email</label>
              <p className="mt-1">{contact?.email ?? "-"}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Telefone</label>
              <p className="mt-1">{contact?.phoneNumber ?? "-"}</p>
            </div>
            {contact?.additionalPhones && contact.additionalPhones.length > 0 && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Telefones Adicionais</label>
                <p className="mt-1">{contact.additionalPhones.join(", ")}</p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="h-5 w-5" />Endereço
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Cidade</label>
              <p className="mt-1">{address?.city ?? "-"}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Estado</label>
              <p className="mt-1">{address?.state ?? "-"}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Endereço</label>
              <p className="mt-1">
                {[
                  address?.street,
                  address?.number,
                  address?.neighborhood,
                  address?.zipCode,
                ].filter(Boolean).join(", ") || "-"}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5" />Documentos
            </CardTitle>
          </CardHeader>
          <CardContent>
            {provider.documents && provider.documents.length > 0 ? (
              <div className="space-y-2">
                {provider.documents.map((doc: any, index: number) => (
                  <div key={index} className="flex items-center justify-between rounded-lg border p-3">
                    <div>
                      <p className="font-medium">{doc.documentType ?? "Documento"}</p>
                      <p className="text-sm text-muted-foreground">{doc.fileName ?? "-"}</p>
                    </div>
                    <Badge variant={getVerificationBadgeVariant(doc.verificationStatus as VerificationStatus)}>
                      {verificationStatusLabels[doc.verificationStatus as keyof typeof verificationStatusLabels] ?? "-"}
                    </Badge>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground">Nenhum documento enviado.</p>
            )}
          </CardContent>
        </Card>

        {provider.services && provider.services.length > 0 && (
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle>Serviços</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {provider.services.map((service: any, index: number) => (
                  <Badge key={index} variant="secondary">
                    {service.serviceName ?? "Serviço"}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Informações Adicionais</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-3">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Status do Cadastro</label>
              <p className="mt-1">
                {providerStatusLabels[provider.status as keyof typeof providerStatusLabels] ?? "-"}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Data de Criação</label>
              <p className="mt-1">
                {provider.createdAt ? new Date(provider.createdAt).toLocaleDateString("pt-BR") : "-"}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Última Atualização</label>
              <p className="mt-1">
                {provider.updatedAt ? new Date(provider.updatedAt).toLocaleDateString("pt-BR") : "-"}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Dialog open={isApproveOpen} onOpenChange={setIsApproveOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Aprovar Prestador</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja aprovar o prestador <strong>{provider.name}</strong>?
              Ele poderá começar a operar na plataforma.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="secondary" onClick={() => setIsApproveOpen(false)}>Cancelar</Button>
            <Button onClick={handleApprove} disabled={activateMutation.isPending}>
              {activateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Aprovar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={isRejectOpen} onOpenChange={setIsRejectOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Suspender Prestador</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja suspender o prestador <strong>{provider.name}</strong>?
              Ele não poderá mais operar na plataforma.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="secondary" onClick={() => setIsRejectOpen(false)}>Cancelar</Button>
            <Button variant="destructive" onClick={handleReject} disabled={deactivateMutation.isPending}>
              {deactivateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Suspender
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
