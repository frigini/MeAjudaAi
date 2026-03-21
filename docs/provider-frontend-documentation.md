# MeAjudaAi.Web.Provider - Documentação Completa de Frontend

Este documento contém toda a estrutura HTML, CSS e informações de design do projeto MeAjudaAi.Web.Provider.

---

# ÍNDICE

1. [CSS Global](#1-css-global)
2. [Pages (Rotas)](#2-pages-rotas)
3. [Components - UI](#3-components---ui)
4. [Components - Layout](#4-components---layout)
5. [Components - Dashboard](#5-components---dashboard)
6. [Components - Profile](#6-components---profile)
7. [Imagens e Assets](#7-imagens-e-assets)
8. [Sistema de Design](#8-sistema-de-design)

---

# 1. CSS GLOBAL

## Arquivo: `app/global.css`

```css
@import "tailwindcss";

:root {
  --background: #ffffff;
  --foreground: #2e2e2e;

  /* Light mode tokens */
  --surface: #ffffff;
  --surface-raised: #f5f5f5;
  --foreground-subtle: #666666;
  --border: #e0e0e0;
  --input: #e0e0e0;
  --popover: #ffffff;
  --popover-foreground: var(--foreground);
  --card: #ffffff;
  --card-foreground: var(--foreground);
  --muted: #f5f5f5;
  --muted-foreground: #666666;
  --accent: #f5f5f5;
  --accent-foreground: var(--foreground);
}

@media (prefers-color-scheme: dark) {
  :root {
    --background: #0a0a0a;
    --foreground: #ededed;

    /* Dark mode overrides */
    --surface: #1a1a1a;
    --surface-raised: #262626;
    --foreground-subtle: #a3a3a3;
    --border: #404040;
    --input: #404040;
    --popover: #1a1a1a;
    --popover-foreground: var(--foreground);
    --card: #1a1a1a;
    --card-foreground: var(--foreground);
    --muted: #262626;
    --muted-foreground: #a3a3a3;
    --accent: #262626;
    --accent-foreground: var(--foreground);
  }
}

@theme inline {
  /* Colors from Figma */
  --color-primary: #395873;
  --color-primary-foreground: #ffffff;
  --color-primary-hover: #2E4760;

  --color-secondary: #D96704;
  --color-secondary-light: #F2AE72;
  --color-secondary-foreground: #ffffff;
  --color-secondary-hover: #B85703;

  --color-surface: var(--surface);
  --color-surface-raised: var(--surface-raised);

  --color-foreground: var(--foreground);
  --color-foreground-subtle: var(--foreground-subtle);

  --color-border: var(--border);
  --color-input: var(--input);
  --color-ring: #D96704;

  --color-brand: #E0702B;
  --color-brand-hover: #c56226;

  --color-destructive: #dc2626;
  --color-destructive-foreground: #ffffff;

  --color-popover: var(--popover);
  --color-popover-foreground: var(--popover-foreground);

  --color-card: var(--card);
  --color-card-foreground: var(--card-foreground);

  --color-muted: var(--muted);
  --color-muted-foreground: var(--muted-foreground);

  --color-accent: var(--accent);
  --color-accent-foreground: var(--accent-foreground);

  --color-background: var(--background);
  --font-sans: 'Roboto', Arial, Helvetica, sans-serif;
}

body {
  background: var(--background);
  color: var(--foreground);
  font-family: var(--font-sans);
}
```

---

# 2. PAGES (ROTAS)

## ROTA: `/` (Dashboard do Prestador)
**Arquivo:** `app/page.tsx`

```tsx
"use client";

import { useQuery } from "@tanstack/react-query";
import { apiMeGet } from "../lib/api/generated";
import { ProfileHeader } from "../components/profile/profile-header";
import { ProfileDescription } from "../components/profile/profile-description";
import { ProfileServices } from "../components/profile/profile-services";
import { ProfileReviews } from "../components/profile/profile-reviews";

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
  const bp = provider.businessProfile;
  const contact = bp?.contactInfo;

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
```

---

## ROTA: `/register`
**Arquivo:** `app/register/page.tsx`

```tsx
"use client";

import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { Label } from "../../components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import { useRouter } from "next/navigation";

export default function RegisterPage() {
  const router = useRouter();

  const handleRegister = (e: React.FormEvent) => {
    e.preventDefault();
    router.push("/onboarding/basic-info");
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-raised p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Torne-se um Prestador</CardTitle>
          <p className="text-sm text-foreground-subtle">
            Crie sua conta para oferecer serviços na plataforma MeAjudaAí.
          </p>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleRegister} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="name" required>Nome Completo ou Fantasia</Label>
              <Input id="name" placeholder="Ex: João Silva Reformas" required />
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="document" required>CPF ou CNPJ</Label>
                <Input id="document" placeholder="000.000.000-00" required />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="phone" required>Celular</Label>
                <Input id="phone" type="tel" placeholder="(11) 99999-9999" required />
              </div>
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="email" required>E-mail Profissional</Label>
              <Input id="email" type="email" placeholder="contato@empresa.com" required />
            </div>

            <div className="flex items-center gap-2 pt-2">
              <input type="checkbox" id="terms" required className="size-4 rounded border-border text-primary focus:ring-primary" />
              <Label htmlFor="terms" className="text-xs font-normal">
                Li e aceito os Termos de Uso e Política de Privacidade.
              </Label>
            </div>

            <Button type="submit" className="mt-4 w-full" size="lg">
              Começar agora
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
```

---

## ROTA: `/provider/[slug]` (Perfil Público)
**Arquivo:** `app/provider/[slug]/page.tsx`

```tsx
"use client";

import { use } from "react";
import { useQuery } from "@tanstack/react-query";
import { notFound } from "next/navigation";
import { MapPin, Phone, Mail, Star, CheckCircle, Clock, AlertCircle } from "lucide-react";
import { apiPublicGet } from "@/lib/api/generated";

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
    return <div className="animate-pulse text-muted-foreground">Carregando perfil...</div>;
  }

  if (error || !response?.data?.data) {
    return notFound();
  }

  const provider = response.data.data;
  const verification = getVerificationLabel(provider.verificationStatus);
  const isVerified = provider.verificationStatus === 3;
  const displayName = provider.fantasyName || provider.name || "Prestador";

  return (
    <div className="container mx-auto max-w-4xl py-8 px-4">
      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10">
        <div className="flex flex-col gap-6 md:flex-row md:items-start">
          {/* Avatar e Info Lateral */}
          <div className="flex w-full md:w-64 flex-col items-center gap-3">
            <div className="relative flex h-32 w-32 shrink-0 items-center justify-center overflow-hidden rounded-full bg-muted">
              <img src="https://i.pravatar.cc/150" alt={displayName} className="h-full w-full object-cover" />
            </div>
            
            {provider.rating && provider.rating > 0 && (
              <div className="flex flex-col items-center gap-1">
                <div className="flex text-primary">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <Star key={i} className={`h-5 w-5 ${i < Math.floor(provider.rating || 0) ? "fill-current text-primary" : "text-muted-foreground"}`} />
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
                {isVerified ? <CheckCircle className="h-3 w-3" /> : provider.verificationStatus === 1 ? <Clock className="h-3 w-3" /> : <AlertCircle className="h-3 w-3" />}
                {verification.label}
              </span>
            </div>
          </div>

          {/* Info Principal */}
          <div className="flex flex-1 flex-col">
            <div className="flex flex-col">
              <h1 className="text-2xl font-bold tracking-tight text-foreground">{displayName}</h1>
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

            {/* Contatos */}
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

        {/* Descrição */}
        {provider.description && (
          <div className="mt-8">
            <h2 className="mb-2 text-base font-bold text-foreground">Sobre</h2>
            <p className="text-sm leading-relaxed text-foreground-subtle">{provider.description}</p>
          </div>
        )}

        {/* Serviços */}
        {provider.services && provider.services.length > 0 && (
          <div className="mt-8">
            <h2 className="mb-4 text-base font-bold text-foreground">Serviços</h2>
            <div className="flex flex-wrap gap-2">
              {provider.services.map((service, index) => (
                <span key={index} className="flex items-center rounded-md bg-primary px-3 py-1 text-xs font-medium text-primary-foreground">
                  {service}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Perfil Inativo */}
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
```

---

## ROTA: `/onboarding/basic-info`
**Arquivo:** `app/onboarding/basic-info/page.tsx`

```tsx
"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Label } from "../../../components/ui/label";
import { Input } from "../../../components/ui/input";
import { Button } from "../../../components/ui/button";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiMeGet, apiMePut } from "@/lib/api/generated";
import { toast } from "sonner";

const basicInfoSchema = z.object({
  description: z.string().max(1000).optional(),
  zipCode: z.string().min(8).max(9),
  street: z.string().min(3).max(200),
  number: z.string().min(1).max(20),
  complement: z.string().max(100).optional(),
  neighborhood: z.string().min(2).max(100),
  city: z.string().min(2).max(100),
  state: z.string().length(2).max(50),
});

type BasicInfoFormValues = z.infer<typeof basicInfoSchema>;

export default function BasicInfoPage() {
  const router = useRouter();
  const queryClient = useQueryClient();

  const form = useForm<BasicInfoFormValues>({
    resolver: zodResolver(basicInfoSchema),
    defaultValues: { description: "", zipCode: "", street: "", number: "", complement: "", neighborhood: "", city: "", state: "" },
  });

  const { data: providerData, isLoading } = useQuery({
    queryKey: ["providerMe"],
    queryFn: () => apiMeGet(),
  });

  useEffect(() => {
    if (providerData?.data?.data) {
      const bp = providerData.data.data.businessProfile;
      const addr = bp?.primaryAddress;
      form.reset({
        description: bp?.description || "",
        zipCode: addr?.zipCode || "",
        street: addr?.street || "",
        number: addr?.number || "",
        complement: addr?.complement || "",
        neighborhood: addr?.neighborhood || "",
        city: addr?.city || "",
        state: addr?.state || "",
      });
    }
  }, [providerData, form]);

  const updateMutation = useMutation({
    mutationFn: (req) => apiMePut({ body: req }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["providerMe"] });
      toast.success("Informações básicas salvas com sucesso!");
      router.push("/onboarding/services");
    },
    onError: () => toast.error("Erro ao salvar. Tente novamente."),
  });

  const onSubmit = (data: BasicInfoFormValues) => {
    const currentBp = providerData?.data?.data?.businessProfile;
    updateMutation.mutate({
      name: providerData?.data?.data?.name || "",
      businessProfile: {
        description: data.description,
        primaryAddress: { street: data.street, number: data.number, complement: data.complement, neighborhood: data.neighborhood, city: data.city, state: data.state, zipCode: data.zipCode, country: "Brasil" },
      },
    });
  };

  if (isLoading) return <div className="flex items-center justify-center min-h-[400px]"><div className="animate-pulse text-muted-foreground">Carregando...</div></div>;

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Informações Básicas</h2>
        <p className="mt-1 text-sm text-foreground-subtle">Complete os dados do seu perfil de negócio.</p>
      </div>

      <div className="flex flex-col gap-4 border-t border-border pt-6">
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="description">Descrição do Negócio / Biografia</Label>
          <textarea id="description" rows={4} className="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm" placeholder="Conte um pouco sobre sua experiência..." {...form.register("description")} />
        </div>

        <h3 className="text-sm font-semibold text-foreground pt-4">Endereço Principal</h3>
        
        <div className="grid grid-cols-6 gap-4">
          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="zipCode">CEP</Label>
            <Input id="zipCode" placeholder="00000-000" {...form.register("zipCode")} />
          </div>
          <div className="col-span-4 flex flex-col gap-1.5">
            <Label htmlFor="street">Rua / Avenida</Label>
            <Input id="street" placeholder="Av. Principal" {...form.register("street")} />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="number">Número</Label>
            <Input id="number" placeholder="123" {...form.register("number")} />
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="complement">Complemento</Label>
            <Input id="complement" placeholder="Sala 101" {...form.register("complement")} />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="neighborhood">Bairro</Label>
            <Input id="neighborhood" placeholder="Centro" {...form.register("neighborhood")} />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="city">Cidade</Label>
            <Input id="city" placeholder="São Paulo" {...form.register("city")} />
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="state">Estado</Label>
            <Input id="state" placeholder="SP" maxLength={2} {...form.register("state")} />
          </div>
        </div>
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()}>Voltar</Button>
        <Button type="submit" disabled={updateMutation.isPending}>
          {updateMutation.isPending ? "Salvando..." : "Salvar e Continuar"}
        </Button>
      </div>
    </form>
  );
}
```

---

## ROTA: `/onboarding/services`
**Arquivo:** `app/onboarding/services/page.tsx`

```tsx
"use client";

import { Button } from "../../../components/ui/button";
import { Label } from "../../../components/ui/label";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { twMerge } from "tailwind-merge";

const MOCK_SERVICES = [
  { id: "1", name: "Limpeza Geral" },
  { id: "2", name: "Eletricista" },
  { id: "3", name: "Encanador" },
  { id: "4", name: "Pintor" },
  { id: "5", name: "Montador de Móveis" },
];

export default function ServicesPage() {
  const router = useRouter();
  const [selectedServices, setSelectedServices] = useState<string[]>([]);

  const toggleService = (id: string) => {
    setSelectedServices(prev => prev.includes(id) ? prev.filter(s => s !== id) : [...prev, id]);
  };

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    router.push("/onboarding/documents");
  };

  return (
    <form onSubmit={handleSave} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Seleção de Serviços</h2>
        <p className="mt-1 text-sm text-foreground-subtle">Quais serviços você pretende oferecer na plataforma?</p>
      </div>

      <div className="flex flex-col gap-4 border-t border-border pt-6">
        <Label>Categorias Disponíveis</Label>
        
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
          {MOCK_SERVICES.map(service => {
            const isSelected = selectedServices.includes(service.id);
            return (
              <button key={service.id} type="button" onClick={() => toggleService(service.id)}
                className={twMerge("flex items-center justify-center rounded-lg border p-4 text-sm font-medium transition-colors",
                  isSelected ? "border-primary bg-primary text-primary-foreground" : "border-border bg-surface text-foreground hover:bg-surface-raised")}>
                {service.name}
              </button>
            );
          })}
        </div>
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()}>Voltar</Button>
        <Button type="submit" disabled={selectedServices.length === 0}>Salvar e Continuar</Button>
      </div>
    </form>
  );
}
```

---

## ROTA: `/onboarding/documents`
**Arquivo:** `app/onboarding/documents/page.tsx`

```tsx
"use client";

import { useState } from "react";
import { Button } from "../../../components/ui/button";
import { FileUpload } from "../../../components/ui/file-upload";
import { useRouter } from "next/navigation";
import { z } from "zod";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { apiUploadPost } from "@/lib/api/generated";
import { toast } from "sonner";

const documentSchema = z.object({
  identityFile: z.custom<File>((val) => val instanceof File, "O documento de identidade é obrigatório")
    .refine((file) => file.size <= 5 * 1024 * 1024, "O tamanho máximo é 5MB")
    .refine((file) => ["image/jpeg", "image/png", "application/pdf"].includes(file.type), "Formato inválido"),
  certificateFile: z.instanceof(File).optional(),
});

type DocumentFormData = z.infer<typeof documentSchema>;

export default function DocumentsPage() {
  const router = useRouter();
  const [uploadProgress, setUploadProgress] = useState<string>("");

  const { control, handleSubmit, formState: { errors, isValid } } = useForm<DocumentFormData>({
    resolver: zodResolver(documentSchema),
    mode: "onChange",
  });

  const uploadMutation = useMutation({
    mutationFn: async ({ file, documentType }: { file: File; documentType: 1 | 2 | 3 | 99 }) => {
      const uploadResponse = await apiUploadPost({ body: { documentType, fileName: file.name, contentType: file.type, fileSizeBytes: file.size } });
      if (!uploadResponse.data?.uploadUrl) throw new Error("Falha ao obter URL de upload");
      const { uploadUrl, documentId } = uploadResponse.data;
      setUploadProgress(`Enviando ${file.name}...`);
      await fetch(uploadUrl, { method: "PUT", body: file, headers: { "Content-Type": file.type } });
      return { documentId, fileName: file.name };
    },
    onSuccess: () => toast.success("Documento enviado com sucesso!"),
    onError: (error) => { toast.error("Erro ao enviar documento"); },
  });

  const onSubmit = async (data: DocumentFormData) => {
    setUploadProgress("");
    try {
      await uploadMutation.mutateAsync({ file: data.identityFile, documentType: 1 });
      if (data.certificateFile) await uploadMutation.mutateAsync({ file: data.certificateFile, documentType: 2 });
      setUploadProgress("Concluindo...");
      router.push("/");
    } catch (error) { console.error(error); }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Documentos</h2>
        <p className="mt-1 text-sm text-foreground-subtle">Para garantir a segurança da plataforma, precisamos validar sua identidade.</p>
      </div>

      <div className="flex flex-col gap-8 border-t border-border pt-6">
        <Controller name="identityFile" control={control} render={({ field: { onChange } }) => (
          <div>
            <FileUpload label="Documento de Identidade (Frente e Verso)" description="Faça o upload do seu RG ou CNH. Formatos aceitos: .jpg, .png, .pdf." required onFileSelect={(file) => onChange(file)} />
            {errors.identityFile && <p className="mt-1 text-sm text-destructive">{errors.identityFile.message as string}</p>}
          </div>
        )} />

        <Controller name="certificateFile" control={control} render={({ field: { onChange } }) => (
          <div>
            <FileUpload label="Comprovante de Residência (Opcional)" description="Envie um comprovante de residência recente." onFileSelect={(file) => onChange(file)} />
          </div>
        )} />
      </div>

      {uploadProgress && <div className="rounded-lg bg-secondary p-3 text-sm text-muted-foreground">{uploadProgress}</div>}

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()} disabled={uploadMutation.isPending}>Voltar</Button>
        <Button type="submit" disabled={!isValid || uploadMutation.isPending}>
          {uploadMutation.isPending ? "Enviando para Azure..." : "Concluir Onboarding"}
        </Button>
      </div>
    </form>
  );
}
```

---

## ROTA: `/configuracoes`
**Arquivo:** `app/configuracoes/page.tsx`

```tsx
"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "../../components/ui/button";
import { useMutation, useQuery } from "@tanstack/react-query";
import { apiMeGet } from "@/lib/api/generated";
import { signOut } from "next-auth/react";
import { toast } from "sonner";

export default function ConfiguracoesPage() {
  const router = useRouter();
  const [isVisible, setIsVisible] = useState(true);
  const [showDeactivateModal, setShowDeactivateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteConfirmation, setDeleteConfirmation] = useState("");

  const { data: providerData } = useQuery({ queryKey: ["providerMe"], queryFn: () => apiMeGet() });

  const deactivateMutation = useMutation({
    mutationFn: async () => {
      const response = await fetch(`/api/v1/providers/${providerData?.data?.data?.id}/deactivate`, { method: "POST" });
      if (!response.ok) throw new Error("Falha ao desativar");
      return response.json();
    },
    onSuccess: () => { toast.success("Perfil desativado com sucesso!"); setIsVisible(false); setShowDeactivateModal(false); router.refresh(); },
    onError: () => toast.error("Erro ao desativar o perfil."),
  });

  const confirmDelete = async () => {
    if (deleteConfirmation !== "EXCLUIR") { toast.error("Digite EXACTAMENTE EXCLUIR para confirmar"); return; }
    try {
      const userId = providerData?.data?.data?.userId;
      const response = await fetch(`/api/v1/users/${userId}`, { method: "DELETE" });
      if (!response.ok) throw new Error("Falha ao excluir conta");
      toast.success("Conta excluída com sucesso!");
      setShowDeleteModal(false);
      setTimeout(() => signOut({ callbackUrl: "/" }), 1500);
    } catch (error) { toast.error("Erro ao excluir a conta."); }
  };

  return (
    <div className="container mx-auto max-w-2xl py-8 px-4">
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild className="h-8 w-8 px-0">
          <Link href="/"><span className="text-xl">&lsaquo;</span><span className="sr-only">Voltar</span></Link>
        </Button>
        <span className="font-bold">AjudaAí</span>
        <div className="ml-auto text-sm font-medium hover:underline cursor-pointer" onClick={() => signOut({ callbackUrl: "/" })}>Sair</div>
      </div>

      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10 flex flex-col min-h-[60vh]">
        <h1 className="mb-12 text-2xl font-bold tracking-tight text-foreground">Configurações</h1>

        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-foreground">Deixar meu perfil visível</span>
          <button type="button" role="switch" aria-checked={isVisible} onClick={() => isVisible ? setShowDeactivateModal(true) : setIsVisible(true)}
            className="relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center justify-center rounded-full bg-border transition-colors focus:outline-none focus:ring-2 focus:ring-primary aria-checked:bg-emerald-500">
          </button>
        </div>

        <div className="mt-auto flex justify-center pt-12">
          <Button variant="destructive" size="sm" onClick={() => setShowDeleteModal(true)}>Apagar minha conta</Button>
        </div>
      </main>

      {/* Modal Desativar */}
      {showDeactivateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-lg">
            <div className="mb-4 flex items-start justify-between">
              <h2 className="text-lg font-bold text-foreground">Esconder perfil?</h2>
              <button onClick={() => setShowDeactivateModal(false)} className="text-foreground-subtle hover:text-foreground">&times;</button>
            </div>
            <p className="mb-6 text-sm text-foreground-subtle">Desativando seu perfil ele não será mais exibido em buscas.</p>
            <div className="flex justify-end gap-3">
              <Button variant="ghost" onClick={() => setShowDeactivateModal(false)}>Cancelar</Button>
              <Button variant="destructive" onClick={() => deactivateMutation.mutate()} disabled={deactivateMutation.isPending}>
                {deactivateMutation.isPending ? "Desativando..." : "Desativar"}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Modal Apagar Conta */}
      {showDeleteModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-lg">
            <div className="mb-4 flex items-start justify-between">
              <h2 className="text-lg font-bold text-foreground">Apagar perfil?</h2>
              <button onClick={() => { setShowDeleteModal(false); setDeleteConfirmation(""); }} className="text-foreground-subtle hover:text-foreground">&times;</button>
            </div>
            <p className="mb-4 text-sm text-foreground-subtle">Apagando seu perfil, excluir todos os seus dados. Esta ação NÃO pode ser desfeita.</p>
            <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/10 p-3">
              <p className="text-xs text-destructive font-medium mb-2">Esta ação NÃO pode ser desfeita:</p>
              <ul className="text-xs text-destructive/80 list-disc list-inside space-y-1">
                <li>Seu perfil de prestador será removido</li>
                <li>Sua conta de usuário será excluída</li>
                <li>Todos os seus dados serão apagados</li>
              </ul>
            </div>
            <div className="mb-6">
              <label className="block text-sm font-medium text-foreground mb-2">Digite <span className="font-bold text-destructive">EXCLUIR</span> para confirmar:</label>
              <input type="text" value={deleteConfirmation} onChange={(e) => setDeleteConfirmation(e.target.value)} className="w-full rounded-md border border-input px-3 py-2 text-sm focus:border-destructive focus:outline-none" placeholder="EXCLUIR" />
            </div>
            <div className="flex justify-end gap-3">
              <Button variant="ghost" onClick={() => { setShowDeleteModal(false); setDeleteConfirmation(""); }}>Cancelar</Button>
              <Button variant="destructive" onClick={confirmDelete} disabled={deleteConfirmation !== "EXCLUIR"}>Excluir Minha Conta</Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
```

---

## ROTA: `/alterar-dados`
**Arquivo:** `app/alterar-dados/page.tsx`

```tsx
"use client";

import { useForm, useFieldArray, useEffect } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Plus, Trash2 } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiMeGet, apiMePut } from "@/lib/api/generated";
import { toast } from "sonner";

const profileSchema = z.object({
  fullName: z.string().min(3),
  fantasyName: z.string().optional(),
  email: z.string().email(),
  cpf: z.string().min(11),
  phones: z.array(z.object({ number: z.string().min(8), isWhatsapp: z.boolean().default(false) })).min(1),
  cep: z.string().min(8),
  address: z.string().min(3),
  number: z.string().min(1),
  neighborhood: z.string().min(2),
  city: z.string().min(2),
  state: z.string().length(2),
  showAddressToClient: z.boolean().default(false),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export default function AlterarDadosPage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  
  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: { fullName: "", fantasyName: "", email: "", cpf: "", phones: [{ number: "", isWhatsapp: false }], cep: "", address: "", number: "", neighborhood: "", city: "", state: "", showAddressToClient: false },
  });

  const { fields: phoneFields, append: appendPhone, remove: removePhone } = useFieldArray({ control: form.control, name: "phones" });

  const { data: response, isLoading } = useQuery({ queryKey: ["providerMe"], queryFn: () => apiMeGet() });

  useEffect(() => {
    if (response?.data?.data) {
      const provider = response.data.data;
      const bp = provider.businessProfile;
      const contact = bp?.contactInfo;
      const addr = bp?.primaryAddress;

      let mappedPhones = [];
      if (contact?.phoneNumber) mappedPhones.push({ number: contact.phoneNumber, isWhatsapp: false });
      if (contact?.additionalPhones) contact.additionalPhones.forEach((p) => mappedPhones.push({ number: p, isWhatsapp: false }));
      if (mappedPhones.length === 0) mappedPhones.push({ number: "", isWhatsapp: false });

      form.reset({
        fullName: provider.name || "",
        fantasyName: bp?.fantasyName || "",
        email: contact?.email || "",
        cpf: "Não disponível",
        phones: mappedPhones,
        cep: addr?.zipCode || "",
        address: addr?.street || "",
        number: addr?.number || "",
        neighborhood: addr?.neighborhood || "",
        city: addr?.city || "",
        state: addr?.state || "",
        showAddressToClient: bp?.showAddressToClient ?? false,
      });
    }
  }, [response, form]);

  const updateMutation = useMutation({
    mutationFn: (req) => apiMePut({ body: req }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["providerMe"] }); toast.success("Perfil atualizado com sucesso!"); router.push("/"); },
    onError: () => toast.error("Erro ao atualizar o perfil."),
  });

  const onSubmit = (data: ProfileFormValues) => {
    const primaryPhone = data.phones[0]?.number || "";
    const additionalPhones = data.phones.slice(1).map(p => p.number).filter(p => !!p);
    updateMutation.mutate({
      name: data.fullName,
      businessProfile: {
        fantasyName: data.fantasyName,
        showAddressToClient: data.showAddressToClient,
        contactInfo: { email: data.email, phoneNumber: primaryPhone, additionalPhones },
        primaryAddress: { zipCode: data.cep, street: data.address, number: data.number, neighborhood: data.neighborhood, city: data.city, state: data.state, country: "Brasil" },
      },
    });
  };

  return (
    <div className="container mx-auto max-w-2xl py-8 px-4">
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild className="h-8 w-8 px-0">
          <Link href="/"><span className="text-xl">&lsaquo;</span><span className="sr-only">Voltar</span></Link>
        </Button>
        <span className="font-bold">AjudaAí</span>
        <div className="ml-auto text-sm font-medium hover:underline cursor-pointer">Sair</div>
      </div>

      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10">
        <h1 className="mb-8 text-2xl font-bold tracking-tight text-foreground">Alterar dados</h1>

        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-6">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="fullName">Nome completo</Label>
            <Input id="fullName" {...form.register("fullName")} />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="fantasyName">Nome fantasia</Label>
            <Input id="fantasyName" {...form.register("fantasyName")} />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="email">E-mail</Label>
            <Input id="email" type="email" {...form.register("email")} />
          </div>

          {/* Telefones */}
          <div className="flex flex-col gap-3">
            <Label>Telefones principais</Label>
            <div className="flex items-center gap-2">
              <Input placeholder="Adicione um novo telefone" className="w-full text-sm h-9" id="new-phone" />
              <Button type="button" size="sm" onClick={() => {
                const input = document.getElementById("new-phone") as HTMLInputElement;
                if (input.value) { appendPhone({ number: input.value, isWhatsapp: false }); input.value = ""; }
              }} className="h-9 w-9 px-0 shrink-0"><Plus className="h-4 w-4" /></Button>
            </div>
            
            {phoneFields.length > 0 && (
              <div className="mt-2 text-sm">
                <div className="mb-2 grid grid-cols-[1fr_80px_60px] gap-4 font-semibold text-xs text-foreground-subtle uppercase tracking-wide">
                  <div>Número</div><div className="text-center">Whatsapp?</div><div className="text-center">Excluir</div>
                </div>
                {phoneFields.map((field, index) => (
                  <div key={field.id} className="grid grid-cols-[1fr_80px_60px] items-center gap-4 py-2 border-b border-border/50 last:border-0 hover:bg-muted/50">
                    <span className="tabular-nums opacity-90">{field.number}</span>
                    <div className="flex justify-center"><input type="checkbox" {...form.register(`phones.${index}.isWhatsapp`)} className="h-4 w-4 accent-emerald-500" /></div>
                    <div className="flex justify-center"><button type="button" onClick={() => removePhone(index)} className="text-destructive hover:bg-destructive/10 p-1.5 rounded-md"><Trash2 className="h-4 w-4" /></button></div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Endereço */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="cep">CEP</Label>
            <Input id="cep" {...form.register("cep")} />
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="address">Logradouro</Label>
            <Input id="address" {...form.register("address")} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="number">Número</Label>
              <Input id="number" {...form.register("number")} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="neighborhood">Bairro</Label>
              <Input id="neighborhood" {...form.register("neighborhood")} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="city">Cidade</Label>
              <Input id="city" {...form.register("city")} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="state">Estado</Label>
              <Input id="state" {...form.register("state")} />
            </div>
          </div>

          <div className="flex items-center justify-between border-t border-border pt-6">
            <Label htmlFor="showAddress" className="font-semibold text-foreground">Mostrar endereço para meu cliente?</Label>
            <input type="checkbox" {...form.register("showAddressToClient")} className="h-6 w-11 rounded-full bg-border appearance-none after:absolute after:left-0.5 after:top-0.5 after:h-5 after:w-5 after:rounded-full after:bg-white after:transition-all checked:bg-emerald-500 checked:after:translate-x-5" />
          </div>

          <div className="mt-8 flex justify-center gap-4 border-t border-border pt-8">
            <Button variant="ghost" type="button" onClick={() => router.back()}>Cancelar</Button>
            <Button type="submit" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? "Salvando..." : "Salvar Alterações"}
            </Button>
          </div>
        </form>
      </main>
    </div>
  );
}
```

---

# 3. COMPONENTS - UI

## `components/ui/button.tsx`

```tsx
import * as React from "react";
import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0",
  {
    variants: {
      variant: {
        default: "bg-primary text-primary-foreground hover:bg-primary-hover",
        destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90",
        outline: "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        secondary: "bg-secondary text-secondary-foreground hover:bg-secondary-hover",
        ghost: "hover:bg-accent hover:text-accent-foreground",
        link: "text-primary underline-offset-4 hover:underline",
      },
      size: {
        default: "h-10 px-4 py-2",
        sm: "h-9 rounded-md px-3",
        lg: "h-11 rounded-md px-8",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: { variant: "default", size: "default" },
  }
);

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement>, VariantProps<typeof buttonVariants> { asChild?: boolean; }

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button";
    return <Comp className={buttonVariants({ variant, size, className })} ref={ref} {...props} />;
  }
);
Button.displayName = "Button";

export { Button, buttonVariants };
```

---

## `components/ui/card.tsx`

```tsx
import * as React from "react";
import { cn } from "@/lib/utils";

const Card = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn("rounded-lg border bg-card text-card-foreground shadow-sm", className)} {...props} />
  )
);
Card.displayName = "Card";

const CardHeader = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn("flex flex-col space-y-1.5 p-6", className)} {...props} />
  )
);
CardHeader.displayName = "CardHeader";

const CardTitle = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLHeadingElement>>(
  ({ className, ...props }, ref) => (
    <h3 ref={ref} className={cn("text-2xl font-semibold leading-none tracking-tight", className)} {...props} />
  )
);
CardTitle.displayName = "CardTitle";

const CardContent = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn("p-6 pt-0", className)} {...props} />
  )
);
CardContent.displayName = "CardContent";

export { Card, CardHeader, CardTitle, CardContent };
```

---

## `components/ui/input.tsx`

```tsx
import * as React from "react";
import { cn } from "@/lib/utils";

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, ...props }, ref) => {
    return (
      <input
        type={type}
        className={cn("flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50", className)}
        ref={ref}
        {...props}
      />
    );
  }
);
Input.displayName = "Input";

export { Input };
```

---

## `components/ui/label.tsx`

```tsx
"use client";

import * as React from "react";
import * as LabelPrimitive from "@radix-ui/react-label";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const labelVariants = cva("text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70");

const Label = React.forwardRef<
  React.ElementRef<typeof LabelPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof LabelPrimitive.Root> & VariantProps<typeof labelVariants>
>(({ className, ...props }, ref) => (
  <LabelPrimitive.Root ref={ref} className={cn(labelVariants(), className)} {...props} />
));
Label.displayName = LabelPrimitive.Root.displayName;

export { Label };
```

---

## `components/ui/file-upload.tsx`

```tsx
"use client";

import { useCallback, useState } from "react";
import { Upload, X, File } from "lucide-react";
import { cn } from "@/lib/utils";

interface FileUploadProps {
  label: string;
  description?: string;
  accept?: string;
  required?: boolean;
  onFileSelect?: (file: File) => void;
}

export function FileUpload({ label, description, accept, required, onFileSelect }: FileUploadProps) {
  const [dragActive, setDragActive] = useState(false);
  const [file, setFile] = useState<File | null>(null);

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") setDragActive(true);
    else if (e.type === "dragleave") setDragActive(false);
  }, []);

  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]) {
      const selectedFile = e.target.files[0];
      setFile(selectedFile);
      onFileSelect?.(selectedFile);
    }
  }, [onFileSelect]);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      const droppedFile = e.dataTransfer.files[0];
      setFile(droppedFile);
      onFileSelect?.(droppedFile);
    }
  }, [onFileSelect]);

  const removeFile = () => { setFile(null); };

  return (
    <div>
      <label className="text-sm font-medium text-foreground">{label} {required && <span className="text-destructive">*</span>}</label>
      {description && <p className="mt-1 text-xs text-muted-foreground">{description}</p>}
      
      {!file ? (
        <div
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
          className={cn("mt-2 flex flex-col items-center justify-center rounded-lg border-2 border-dashed p-6 transition-colors", dragActive ? "border-primary bg-primary/5" : "border-border hover:border-primary/50")}
        >
          <Upload className="mb-2 h-8 w-8 text-muted-foreground" />
          <span className="text-sm text-muted-foreground">Arraste o arquivo aqui ou <span className="text-primary font-medium cursor-pointer">clique para selecionar</span></span>
          <input type="file" accept={accept} onChange={handleChange} className="absolute inset-0 z-10 h-full w-full cursor-pointer opacity-0" />
        </div>
      ) : (
        <div className="mt-2 flex items-center justify-between rounded-lg border p-4">
          <div className="flex items-center gap-3">
            <File className="h-8 w-8 text-primary" />
            <div className="flex flex-col">
              <span className="text-sm font-medium">{file.name}</span>
              <span className="text-xs text-muted-foreground">{(file.size / 1024 / 1024).toFixed(2)} MB</span>
            </div>
          </div>
          <button type="button" onClick={removeFile} className="text-muted-foreground hover:text-destructive"><X className="h-5 w-5" /></button>
        </div>
      )}
    </div>
  );
}
```

---

# 4. COMPONENTS - LAYOUT

## `components/layout/header.tsx`

```tsx
"use client";

import Link from "next/link";
import { useSession, signOut } from "next-auth/react";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Menu, X, LogOut, Settings } from "lucide-react";
import { useState } from "react";

export function Header() {
  const { data: session } = useSession();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur">
      <div className="container mx-auto flex h-14 items-center justify-between px-4">
        <Link href="/" className="flex items-center gap-2">
          <span className="text-lg font-bold text-primary">MeAjudaAí</span>
        </Link>

        {session ? (
          <div className="flex items-center gap-4">
            <nav className="hidden md:flex items-center gap-6">
              <Link href="/alterar-dados" className="text-sm font-medium hover:text-primary">Editar Perfil</Link>
              <Link href="/configuracoes" className="text-sm font-medium hover:text-primary">Configurações</Link>
            </nav>
            
            <div className="flex items-center gap-2">
              <Avatar className="h-8 w-8">
                <AvatarFallback>{session.user?.name?.charAt(0).toUpperCase() ?? "P"}</AvatarFallback>
              </Avatar>
              <button onClick={() => signOut()} className="text-muted-foreground hover:text-foreground">
                <LogOut className="h-5 w-5" />
              </button>
            </div>
          </div>
        ) : (
          <Link href="/register">
            <Button size="sm">Cadastrar</Button>
          </Link>
        )}

        <button className="md:hidden" onClick={() => setMobileMenuOpen(!mobileMenuOpen)}>
          {mobileMenuOpen ? <X /> : <Menu />}
        </button>
      </div>

      {mobileMenuOpen && (
        <div className="md:hidden border-t p-4">
          {session ? (
            <nav className="flex flex-col gap-2">
              <Link href="/alterar-dados" className="py-2 text-sm font-medium">Editar Perfil</Link>
              <Link href="/configuracoes" className="py-2 text-sm font-medium">Configurações</Link>
              <button onClick={() => signOut()} className="py-2 text-sm font-medium text-destructive">Sair</button>
            </nav>
          ) : (
            <Link href="/register"><Button className="w-full">Cadastrar</Button></Link>
          )}
        </div>
      )}
    </header>
  );
}
```

---

## `components/layout/footer.tsx`

```tsx
import Link from "next/link";

export function Footer() {
  return (
    <footer className="border-t bg-surface">
      <div className="container mx-auto px-4 py-6">
        <div className="flex flex-col items-center gap-4 md:flex-row md:justify-between">
          <div className="flex items-center gap-2">
            <span className="text-lg font-bold text-primary">MeAjudaAí</span>
            <span className="text-xs text-muted-foreground">Prestador</span>
          </div>
          
          <nav className="flex gap-6 text-sm">
            <Link href="/termos" className="text-muted-foreground hover:text-foreground">Termos de Uso</Link>
            <Link href="/privacidade" className="text-muted-foreground hover:text-foreground">Política de Privacidade</Link>
          </nav>
        </div>
        
        <div className="mt-4 text-center text-xs text-muted-foreground">
          <p>&copy; {new Date().getFullYear()} MeAjudaAí. Todos os direitos reservados.</p>
        </div>
      </div>
    </footer>
  );
}
```

---

# 5. COMPONENTS - DASHBOARD

## `components/dashboard/verification-card.tsx`

```tsx
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CheckCircle, Clock, AlertCircle } from "lucide-react";
import { Badge } from "@/components/ui/badge";

interface VerificationCardProps {
  status: number;
}

export function VerificationCard({ status }: VerificationCardProps) {
  const getStatusInfo = () => {
    switch (status) {
      case 1: return { label: "Pendente", color: "bg-yellow-100 text-yellow-800", icon: Clock };
      case 2: return { label: "Em análise", color: "bg-blue-100 text-blue-800", icon: Clock };
      case 3: return { label: "Aprovado", color: "bg-green-100 text-green-800", icon: CheckCircle };
      case 4: return { label: "Rejeitado", color: "bg-red-100 text-red-800", icon: AlertCircle };
      default: return { label: "Não verificado", color: "bg-gray-100 text-gray-800", icon: AlertCircle };
    }
  };

  const { label, color, icon: Icon } = getStatusInfo();

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Status de Verificação</CardTitle>
      </CardHeader>
      <CardContent>
        <div className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-sm font-medium ${color}`}>
          <Icon className="h-4 w-4" />
          {label}
        </div>
      </CardContent>
    </Card>
  );
}
```

---

## `components/dashboard/services-configuration-card.tsx`

```tsx
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Wrench } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";

interface ServicesConfigurationCardProps {
  servicesCount: number;
}

export function ServicesConfigurationCard({ servicesCount }: ServicesConfigurationCardProps) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center gap-2 text-sm font-medium">
          <Wrench className="h-4 w-4" />
          Meus Serviços
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-bold">{servicesCount}</p>
        <p className="text-sm text-muted-foreground">serviços cadastrados</p>
        <Link href="/alterar-dados" className="mt-4 inline-block">
          <Button variant="outline" size="sm">Editar Serviços</Button>
        </Link>
      </CardContent>
    </Card>
  );
}
```

---

## `components/dashboard/profile-status-card.tsx`

```tsx
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Eye, EyeOff } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";

interface ProfileStatusCardProps {
  isActive: boolean;
}

export function ProfileStatusCard({ isActive }: ProfileStatusCardProps) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Visibilidade do Perfil</CardTitle>
      </CardHeader>
      <CardContent>
        <div className={`inline-flex items-center gap-2 text-sm font-medium ${isActive ? "text-green-600" : "text-red-600"}`}>
          {isActive ? <Eye className="h-4 w-4" /> : <EyeOff className="h-4 w-4" />}
          {isActive ? "Visível nas buscas" : "Oculto das buscas"}
        </div>
        <Link href="/configuracoes" className="mt-4 inline-block">
          <Button variant="outline" size="sm">Configurações</Button>
        </Link>
      </CardContent>
    </Card>
  );
}
```

---

# 6. COMPONENTS - PROFILE

## `components/profile/profile-header.tsx`

```tsx
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Mail, Phone } from "lucide-react";

