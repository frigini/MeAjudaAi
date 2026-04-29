"use client";

import { useQuery } from "@tanstack/react-query";
import { apiMeGet } from "../lib/api/generated";
import { ProfileHeader } from "../components/profile/profile-header";
import { ProfileDescription } from "../components/profile/profile-description";
import { ProfileServices } from "../components/profile/profile-services";
import { ProfileReviews } from "../components/profile/profile-reviews";
import { useProviderVerificationEvents } from "../hooks/use-provider-verification";

export default function ProviderDashboard() {
  const { data: response, isLoading, error } = useQuery({
    queryKey: ["providerMe"],
    queryFn: () => apiMeGet(),
  });

  if (isLoading) {
    return (
      <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="animate-pulse text-muted-foreground">Carregando seu perfil...</div>
        </div>
      </div>
    );
  }

  if (error || !response?.data?.data) {
    return (
      <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center min-h-[400px] gap-4">
          <p className="text-muted-foreground">Erro ao carregar perfil. Tente novamente mais tarde.</p>
        </div>
      </div>
    );
  }

  const provider = response.data.data;
  
  // Ativa o streaming SSE para status de verificação
  useProviderVerificationEvents(provider.id);

  const bp = provider.businessProfile;
  const contact = bp?.contactInfo;
  const addr = bp?.primaryAddress;

  const phones: string[] = [];
  if (contact?.phoneNumber) {
    phones.push(contact.phoneNumber);
  }
  if (contact?.additionalPhones && contact.additionalPhones.length > 0) {
    phones.push(...contact.additionalPhones);
  }

  const services: string[] = [];
  if (provider.services && provider.services.length > 0) {
    services.push(...provider.services.map((s) => s.serviceName || "Serviço"));
  }

  return (
    <div className="container mx-auto max-w-5xl py-8 px-4 sm:px-6 lg:px-8">
      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10">
        <ProfileHeader
          name={provider.name || "Prestador"}
          email={contact?.email || ""}
          isOnline={provider.isActive ?? false}
          phones={phones.length > 0 ? phones : ["Sem telefone cadastrado"]}
          rating={3.5}
        />
        
        <ProfileDescription 
          description={bp?.description || "Nenhuma descrição cadastrada. Clique em editar para adicionar."} 
        />
        
        <ProfileServices services={services} />

        <ProfileReviews reviews={[]} />
      </main>
    </div>
  );
}