interface ProfileHeaderProps {
  name: string;
  email: string;
  isOnline: boolean;
  phones: string[];
  rating: number;
}

export function ProfileHeader({ name, email, isOnline, phones, rating }: ProfileHeaderProps) {
  return (
    <div className="flex flex-col items-center text-center sm:flex-row sm:text-left gap-6">
      <Avatar className="h-24 w-24 border-4 border-white shadow-lg text-3xl font-bold">
        <AvatarFallback className="bg-primary text-primary-foreground">{name.substring(0, 2).toUpperCase()}</AvatarFallback>
      </Avatar>

      <div className="flex-1 space-y-2">
        <div className="flex flex-col items-center sm:flex-row sm:items-center gap-2">
          <h1 className="text-2xl font-bold text-foreground">{name}</h1>
          {isOnline && <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-medium text-emerald-700">Ativo</span>}
        </div>

        <div className="flex flex-col items-center sm:flex-row gap-4 text-sm text-muted-foreground">
          <div className="flex items-center gap-1">
            <Mail className="h-4 w-4" />
            <span>{email || "Sem email"}</span>
          </div>
          <div className="flex items-center gap-1">
            <Phone className="h-4 w-4" />
            <span>{phones[0] || "Sem telefone"}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
```

---

## `components/profile/profile-description.tsx`

```tsx
interface ProfileDescriptionProps {
  description: string;
}

export function ProfileDescription({ description }: ProfileDescriptionProps) {
  return (
    <div className="mt-6 border-t pt-6">
      <h2 className="mb-2 text-sm font-semibold text-foreground">Sobre</h2>
      <p className="text-sm leading-relaxed text-muted-foreground">{description}</p>
    </div>
  );
}
```

---

## `components/profile/profile-services.tsx`

```tsx
import { Badge } from "@/components/ui/badge";
import { Wrench } from "lucide-react";

interface ProfileServicesProps {
  services: string[];
}

export function ProfileServices({ services }: ProfileServicesProps) {
  if (services.length === 0) return null;

  return (
    <div className="mt-6 border-t pt-6">
      <h2 className="mb-3 flex items-center gap-2 text-sm font-semibold text-foreground">
        <Wrench className="h-4 w-4" />
        Serviços
      </h2>
      <div className="flex flex-wrap gap-2">
        {services.map((service, index) => (
          <Badge key={index} variant="secondary">{service}</Badge>
        ))}
      </div>
    </div>
  );
}
```

---

## `components/profile/profile-reviews.tsx`

```tsx
import { Review, Star } from "lucide-react";

interface ProfileReviewsProps {
  reviews: Array<{ id: string; author: string; rating: number; comment: string; createdAt: string }>;
}

export function ProfileReviews({ reviews }: ProfileReviewsProps) {
  return (
    <div className="mt-6 border-t pt-6">
      <h2 className="mb-4 flex items-center gap-2 text-sm font-semibold text-foreground">
        <Review className="h-4 w-4" />
        Avaliações
      </h2>

      {reviews.length === 0 ? (
        <p className="text-sm text-muted-foreground">Nenhuma avaliação ainda.</p>
      ) : (
        <div className="space-y-4">
          {reviews.map((review) => (
            <div key={review.id} className="rounded-lg border p-4">
              <div className="flex items-center justify-between">
                <span className="font-medium text-sm">{review.author}</span>
                <div className="flex items-center gap-1">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <Star key={i} className={`h-4 w-4 ${i < review.rating ? "fill-yellow-400 text-yellow-400" : "text-gray-300"}`} />
                  ))}
                </div>
              </div>
              <p className="mt-2 text-sm text-muted-foreground">{review.comment}</p>
              <span className="mt-1 block text-xs text-muted-foreground">
                {new Date(review.createdAt).toLocaleDateString("pt-BR")}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

---

## `components/providers/app-providers.tsx`

```tsx
"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { SessionProvider } from "next-auth/react";
import { useState } from "react";

export function AppProviders({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000,
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  return (
    <SessionProvider>
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    </SessionProvider>
  );
}
```

---

# 7. IMAGENS E ASSETS

## Imagens (Nomes dos arquivos)

O projeto Provider não possui imagens customizadas. Utiliza:
- Avatares de placeholder: `https://i.pravatar.cc/150`
- Sem imagens próprias no diretório `public/`

---

# 8. SISTEMA DE DESIGN

## Paleta de Cores

| Nome | Hex | Uso |
|------|-----|-----|
| Primary | `#395873` | Botões primários, backgrounds |
| Primary Hover | `#2E4760` | Hover de elementos primários |
| Secondary | `#D96704` | Cor secundária, botões CTA |
| Secondary Light | `#F2AE72` | Versão clara do secondary |
| Secondary Hover | `#B85703` | Hover de elementos secondary |
| Brand | `#E0702B` | Brand principal |
| Brand Hover | `#c56226` | Hover do brand |
| Destructive | `#dc2626` | Mensagens de erro, exclusão |

## Cores de Tema (Light Mode)

| Variável CSS | Hex | Uso |
|--------------|-----|-----|
| `--background` | `#ffffff` | Background principal |
| `--foreground` | `#2e2e2e` | Cor do texto principal |
| `--surface` | `#ffffff` | Cards, elementos elevados |
| `--surface-raised` | `#f5f5f5` | Elementos levemente elevados |
| `--foreground-subtle` | `#666666` | Texto secundário |
| `--border` | `#e0e0e0` | Bordas |
| `--input` | `#e0e0e0` | Campos de input |
| `--card` | `#ffffff` | Cards |
| `--muted` | `#f5f5f5` | Backgrounds sutis |

## Cores de Tema (Dark Mode)

| Variável CSS | Hex | Uso |
|--------------|-----|-----|
| `--background` | `#0a0a0a` | Background principal |
| `--foreground` | `#ededed` | Cor do texto principal |
| `--surface` | `#1a1a1a` | Cards |
| `--surface-raised` | `#262626` | Elementos elevados |
| `--foreground-subtle` | `#a3a3a3` | Texto secundário |
| `--border` | `#404040` | Bordas |
| `--input` | `#404040` | Campos de input |
| `--muted` | `#262626` | Backgrounds sutis |

## Tipografia

| Propriedade | Valor |
|--------------|-------|
| Font Family | `'Roboto', Arial, Helvetica, sans-serif` |
| H1 | `text-2xl font-bold` |
| H2 | `text-xl font-semibold` |
| Body | `text-sm` |

## Breakpoints

| Breakpoint | Valor |
|------------|-------|
| Mobile | `< 640px` (default) |
| Tablet | `md: 768px` |
| Desktop | `lg: 1024px` |

## Dependências de UI Principais

| Biblioteca | Uso |
|------------|-----|
| `@radix-ui/react-slot` | Componente Slot para asChild |
| `@radix-ui/react-label` | Labels acessíveis |
| `lucide-react` | Ícones |
| `tailwindcss` v4 | CSS Framework |
| `class-variance-authority` | Variantes de componentes |
| `react-hook-form` | Formulários |
| `@hookform/resolvers/zod` | Validação com Zod |
| `@tanstack/react-query` | Data fetching |

---

*Documento gerado em: 2026-03-21*
*Projeto: MeAjudaAi.Web.Provider*
