# MeAjudaAi.Web.Customer - Documentação Completa de Frontend

Este documento contém toda a estrutura HTML, CSS e informações de design do projeto MeAjudaAi.Web.Customer.

---

# ÍNDICE

1. [CSS Global](#1-css-global)
2. [Pages (Rotas)](#2-pages-rotas)
3. [Components - UI](#3-components---ui)
4. [Components - Layout](#4-components---layout)
5. [Components - Features](#5-components---features)
6. [Imagens e Assets](#6-imagens-e-assets)
7. [Sistema de Design](#7-sistema-de-design)

---

# 1. CSS GLOBAL

## Arquivo: `app/globals.css`

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

## ROTA: `/` (Home)
**Arquivo:** `app/(main)/page.tsx`

```tsx
import { CheckCircle2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { AdBanner } from "@/components/ui/ad-banner";
import { CitySearch } from "@/components/search/city-search";
import Image from "next/image";
import { HowItWorks } from "@/components/home/how-it-works";

export default function HomePage() {
  return (
    <div className="flex flex-col min-h-screen">
      <AdBanner />

      {/* Hero Section - White Background */}
      <section className="bg-white py-20 pb-10">
        <div className="container mx-auto px-4">
          <div className="flex flex-col items-start text-left max-w-4xl mb-12">
            <h1 className="flex flex-col gap-2 font-bold">
              <span className="text-2xl text-black">Conectando quem precisa com</span>
              <span className="text-5xl md:text-6xl text-secondary">quem sabe fazer.</span>
            </h1>
          </div>

          {/* City Search - Center aligned */}
          <div className="w-full max-w-2xl mx-auto">
            <CitySearch />
          </div>
        </div>
      </section>

      {/* Blue Section - Conheça */}
      <section className="bg-primary text-white py-20 overflow-hidden relative">
        <div className="container mx-auto px-4 flex flex-col md:flex-row items-center gap-12">
          <div className="flex-1 space-y-6">
            <h2 className="text-4xl font-bold text-white">
              Conheça o MeAjudaAí
            </h2>
            <p className="text-xl text-blue-50">
              Você já precisou de algum serviço e não sabia de nenhuma referência
              ou alguém que conhecia alguém que faça esse serviço que você está
              precisando?
            </p>
            <p className="text-xl text-blue-50">
              Nós nascemos para solucionar esse problema, uma plataforma que
              conecta quem está oferecendo serviço com quem está prestando
              serviço. Oferecemos métodos de avaliação dos serviços prestados,
              você consegue saber se o prestador possui boas indicações com
              base nos serviços já prestados por ele pela nossa plataforma.
            </p>
          </div>

          <div className="flex-1 relative h-[500px] w-full hidden md:block">
            <Image
              src="/illustration-woman.png"
              alt="Conheça o MeAjudaAí"
              fill
              className="object-contain object-center z-10"
              priority
              sizes="(max-width: 768px) 100vw, 50vw"
            />
          </div>
        </div>
      </section>

      {/* How It Works Section */}
      <section className="bg-white py-20 pb-8">
        <div className="container mx-auto px-4">
          <HowItWorks />
        </div>
      </section>

      {/* CTA Prestadores */}
      <section className="py-20 bg-white">
        <div className="container mx-auto px-4">
          <div className="flex flex-col md:flex-row items-center gap-12">

            <div className="flex-1 relative h-[500px] w-full hidden md:block order-2 md:order-1">
              <Image
                src="/illustration-man.png"
                alt="Seja um prestador"
                fill
                className="object-contain object-center mix-blend-multiply"
                sizes="(max-width: 768px) 100vw, 50vw"
              />
            </div>

            <div className="flex-1 space-y-6 order-1 md:order-2">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-secondary/10 rounded-full">
                  <CheckCircle2 className="size-6 text-secondary" />
                </div>
                <h2 className="text-3xl font-bold text-foreground">
                  Você é prestador de serviço?
                </h2>
              </div>

              <p className="text-xl text-foreground-subtle">
                Faça seu cadastro na nossa plataforma, cadastre seus serviços,
                meios de contato e apareça para seus clientes, tenha boas
                recomendações e destaque-se frente aos seus concorrentes.
              </p>

              <p className="text-xl text-foreground-subtle">
                Não importa qual tipo de serviço você presta, sempre tem alguém
                precisando de uma ajuda! Conseguimos fazer com que o seu cliente
                te encontre, você estará na vitrine virtual mais cobiçada do Brasil.
              </p>

              <Button size="lg" className="bg-secondary hover:bg-secondary-hover text-white mt-4" asChild>
                <a href="/auth/signin">Cadastre-se grátis</a>
              </Button>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
```

---

## ROTA: `/buscar`
**Arquivo:** `app/(main)/buscar/page.tsx`

```tsx
import { Suspense } from "react";
import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ServiceCard } from "@/components/service/service-card";
import { AdCard } from "@/components/search/ad-card";
import { ServiceTags } from "@/components/search/service-tags";
import { SearchFilters } from "@/components/search/search-filters";

export default async function SearchPage({ searchParams }) {
    // ... (server component com fetch de providers)

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Search Bar Centered */}
            <div className="max-w-3xl mx-auto mb-8">
                <form action="/buscar" method="get" role="search" className="relative flex items-center gap-2">
                    <div className="relative w-full">
                        <input
                            name="q"
                            type="search"
                            placeholder={cityFilter ? `Buscar em ${cityFilter}...` : "Buscar serviço"}
                            defaultValue={searchQuery}
                            className="w-full pl-6 pr-14 py-4 border border-orange-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-orange-500 text-lg placeholder:text-foreground-subtle"
                        />
                        <Button
                            type="submit"
                            size="icon"
                            className="absolute right-2 top-1/2 -translate-y-1/2 bg-[#E0702B] hover:bg-[#c56226] h-10 w-10 rounded-md"
                        >
                            <Search className="h-5 w-5 text-white" />
                        </Button>
                    </div>
                </form>
            </div>

            <div className="flex flex-col lg:flex-row gap-8">
                {/* Sidebar Filters */}
                <aside className="w-full lg:w-64 shrink-0">
                    <Suspense fallback={<div className="h-96 w-full bg-gray-100 rounded-xl animate-pulse" />}>
                        <SearchFilters />
                    </Suspense>
                </aside>

                <main className="flex-1">
                    {/* Service Tags */}
                    <div className="mb-8">
                        <Suspense fallback={<div className="h-10 w-full bg-gray-100 rounded-full animate-pulse" />}>
                            <ServiceTags />
                        </Suspense>
                    </div>

                    {/* Provider Grid */}
                    {providers.length > 0 ? (
                        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-3">
                            {gridItems.map((item, index) => {
                                if (item.type === 'ad') {
                                    return <AdCard key={`ad-${index}`} />;
                                }
                                return (
                                    <ServiceCard
                                        key={provider.id}
                                        id={provider.id}
                                        name={provider.name}
                                        avatarUrl={provider.avatarUrl ?? undefined}
                                        description={provider.description || "Prestador de serviços disponível para te atender."}
                                        services={provider.services.map(s => s.serviceName).filter((s): s is string => !!s)}
                                        rating={provider.averageRating ?? 0}
                                        reviewCount={provider.reviewCount ?? 0}
                                    />
                                );
                            })}
                        </div>
                    ) : (
                        <div className="text-center py-16 bg-gray-50 rounded-xl border border-dashed border-gray-200">
                            <Search className="mx-auto mb-4 h-16 w-16 text-gray-300" />
                            <h3 className="text-xl font-semibold text-foreground mb-2">
                                Nenhum prestador encontrado
                            </h3>
                        </div>
                    )}
                </main>
            </div>
        </div>
    );
}
```

---

## ROTA: `/prestador/[id]`
**Arquivo:** `app/(main)/prestador/[id]/page.tsx`

```tsx
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";
import { Badge } from "@/components/ui/badge";
import { MessageCircle } from "lucide-react";
import { VerifiedBadge } from "@/components/ui/verified-badge";
import { getWhatsappLink } from "@/lib/utils/phone";

export default async function ProviderProfilePage({ params }) {
    const { id } = await params;
    const providerData = await getCachedProvider(id);

    return (
        <div className="container mx-auto px-4 py-8">
            <div className="max-w-4xl mx-auto">
                <div className="grid grid-cols-1 md:grid-cols-12 gap-8 mb-12">
                    {/* Left Column: Avatar, Rating, Phones */}
                    <div className="md:col-span-3 flex flex-col items-center space-y-4">
                        <Avatar
                            src={undefined}
                            alt={displayName}
                            fallback={displayName.substring(0, 2).toUpperCase()}
                            containerClassName="h-32 w-32 border-4 border-white shadow-md text-3xl font-bold"
                        />

                        <div className="flex items-center gap-2">
                            <Rating value={rating} className="text-[#E0702B]" />
                            {reviewCount > 0 && (
                                <span className="text-sm text-gray-600">({reviewCount} avaliações)</span>
                            )}
                        </div>

                        {phones.length > 0 ? (
                            <div className="w-full space-y-2">
                                {phones.map((phone, i) => (
                                    <div key={i} className="flex items-center gap-2 text-gray-600 text-sm">
                                        <span className="font-medium">{phone}</span>
                                        <a href={getWhatsappLink(phone)} target="_blank" rel="noopener noreferrer">
                                            <MessageCircle className="w-4 h-4 text-green-500" />
                                        </a>
                                    </div>
                                ))}
                            </div>
                        ) : isAuthenticated ? (
                            <div className="w-full p-4 bg-blue-50 border border-blue-100 rounded-lg text-center">
                                <p className="text-sm text-gray-700">Este prestador não informou contatos.</p>
                            </div>
                        ) : (
                            <div className="w-full p-4 bg-orange-50 border border-orange-100 rounded-lg text-center">
                                <p className="text-sm text-gray-700 mb-2">Faça login para visualizar os contatos.</p>
                            </div>
                        )}
                    </div>

                    {/* Right Column: Name, Email, Description, Services */}
                    <div className="md:col-span-9 space-y-4">
                        <div className="flex items-center gap-3">
                            <h1 className="text-3xl md:text-4xl font-bold text-[#E0702B]">{displayName}</h1>
                            <VerifiedBadge status={providerData.verificationStatus ?? undefined} size="lg" />
                        </div>

                        {providerData.email && (
                            <p className="text-gray-500 font-medium text-sm lowercase">{providerData.email}</p>
                        )}

                        <div className="text-gray-600 leading-relaxed text-justify">
                            <p>{description}</p>
                        </div>

                        {services.length > 0 && (
                            <div className="pt-4">
                                <h2 className="text-lg font-bold text-gray-900 mb-3">Serviços</h2>
                                <div className="flex flex-wrap gap-2">
                                    {services.map((service, i) => (
                                        <Badge key={i} className="px-3 py-1 bg-[#E0702B] text-white text-sm rounded-full">
                                            {service}
                                        </Badge>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            {/* Comments Section */}
            <div className="pt-8 border-t border-gray-200">
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-xl font-bold text-gray-900">Comentários</h2>
                </div>
                <div className="space-y-6">
                    <div className="bg-gray-50 p-3 rounded-lg">
                        <ReviewForm providerId={id} />
                    </div>
                    <ReviewList providerId={id} />
                </div>
            </div>
        </div>
    );
}
```

---

## ROTA: `/prestador` (Dashboard Provider)
**Arquivo:** `app/(main)/prestador/page.tsx`

```tsx
import DashboardClient from "@/components/providers/dashboard-client";

export default async function DashboardPage() {
    // Server component que busca dados do provider e renderiza DashboardClient
    return <DashboardClient provider={provider} />;
}
```

---

## ROTA: `/perfil`
**Arquivo:** `app/(main)/perfil/page.tsx`

```tsx
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { User, Mail, Phone, MapPin, Pencil } from "lucide-react";
import Link from "next/link";

export default async function ProfilePage() {
    return (
        <div className="container mx-auto px-4 py-8 max-w-4xl">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-8 gap-4">
                <h1 className="text-3xl font-bold">Meu Perfil</h1>
                <Button asChild>
                    <Link href="/perfil/editar">
                        <Pencil className="mr-2 size-4" />
                        Editar Perfil
                    </Link>
                </Button>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="text-xl">Informações Pessoais</CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                    <div className="grid gap-6 md:grid-cols-2">
                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <User className="size-4" /> Nome Completo
                            </h4>
                            <p className="font-medium text-lg">{user.fullName}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <Mail className="size-4" /> Email
                            </h4>
                            <p className="font-medium text-lg">{user.email}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <Phone className="size-4" /> Telefone
                            </h4>
                            <p className="font-medium text-lg">{"Não informado"}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <MapPin className="size-4" /> Localização
                            </h4>
                            <p className="font-medium text-lg">{"Não informado"}</p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
```

---

## ROTA: `/perfil/editar`
**Arquivo:** `app/(main)/perfil/editar/page.tsx`

```tsx
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { EditProfileForm } from "@/components/profile/edit-profile-form";

export default async function EditProfilePage() {
    return (
        <div className="container mx-auto px-4 py-8 max-w-2xl">
            <h1 className="text-3xl font-bold mb-8">Editar Perfil</h1>

            <Card>
                <CardHeader>
                    <CardTitle>Dados Pessoais</CardTitle>
                </CardHeader>
                <CardContent>
                    <EditProfileForm
                        userId={session.user.id}
                        initialData={{
                            firstName: user.firstName ?? "",
                            lastName: user.lastName ?? "",
                            email: user.email ?? "",
                        }}
                    />
                </CardContent>
            </Card>
        </div>
    );
}
```

---

## ROTA: `/cadastro/prestador`
**Arquivo:** `app/(main)/cadastro/prestador/page.tsx`

```tsx
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Checkbox } from "@/components/ui/checkbox";
import { ShieldCheck, Info } from "lucide-react";
import Link from "next/link";

export default function RegisterProviderPage() {
    const form = useForm<RegisterProviderSchema>({
        resolver: zodResolver(registerProviderSchema),
        defaultValues: {
            name: "",
            type: EProviderType.Individual,
            documentNumber: "",
            phoneNumber: "",
            email: "",
            acceptedTerms: false,
            acceptedPrivacyPolicy: false,
        },
    });

    return (
        <div className="w-full max-w-md mx-auto space-y-8 px-4 py-8 mt-12 mb-12">
            {/* Stepper */}
            <div className="flex items-center justify-between w-full max-w-xs mx-auto mb-8 relative">
                <div className="absolute left-0 top-1/2 -translate-y-1/2 w-full h-0.5 bg-border z-0"></div>

                <div className="relative z-10 flex flex-col items-center gap-2 bg-background px-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground font-semibold text-sm ring-4 ring-background">
                        1
                    </div>
                    <span className="text-xs font-medium text-primary absolute -bottom-6 whitespace-nowrap">Dados Iniciais</span>
                </div>

                <div className="relative z-10 flex flex-col items-center gap-2 bg-background px-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-muted-foreground font-semibold text-sm ring-4 ring-background">
                        2
                    </div>
                    <span className="text-xs font-medium text-muted-foreground absolute -bottom-6 whitespace-nowrap">Endereço</span>
                </div>

                <div className="relative z-10 flex flex-col items-center gap-2 bg-background px-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-muted-foreground font-semibold text-sm ring-4 ring-background">
                        3
                    </div>
                    <span className="text-xs font-medium text-muted-foreground absolute -bottom-6 whitespace-nowrap">Documentos</span>
                </div>
            </div>

            <div className="text-center pt-4">
                <h1 className="text-2xl font-bold tracking-tight">
                    Passo 1: Crie sua conta
                </h1>
                <p className="text-muted-foreground mt-2 text-sm">
                    Inicie seu credenciamento. Nas próximas etapas, pediremos seu endereço e documentos.
                </p>
            </div>

            <Form {...form}>
                <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-7">
                    <FormField control={form.control} name="name" render={({ field }) => (
                        <FormItem>
                            <FormLabel>Nome Completo (ou Razão Social)</FormLabel>
                            <FormControl>
                                <Input placeholder="Seu nome" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )} />

                    <FormField control={form.control} name="type" render={({ field }) => (
                        <FormItem>
                            <FormLabel>Tipo de Pessoa</FormLabel>
                            <FormControl>
                                <div className="flex flex-wrap gap-4">
                                    <Button type="button" variant={field.value === EProviderType.Individual ? "primary" : "outline"} onClick={() => field.onChange(EProviderType.Individual)}>
                                        Pessoa Física (CPF)
                                    </Button>
                                    <Button type="button" variant={field.value === EProviderType.Company ? "primary" : "outline"} onClick={() => field.onChange(EProviderType.Company)}>
                                        Pessoa Jurídica (CNPJ)
                                    </Button>
                                    <Button type="button" variant={field.value === EProviderType.Cooperative ? "primary" : "outline"} onClick={() => field.onChange(EProviderType.Cooperative)}>
                                        Cooperativa
                                    </Button>
                                    <Button type="button" variant={field.value === EProviderType.Freelancer ? "primary" : "outline"} onClick={() => field.onChange(EProviderType.Freelancer)}>
                                        Autônomo
                                    </Button>
                                </div>
                            </FormControl>
                        </FormItem>
                    )} />

                    {/* Checkboxes de termos */}
                    <FormField control={form.control} name="acceptedTerms" render={({ field }) => (
                        <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4 shadow-sm">
                            <FormControl>
                                <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                            </FormControl>
                            <div className="space-y-1 leading-none">
                                <FormLabel>
                                    Aceito os <Link href="/termos-de-uso" target="_blank" className="underline hover:text-primary">Termos de Uso</Link>
                                </FormLabel>
                            </div>
                        </FormItem>
                    )} />

                    <Button type="submit" size="lg" className="w-full mt-8 bg-secondary hover:bg-secondary-hover text-white text-lg font-semibold">
                        {isPending ? "Criando conta..." : "Continuar"}
                    </Button>
                </form>
            </Form>

            {/* Privacy Badge */}
            <div className="relative">
                <button type="button" onClick={() => setShowPrivacyInfo(!showPrivacyInfo)} className="w-full flex items-center gap-3 rounded-lg border border-green-200 bg-green-50 px-4 py-3">
                    <ShieldCheck className="h-5 w-5 text-green-600 shrink-0" />
                    <span className="text-sm font-medium text-green-800 flex-1">Privacidade e segurança</span>
                    <Info className="h-4 w-4 text-green-600 shrink-0" />
                </button>
            </div>
        </div>
    );
}
```

---

## ROTA: `/cadastro/prestador/perfil`
**Arquivo:** `app/(main)/cadastro/prestador/perfil/page.tsx`

```tsx
import { BasicInfoForm } from "@/components/providers/basic-info-form";

export default function BasicInfoPage() {
    return (
        <div className="w-full max-w-2xl mx-auto space-y-8 px-4 py-8 mt-12 mb-12">
            {/* Stepper com passo 1 ativo */}
            {/* Formulário BasicInfoForm */}
            <BasicInfoForm />
        </div>
    );
}
```

---

## ROTA: `/cadastro/prestador/perfil/endereco`
**Arquivo:** `app/(main)/cadastro/prestador/perfil/endereco/page.tsx`

```tsx
import { AddressForm } from "@/components/providers/address-form";

export default function AddressPage() {
    return (
        <div className="w-full max-w-2xl mx-auto space-y-8 px-4 py-8 mt-12 mb-12">
            {/* Stepper com passo 2 ativo */}
            <AddressForm />
        </div>
    );
}
```

---

## ROTA: `/cadastro/prestador/perfil/documentos`
**Arquivo:** `app/(main)/cadastro/prestador/perfil/documentos/page.tsx`

```tsx
import { DocumentUpload } from "@/components/providers/document-upload";

export default function DocumentsPage() {
    return (
        <div className="w-full max-w-2xl mx-auto space-y-8 px-4 py-8 mt-12 mb-12">
            {/* Stepper com passo 3 ativo */}
            <DocumentUpload />
        </div>
    );
}
```

---

## ROTA: `/auth/signin`
**Arquivo:** `app/(auth)/auth/signin/page.tsx`

```tsx
import { LoginForm } from "@/components/auth/login-form";
import { AuthSelectionDropdown } from "@/components/auth/auth-selection-dropdown";

export default function SignInPage() {
    return (
        <div className="min-h-screen flex items-center justify-center">
            <div className="w-full max-w-md space-y-8 px-4 py-8">
                <AuthSelectionDropdown />
                <LoginForm />
            </div>
        </div>
    );
}
```

---

## ROTA: `/auth/cadastro/cliente`
**Arquivo:** `app/(auth)/cadastro/cliente/page.tsx`

```tsx
import { CustomerRegisterForm } from "@/components/auth/customer-register-form";

export default function CustomerRegisterPage() {
    return (
        <div className="min-h-screen flex items-center justify-center">
            <div className="w-full max-w-md space-y-8 px-4 py-8">
                <CustomerRegisterForm />
            </div>
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
        primary: "bg-primary text-primary-foreground hover:bg-primary-hover",
      },
      size: {
        default: "h-10 px-4 py-2",
        sm: "h-9 rounded-md px-3",
        lg: "h-11 rounded-md px-8",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button";
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    );
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

## `components/ui/badge.tsx`

```tsx
import * as React from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const badgeVariants = cva(
  "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
  {
    variants: {
      variant: {
        default: "border-transparent bg-primary text-primary-foreground hover:bg-primary/80",
        secondary: "border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80",
        destructive: "border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80",
        outline: "text-foreground",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
);

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />;
}

export { Badge, badgeVariants };
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
        className={cn(
          "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
          className
        )}
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

## `components/ui/textarea.tsx`

```tsx
import * as React from "react";
import { cn } from "@/lib/utils";

export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {}

const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ className, ...props }, ref) => {
    return (
      <textarea
        className={cn(
          "flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
          className
        )}
        ref={ref}
        {...props}
      />
    );
  }
);
Textarea.displayName = "Textarea";

export { Textarea };
```

---

## `components/ui/checkbox.tsx`

```tsx
import * as React from "react";
import * as CheckboxPrimitive from "@radix-ui/react-checkbox";
import { Check } from "lucide-react";
import { cn } from "@/lib/utils";

const Checkbox = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof CheckboxPrimitive.Root>
>(({ className, ...props }, ref) => (
  <CheckboxPrimitive.Root
    ref={ref}
    className={cn(
      "peer h-4 w-4 shrink-0 rounded-sm border border-primary ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground",
      className
    )}
    {...props}
  >
    <CheckboxPrimitive.Indicator className={cn("flex items-center justify-center text-current")}>
      <Check className="h-4 w-4" />
    </CheckboxPrimitive.Indicator>
  </CheckboxPrimitive.Root>
));
Checkbox.displayName = CheckboxPrimitive.Root.displayName;

export { Checkbox };
```

---

## `components/ui/avatar.tsx`

```tsx
"use client";

import * as React from "react";
import * as AvatarPrimitive from "@radix-ui/react-avatar";
import { cn } from "@/lib/utils";

const Avatar = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Root> & {
    containerClassName?: string;
  }
>(({ className, containerClassName, ...props }, ref) => (
  <AvatarPrimitive.Root
    ref={ref}
    className={cn("relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full", containerClassName)}
    {...props}
  />
));
Avatar.displayName = AvatarPrimitive.Root.displayName;

const AvatarImage = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Image>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Image>
>(({ className, ...props }, ref) => (
  <AvatarPrimitive.Image ref={ref} className={cn("aspect-square h-full w-full", className)} {...props} />
));
AvatarImage.displayName = AvatarPrimitive.Image.displayName;

const AvatarFallback = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Fallback>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Fallback>
>(({ className, ...props }, ref) => (
  <AvatarPrimitive.Fallback
    ref={ref}
    className={cn("flex h-full w-full items-center justify-center rounded-full bg-muted text-sm font-medium", className)}
    {...props}
  />
));
AvatarFallback.displayName = AvatarPrimitive.Fallback.displayName;

export { Avatar, AvatarImage, AvatarFallback };
```

---

## `components/ui/rating.tsx`

```tsx
"use client";

import * as React from "react";
import { Star } from "lucide-react";
import { cn } from "@/lib/utils";

interface RatingProps extends React.HTMLAttributes<HTMLDivElement> {
  value: number;
  max?: number;
}

export function Rating({ value, max = 5, className, ...props }: RatingProps) {
  return (
    <div className={cn("flex items-center gap-1", className)} {...props}>
      {Array.from({ length: max }).map((_, i) => {
        const filled = i < Math.floor(value);
        const partial = !filled && i < value;
        return (
          <div key={i} className="relative">
            {partial && (
              <div className="absolute inset-0 overflow-hidden w-[50%]">
                <Star className="h-4 w-4 fill-[#E0702B] text-[#E0702B]" />
              </div>
            )}
            <Star className={cn("h-4 w-4", filled ? "fill-[#E0702B] text-[#E0702B]" : "text-gray-300")} />
          </div>
        );
      })}
    </div>
  );
}
```

---

## `components/ui/verified-badge.tsx`

```tsx
"use client";

import { CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface VerifiedBadgeProps {
  status?: string | number;
  size?: "sm" | "md" | "lg";
  className?: string;
}

export function VerifiedBadge({ status, size = "md", className }: VerifiedBadgeProps) {
  const isVerified = status === 2 || status === "approved" || status === "APPROVED";

  if (!isVerified) return null;

  const sizeClasses = {
    sm: "h-4 w-4",
    md: "h-5 w-5",
    lg: "h-6 w-6",
  };

  return (
    <span className={cn("text-green-500 inline-flex", className)} title="Verificado">
      <CheckCircle2 className={sizeClasses[size]} />
    </span>
  );
}
```

---

## `components/ui/label.tsx`

```tsx
"use client";

import * as React from "react";
import * as LabelPrimitive from "@radix-ui/react-label";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const labelVariants = cva(
  "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
);

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

## `components/ui/form.tsx`

```tsx
"use client";

import * as React from "react";
import type * as LabelPrimitive from "@radix-ui/react-label";
import type { Slot } from "@radix-ui/react-slot";
import {
  Controller,
  ControllerProps,
  FieldPath,
  FieldValues,
  FormProvider,
  useFormContext,
} from "react-hook-form";
import { cn } from "@/lib/utils";
import { Label } from "@/components/ui/label";

const Form = FormProvider;

type FormFieldContextValue<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>
> = {
  name: TName;
};

const FormFieldContext = React.createContext<FormFieldContextValue>({} as FormFieldContextValue);

const FormField = <
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>
>({
  ...props
}: ControllerProps<TFieldValues, TName>) => {
  return (
    <FormFieldContext.Provider value={{ name: props.name }}>
      <Controller {...props} />
    </FormFieldContext.Provider>
  );
};

const useFormField = () => {
  const fieldContext = React.useContext(FormFieldContext);
  const { getFieldState, formState } = useFormContext(fieldContext.name);
  return { ...fieldContext, ...getFieldState(fieldContext.name, formState) };
};

const FormItem = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => {
    return <div ref={ref} className={cn("space-y-2", className)} {...props} />;
  }
);
FormItem.displayName = "FormItem";

const FormLabel = React.forwardRef<
  React.ElementRef<typeof LabelPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof LabelPrimitive.Root>
>(({ className, ...props }, ref) => {
  const { error, formItemId } = useFormField();
  return (
    <Label
      ref={ref}
      className={cn(error && "text-destructive", className)}
      htmlFor={formItemId}
      {...props}
    />
  );
});
FormLabel.displayName = "FormLabel";

const FormControl = React.forwardRef<
  React.ElementRef<typeof Slot>,
  React.ComponentPropsWithoutRef<typeof Slot>
>(({ ...props }, ref) => {
  const { error, formItemId, formDescriptionId, formMessageId } = useFormField();
  return (
    <Slot
      ref={ref}
      id={formItemId}
      aria-describedby={!error ? `${formDescriptionId}` : `${formDescriptionId} ${formMessageId}`}
      aria-invalid={!!error}
      {...props}
    />
  );
});
FormControl.displayName = "FormControl";

const FormMessage = React.forwardRef<
  HTMLParagraphElement,
  React.HTMLAttributes<HTMLParagraphElement>
>(({ className, children, ...props }, ref) => {
  const { error, formMessageId } = useFormField();
  const body = error ? String(error?.message) : children;
  if (!body) return null;
  return (
    <p ref={ref} id={formMessageId} className={cn("text-sm font-medium text-destructive", className)} {...props}>
      {body}
    </p>
  );
});
FormMessage.displayName = "FormMessage";

export { useFormField, Form, FormItem, FormLabel, FormControl, FormMessage, FormField };
```

---

## `components/ui/ad-banner.tsx`

```tsx
"use client";

import { X } from "lucide-react";
import { useState } from "react";

export function AdBanner() {
  const [isVisible, setIsVisible] = useState(true);

  if (!isVisible) return null;

  return (
    <div className="bg-[#E0702B] text-white py-2 px-4 relative">
      <p className="text-center text-sm font-medium">
        Anúncio: space reserved for future ads
      </p>
      <button
        onClick={() => setIsVisible(false)}
        className="absolute right-4 top-1/2 -translate-y-1/2 p-1 hover:bg-white/20 rounded"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  );
}
```

---

## `components/ui/dropdown-menu.tsx`

```tsx
"use client";

import * as React from "react";
import * as DropdownMenuPrimitive from "@radix-ui/react-dropdown-menu";
import { Check, ChevronRight, Circle } from "lucide-react";
import { cn } from "@/lib/utils";

const DropdownMenu = DropdownMenuPrimitive.Root;
const DropdownMenuTrigger = DropdownMenuPrimitive.Trigger;
const DropdownMenuGroup = DropdownMenuPrimitive.Group;
const DropdownMenuPortal = DropdownMenuPrimitive.Portal;
const DropdownMenuSub = DropdownMenuPrimitive.Sub;
const DropdownMenuRadioGroup = DropdownMenuPrimitive.RadioGroup;

const DropdownMenuContent = React.forwardRef<
  React.ElementRef<typeof DropdownMenuPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof DropdownMenuPrimitive.Content>
>(({ className, sideOffset = 4, ...props }, ref) => (
  <DropdownMenuPrimitive.Portal>
    <DropdownMenuPrimitive.Content
      ref={ref}
      sideOffset={sideOffset}
      className={cn(
        "z-50 min-w-[8rem] overflow-hidden rounded-md border bg-popover p-1 text-popover-foreground shadow-md",
        "data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95",
        className
      )}
      {...props}
    />
  </DropdownMenuPrimitive.Portal>
));
DropdownMenuContent.displayName = DropdownMenuPrimitive.Content.displayName;

const DropdownMenuItem = React.forwardRef<
  React.ElementRef<typeof DropdownMenuPrimitive.Item>,
  React.ComponentPropsWithoutRef<typeof DropdownMenuPrimitive.Item> & { inset?: boolean }
>(({ className, inset, ...props }, ref) => (
  <DropdownMenuPrimitive.Item
    ref={ref}
    className={cn(
      "relative flex cursor-default select-none items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none transition-colors focus:bg-accent focus:text-accent-foreground data-[disabled]:pointer-events-none data-[disabled]:opacity-50",
      inset && "pl-8",
      className
    )}
    {...props}
  />
));
DropdownMenuItem.displayName = DropdownMenuPrimitive.Item.displayName;

export { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem };
```

---

# 4. COMPONENTS - LAYOUT

## `components/layout/header.tsx`

```tsx
"use client";

import Link from "next/link";
import { useSession, signOut } from "next-auth/react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { UserMenu } from "@/components/layout/user-menu";
import { Menu, X } from "lucide-react";
import { useState } from "react";

export function Header() {
  const { data: session } = useSession();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        {/* Logo */}
        <Link href="/" className="flex items-center gap-2">
          <span className="text-xl font-bold text-primary">MeAjudaAí</span>
        </Link>

        {/* Desktop Navigation */}
        <nav className="hidden md:flex items-center gap-6">
          <Link href="/buscar" className="text-sm font-medium hover:text-primary">
            Buscar
          </Link>
          {session ? (
            <>
              <Link href="/prestador" className="text-sm font-medium hover:text-primary">
                Meu Painel
              </Link>
              <Link href="/perfil" className="text-sm font-medium hover:text-primary">
                Perfil
              </Link>
              <UserMenu />
            </>
          ) : (
            <>
              <Link href="/auth/signin">
                <Button variant="ghost">Entrar</Button>
              </Link>
              <Link href="/auth/cadastro/cliente">
                <Button className="bg-secondary hover:bg-secondary-hover">Cadastrar</Button>
              </Link>
            </>
          )}
        </nav>

        {/* Mobile Menu Button */}
        <button
          className="md:hidden"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
        >
          {mobileMenuOpen ? <X /> : <Menu />}
        </button>
      </div>

      {/* Mobile Menu */}
      {mobileMenuOpen && (
        <div className="md:hidden border-t">
          <nav className="container mx-auto px-4 py-4 space-y-2">
            <Link href="/buscar" className="block py-2 text-sm font-medium">
              Buscar
            </Link>
            {session ? (
              <>
                <Link href="/prestador" className="block py-2 text-sm font-medium">
                  Meu Painel
                </Link>
                <Link href="/perfil" className="block py-2 text-sm font-medium">
                  Perfil
                </Link>
                <button onClick={() => signOut()} className="block py-2 text-sm font-medium text-destructive">
                  Sair
                </button>
              </>
            ) : (
              <>
                <Link href="/auth/signin" className="block py-2 text-sm font-medium">
                  Entrar
                </Link>
                <Link href="/auth/cadastro/cliente" className="block py-2 text-sm font-medium">
                  Cadastrar
                </Link>
              </>
            )}
          </nav>
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
    <footer className="border-t bg-background">
      <div className="container mx-auto px-4 py-8">
        <div className="grid gap-8 md:grid-cols-4">
          <div>
            <h3 className="font-bold text-lg mb-4">MeAjudaAí</h3>
            <p className="text-sm text-muted-foreground">
              Conectando quem precisa com quem sabe fazer.
            </p>
          </div>

          <div>
            <h4 className="font-semibold mb-4">Links</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="/buscar">Buscar Prestadores</Link></li>
              <li><Link href="/auth/signin">Entrar</Link></li>
              <li><Link href="/auth/cadastro/cliente">Cadastrar</Link></li>
            </ul>
          </div>

          <div>
            <h4 className="font-semibold mb-4">Prestadores</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="/cadastro/prestador">Cadastre-se</Link></li>
              <li><Link href="/prestador">Painel</Link></li>
            </ul>
          </div>

          <div>
            <h4 className="font-semibold mb-4">Legal</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="/termos-de-uso">Termos de Uso</Link></li>
              <li><Link href="/politica-de-privacidade">Política de Privacidade</Link></li>
            </ul>
          </div>
        </div>

        <div className="mt-8 pt-8 border-t text-center text-sm text-muted-foreground">
          <p>&copy; {new Date().getFullYear()} MeAjudaAí. Todos os direitos reservados.</p>
        </div>
      </div>
    </footer>
  );
}
```

---

## `components/layout/user-menu.tsx`

```tsx
"use client";

import { useSession, signOut } from "next-auth/react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import Link from "next/link";

export function UserMenu() {
  const { data: session } = useSession();

  if (!session?.user) return null;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button className="flex items-center gap-2">
          <Avatar className="h-8 w-8">
            <AvatarFallback>
              {session.user.name?.charAt(0).toUpperCase() ?? "U"}
            </AvatarFallback>
          </Avatar>
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem asChild>
          <Link href="/perfil">Meu Perfil</Link>
        </DropdownMenuItem>
        <DropdownMenuItem asChild>
          <Link href="/prestador">Painel</Link>
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => signOut()}>
          Sair
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

---

# 5. COMPONENTS - FEATURES

## `components/home/how-it-works.tsx`

```tsx
import Image from "next/image";

export function HowItWorks() {
  const steps = [
    {
      title: "Busque",
      description: "Encontre prestadores de serviços na sua região",
      image: "/assets/illustrations/how-it-works-1.png",
    },
    {
      title: "Compare",
      description: "Veja avaliações e escolha o melhor",
      image: "/assets/illustrations/how-it-works-2.png",
    },
    {
      title: "Contate",
      description: "Converse diretamente com o prestador",
      image: "/assets/illustrations/how-it-works-3.png",
    },
    {
      title: "Avalie",
      description: "Compartilhe sua experiência",
      image: "/assets/illustrations/how-it-works-4.png",
    },
  ];

  return (
    <div className="space-y-8">
      <div className="text-center">
        <h2 className="text-3xl font-bold text-foreground mb-4">Como Funciona</h2>
        <p className="text-muted-foreground">Em quatro passos simples</p>
      </div>

      <div className="grid gap-8 md:grid-cols-4">
        {steps.map((step, index) => (
          <div key={index} className="flex flex-col items-center text-center">
            <div className="relative h-32 w-32 mb-4">
              <Image
                src={step.image}
                alt={step.title}
                fill
                className="object-contain"
              />
            </div>
            <div className="text-4xl font-bold text-secondary mb-2">0{index + 1}</div>
            <h3 className="font-semibold mb-2">{step.title}</h3>
            <p className="text-sm text-muted-foreground">{step.description}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

## `components/search/city-search.tsx`

```tsx
"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Search, MapPin } from "lucide-react";

export function CitySearch() {
  const [city, setCity] = useState("");
  const router = useRouter();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (city.trim()) {
      router.push(`/buscar?city=${encodeURIComponent(city)}`);
    } else {
      router.push("/buscar");
    }
  };

  return (
    <form onSubmit={handleSearch} className="relative">
      <div className="relative">
        <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
        <input
          type="text"
          value={city}
          onChange={(e) => setCity(e.target.value)}
          placeholder="Qual cidade você precisa?"
          className="w-full pl-12 pr-32 py-4 border-2 border-secondary/50 rounded-full text-lg focus:outline-none focus:border-secondary shadow-lg"
        />
        <button
          type="submit"
          className="absolute right-2 top-1/2 -translate-y-1/2 bg-secondary hover:bg-secondary-hover text-white px-6 py-2 rounded-full font-medium transition-colors flex items-center gap-2"
        >
          <Search className="h-4 w-4" />
          Buscar
        </button>
      </div>
    </form>
  );
}
```

---

## `components/search/search-filters.tsx`

```tsx
"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";

export function SearchFilters() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [radius, setRadius] = useState(
    parseInt(searchParams.get("radiusInKm") || "50")
  );
  const [minRating, setMinRating] = useState(
    parseFloat(searchParams.get("minRating") || "0")
  );

  const applyFilters = () => {
    const params = new URLSearchParams(searchParams.toString());
    params.set("radiusInKm", radius.toString());
    if (minRating > 0) {
      params.set("minRating", minRating.toString());
    } else {
      params.delete("minRating");
    }
    router.push(`/buscar?${params.toString()}`);
  };

  return (
    <div className="bg-white rounded-xl border p-4 space-y-6">
      <h3 className="font-semibold">Filtros</h3>

      <div className="space-y-2">
        <Label>Raio de busca: {radius} km</Label>
        <Slider
          value={[radius]}
          onValueChange={([v]) => setRadius(v)}
          min={5}
          max={100}
          step={5}
        />
      </div>

      <div className="space-y-2">
        <Label>Avaliação mínima: {minRating} estrelas</Label>
        <Slider
          value={[minRating]}
          onValueChange={([v]) => setMinRating(v)}
          min={0}
          max={5}
          step={0.5}
        />
      </div>

      <Button onClick={applyFilters} className="w-full bg-secondary hover:bg-secondary-hover">
        Aplicar Filtros
      </Button>
    </div>
  );
}
```

---

## `components/search/service-tags.tsx`

```tsx
"use client";

import { useQuery } from "@tanstack/react-query";
import { apiCategoriesGet } from "@/lib/api/generated";
import { Badge } from "@/components/ui/badge";
import Link from "next/link";
import { useSearchParams } from "next/navigation";

export function ServiceTags() {
  const searchParams = useSearchParams();
  const selectedCategory = searchParams.get("categoryId");

  const { data } = useQuery({
    queryKey: ["categories"],
    queryFn: () => apiCategoriesGet(),
  });

  const categories = data?.data?.data ?? [];

  return (
    <div className="flex flex-wrap gap-2">
      <Link href="/buscar">
        <Badge
          variant={!selectedCategory ? "secondary" : "outline"}
          className="px-4 py-2 cursor-pointer"
        >
          Todos
        </Badge>
      </Link>
      {categories.map((category) => (
        <Link key={category.id} href={`/buscar?categoryId=${category.id}`}>
          <Badge
            variant={selectedCategory === category.id ? "secondary" : "outline"}
            className="px-4 py-2 cursor-pointer"
          >
            {category.name}
          </Badge>
        </Link>
      ))}
    </div>
  );
}
```

---

## `components/search/ad-card.tsx`

```tsx
export function AdCard() {
  return (
    <div className="bg-gray-100 border-2 border-dashed border-gray-300 rounded-xl h-64 flex items-center justify-center">
      <div className="text-center text-gray-400">
        <p className="text-sm">Espaço reservado para anúncios</p>
      </div>
    </div>
  );
}
```

---

## `components/service/service-card.tsx`

```tsx
import Link from "next/link";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { Badge } from "@/components/ui/badge";
import { MapPin } from "lucide-react";

interface ServiceCardProps {
  id: string;
  name: string;
  avatarUrl?: string;
  description: string;
  services: string[];
  rating: number;
  reviewCount: number;
}

export function ServiceCard({
  id,
  name,
  avatarUrl,
  description,
  services,
  rating,
  reviewCount,
}: ServiceCardProps) {
  return (
    <Link href={`/prestador/${id}`}>
      <div className="bg-white rounded-xl border hover:shadow-lg transition-shadow p-4">
        <div className="flex items-start gap-4">
          <Avatar className="h-16 w-16">
            <AvatarFallback className="bg-secondary/10 text-secondary text-xl font-bold">
              {name.substring(0, 2).toUpperCase()}
            </AvatarFallback>
          </Avatar>

          <div className="flex-1 min-w-0">
            <h3 className="font-semibold text-lg truncate">{name}</h3>
            <div className="flex items-center gap-2 mt-1">
              <Rating value={rating} />
              <span className="text-sm text-muted-foreground">
                ({reviewCount})
              </span>
            </div>
          </div>
        </div>

        <p className="mt-4 text-sm text-muted-foreground line-clamp-2">
          {description}
        </p>

        <div className="mt-4 flex flex-wrap gap-1">
          {services.slice(0, 3).map((service, i) => (
            <Badge key={i} variant="secondary" className="text-xs">
              {service}
            </Badge>
          ))}
          {services.length > 3 && (
            <Badge variant="outline" className="text-xs">
              +{services.length - 3}
            </Badge>
          )}
        </div>
      </div>
    </Link>
  );
}
```

---

## `components/providers/provider-card.tsx`

```tsx
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { Badge } from "@/components/ui/badge";
import Link from "next/link";

interface ProviderCardProps {
  provider: {
    id: string;
    name: string;
    avatarUrl?: string;
    services?: string[];
    rating?: number;
    reviewCount?: number;
    city?: string;
  };
}

export function ProviderCard({ provider }: ProviderCardProps) {
  return (
    <Link href={`/prestador/${provider.id}`}>
      <div className="bg-white rounded-xl border p-4 hover:shadow-md transition-shadow">
        <div className="flex items-center gap-3">
          <Avatar className="h-12 w-12">
            <AvatarFallback className="bg-primary/10 text-primary font-bold">
              {provider.name.substring(0, 2).toUpperCase()}
            </AvatarFallback>
          </Avatar>
          <div>
            <h4 className="font-medium">{provider.name}</h4>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Rating value={provider.rating ?? 0} />
              <span>({provider.reviewCount ?? 0})</span>
            </div>
          </div>
        </div>
      </div>
    </Link>
  );
}
```

---

## `components/providers/dashboard-client.tsx`

```tsx
"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { EVerificationStatus } from "@/types/api/provider";

export default function DashboardClient({ provider }) {
  const statusLabels = {
    [EVerificationStatus.Pending]: "Pendente",
    [EVerificationStatus.UnderReview]: "Em Análise",
    [EVerificationStatus.Approved]: "Aprovado",
    [EVerificationStatus.Rejected]: "Rejeitado",
    [EVerificationStatus.Suspended]: "Suspenso",
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Painel do Prestador</h1>

      <div className="grid gap-6 md:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Status da Conta</CardTitle>
          </CardHeader>
          <CardContent>
            <Badge variant={provider.verificationStatus === 2 ? "default" : "secondary"}>
              {statusLabels[provider.verificationStatus]}
            </Badge>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Meus Serviços</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{provider.services?.length ?? 0}</p>
            <p className="text-muted-foreground">serviços cadastrados</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Avaliação</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{provider.averageRating ?? 0}</p>
            <p className="text-muted-foreground">
              {provider.reviewCount ?? 0} avaliações
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
```

---

## `components/providers/service-selector.tsx`

```tsx
"use client";

import { useQuery } from "@tanstack/react-query";
import { apiCategoriesGet } from "@/lib/api/generated";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";

interface ServiceSelectorProps {
  selectedServices: string[];
  onChange: (services: string[]) => void;
}

export function ServiceSelector({ selectedServices, onChange }: ServiceSelectorProps) {
  const { data } = useQuery({
    queryKey: ["categories", "services"],
    queryFn: () => apiCategoriesGet(),
  });

  const categories = data?.data?.data ?? [];

  const toggleService = (serviceId: string) => {
    if (selectedServices.includes(serviceId)) {
      onChange(selectedServices.filter((id) => id !== serviceId));
    } else {
      onChange([...selectedServices, serviceId]);
    }
  };

  return (
    <div className="space-y-4">
      {categories.map((category) => (
        <div key={category.id} className="space-y-2">
          <h4 className="font-medium text-sm">{category.name}</h4>
          {category.services?.map((service) => (
            <div key={service.id} className="flex items-center gap-2">
              <Checkbox
                id={service.id}
                checked={selectedServices.includes(service.id)}
                onCheckedChange={() => toggleService(service.id)}
              />
              <Label htmlFor={service.id} className="text-sm">
                {service.name}
              </Label>
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}
```

---

## `components/providers/provider-grid.tsx`

```tsx
import { ServiceCard } from "@/components/service/service-card";

interface ProviderGridProps {
  providers: Array<{
    id: string;
    name: string;
    avatarUrl?: string;
    description?: string;
    services: string[];
    rating: number;
    reviewCount: number;
  }>;
}

export function ProviderGrid({ providers }: ProviderGridProps) {
  return (
    <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {providers.map((provider) => (
        <ServiceCard
          key={provider.id}
          id={provider.id}
          name={provider.name}
          avatarUrl={provider.avatarUrl}
          description={provider.description || ""}
          services={provider.services}
          rating={provider.rating}
          reviewCount={provider.reviewCount}
        />
      ))}
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

## `components/profile/edit-profile-form.tsx`

```tsx
"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { toast } from "sonner";

interface EditProfileFormProps {
  userId: string;
  initialData: {
    firstName: string;
    lastName: string;
    email: string;
  };
}

export function EditProfileForm({ userId, initialData }: EditProfileFormProps) {
  const form = useForm({
    defaultValues: initialData,
  });

  const onSubmit = async (data) => {
    try {
      // API call to update profile
      toast.success("Perfil atualizado com sucesso!");
    } catch (error) {
      toast.error("Erro ao atualizar perfil");
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="firstName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Nome</FormLabel>
              <FormControl>
                <Input {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="lastName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Sobrenome</FormLabel>
              <FormControl>
                <Input {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full">
          Salvar Alterações
        </Button>
      </form>
    </Form>
  );
}
```

---

## `components/auth/login-form.tsx`

```tsx
"use client";

import { useState } from "react";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";

export function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const router = useRouter();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      const result = await signIn("credentials", {
        email,
        password,
        redirect: false,
      });

      if (result?.error) {
        toast.error("Email ou senha incorretos");
      } else {
        router.push("/");
        router.refresh();
      }
    } catch (error) {
      toast.error("Erro ao fazer login");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <Input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
      </div>
      <div>
        <Input
          type="password"
          placeholder="Senha"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>
      <Button type="submit" className="w-full" disabled={isLoading}>
        {isLoading ? "Entrando..." : "Entrar"}
      </Button>
    </form>
  );
}
```

---

## `components/auth/customer-register-form.tsx`

```tsx
"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Checkbox } from "@/components/ui/checkbox";
import Link from "next/link";
import { toast } from "sonner";
import { useRouter } from "next/navigation";

export function CustomerRegisterForm() {
  const router = useRouter();

  const form = useForm({
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      acceptedTerms: false,
    },
  });

  const onSubmit = async (data) => {
    try {
      // API call to register
      toast.success("Conta criada com sucesso!");
      router.push("/auth/signin");
    } catch (error) {
      toast.error("Erro ao criar conta");
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            control={form.control}
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Nome</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Sobrenome</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input type="email" {...field} />
              </FormControl>
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Senha</FormLabel>
              <FormControl>
                <Input type="password" {...field} />
              </FormControl>
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="acceptedTerms"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start space-x-3 space-y-0">
              <FormControl>
                <Checkbox checked={field.value} onCheckedChange={field.onChange} />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel className="text-sm">
                  Aceito os <Link href="/termos" className="underline">Termos de Uso</Link>
                </FormLabel>
              </div>
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full bg-secondary hover:bg-secondary-hover">
          Criar Conta
        </Button>
      </form>
    </Form>
  );
}
```

---

## `components/auth/auth-selection-dropdown.tsx`

```tsx
"use client";

import { useState } from "react";
import Link from "next/link";
import { ChevronDown } from "lucide-react";

export function AuthSelectionDropdown() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-center justify-between p-4 border rounded-lg bg-white"
      >
        <span>Como você quer se cadastrar?</span>
        <ChevronDown className="h-4 w-4" />
      </button>

      {isOpen && (
        <div className="absolute top-full left-0 right-0 mt-2 border rounded-lg bg-white shadow-lg z-10">
          <Link
            href="/auth/cadastro/cliente"
            className="block p-4 hover:bg-gray-50"
          >
            <div className="font-medium">Sou Cliente</div>
            <div className="text-sm text-muted-foreground">
              Quero contratar serviços
            </div>
          </Link>
          <Link
            href="/cadastro/prestador"
            className="block p-4 hover:bg-gray-50 border-t"
          >
            <div className="font-medium">Sou Prestador</div>
            <div className="text-sm text-muted-foreground">
              Quero oferecer meus serviços
            </div>
          </Link>
        </div>
      )}
    </div>
  );
}
```

---

## `components/reviews/review-list.tsx`

```tsx
"use client";

import { useQuery } from "@tanstack/react-query";
import { ReviewCard } from "@/components/reviews/review-card";

interface ReviewListProps {
  providerId: string;
}

export function ReviewList({ providerId }: ReviewListProps) {
  const { data, isLoading } = useQuery({
    queryKey: ["reviews", providerId],
    queryFn: async () => {
      // API call to fetch reviews
      return [];
    },
  });

  if (isLoading) {
    return <div className="animate-pulse space-y-4">
      <div className="h-24 bg-gray-100 rounded"></div>
      <div className="h-24 bg-gray-100 rounded"></div>
    </div>;
  }

  const reviews = data ?? [];

  if (reviews.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        Nenhum comentário ainda. Seja o primeiro!
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {reviews.map((review) => (
        <ReviewCard key={review.id} review={review} />
      ))}
    </div>
  );
}
```

---

## `components/reviews/review-card.tsx`

```tsx
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";

interface ReviewCardProps {
  review: {
    id: string;
    authorName: string;
    rating: number;
    comment: string;
    createdAt: string;
  };
}

export function ReviewCard({ review }: ReviewCardProps) {
  return (
    <div className="bg-white rounded-lg border p-4">
      <div className="flex items-start gap-4">
        <Avatar className="h-10 w-10">
          <AvatarFallback>
            {review.authorName.substring(0, 2).toUpperCase()}
          </AvatarFallback>
        </Avatar>
        <div className="flex-1">
          <div className="flex items-center justify-between">
            <span className="font-medium">{review.authorName}</span>
            <Rating value={review.rating} />
          </div>
          <p className="mt-2 text-sm text-muted-foreground">{review.comment}</p>
          <p className="mt-2 text-xs text-muted-foreground">
            {new Date(review.createdAt).toLocaleDateString("pt-BR")}
          </p>
        </div>
      </div>
    </div>
  );
}
```

---

## `components/reviews/review-form.tsx`

```tsx
"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Rating } from "@/components/ui/rating";
import { toast } from "sonner";
import { useSession } from "next-auth/react";
import Link from "next/link";

interface ReviewFormProps {
  providerId: string;
}

export function ReviewForm({ providerId }: ReviewFormProps) {
  const { data: session } = useSession();
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState("");
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (data: { rating: number; comment: string }) => {
      // API call to submit review
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews", providerId] });
      setRating(0);
      setComment("");
      toast.success("Avaliação enviada com sucesso!");
    },
    onError: () => {
      toast.error("Erro ao enviar avaliação");
    },
  });

  if (!session) {
    return (
      <div className="text-center py-4">
        <p className="text-muted-foreground mb-2">
          Você precisa estar logado para avaliar
        </p>
        <Link href={`/auth/signin?callbackUrl=/prestador/${providerId}`}>
          <Button variant="outline">Fazer Login</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div>
        <label className="text-sm font-medium">Sua avaliação</label>
        <div className="mt-2">
          <Rating value={rating} onValueChange={setRating} />
        </div>
      </div>
      <div>
        <Textarea
          placeholder="Deixe seu comentário..."
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          rows={3}
        />
      </div>
      <Button
        onClick={() => mutation.mutate({ rating, comment })}
        disabled={rating === 0 || mutation.isPending}
        className="w-full bg-secondary hover:bg-secondary-hover"
      >
        {mutation.isPending ? "Enviando..." : "Enviar Avaliação"}
      </Button>
    </div>
  );
}
```

---

# 6. IMAGENS E ASSETS

## Lista de Imagens (nomes dos arquivos)

### Raiz `public/`
- `logo.png` - Logo principal do site
- `icon.png` - Ícone do site
- `favicon.ico` - Favicon
- `illustration-woman.png` - Ilustração mulher (Hero - Conheça)
- `illustration-man.png` - Ilustração homem (CTA Prestadores)

### `public/assets/illustrations/`
- `how-it-works-1.png` - Passo 1: Busque
- `how-it-works-2.png` - Passo 2: Compare
- `how-it-works-3.png` - Passo 3: Contate
- `how-it-works-4.png` - Passo 4: Avalie

### `public/images/providers/`
- `provider-1.svg` - Avatar provider 1
- `provider-2.svg` - Avatar provider 2
- `provider-3.svg` - Avatar provider 3
- `provider-4.svg` - Avatar provider 4
- `provider-5.svg` - Avatar provider 5
- `provider-6.svg` - Avatar provider 6
- `provider-7.svg` - Avatar provider 7
- `provider-8.svg` - Avatar provider 8
- `provider-9.svg` - Avatar provider 9
- `provider-10.svg` - Avatar provider 10

---

# 7. SISTEMA DE DESIGN

## Paleta de Cores

| Nome | Hex | Uso |
|------|-----|-----|
| Primary | `#395873` | Botões primários, backgrounds de destaque |
| Primary Hover | `#2E4760` | Hover de elementos primários |
| Secondary | `#D96704` | Cor secundária, botões CTA |
| Secondary Light | `#F2AE72` | Versão clara do secondary |
| Secondary Hover | `#B85703` | Hover de elementos secondary |
| Brand | `#E0702B` | Brand principal (similar ao secondary) |
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
| `--muted-foreground` | `#666666` | Texto em backgrounds muted |
| `--accent` | `#f5f5f5` | Backgrounds de destaque sutil |
| `--ring` | `#D96704` | Focus rings |

## Cores de Tema (Dark Mode)

| Variável CSS | Hex | Uso |
|--------------|-----|-----|
| `--background` | `#0a0a0a` | Background principal |
| `--foreground` | `#ededed` | Cor do texto principal |
| `--surface` | `#1a1a1a` | Cards, elementos elevados |
| `--surface-raised` | `#262626` | Elementos levemente elevados |
| `--foreground-subtle` | `#a3a3a3` | Texto secundário |
| `--border` | `#404040` | Bordas |
| `--input` | `#404040` | Campos de input |
| `--card` | `#1a1a1a` | Cards |
| `--muted` | `#262626` | Backgrounds sutis |
| `--muted-foreground` | `#a3a3a3` | Texto em backgrounds muted |
| `--accent` | `#262626` | Backgrounds de destaque sutil |

## Tipografia

| Propriedade | Valor |
|--------------|-------|
| Font Family | `'Roboto', Arial, Helvetica, sans-serif` |
| H1 | `text-3xl font-bold` |
| H2 | `text-2xl font-bold` |
| H3 | `text-xl font-semibold` |
| Body | `text-sm` |
| Small | `text-xs` |

## Breakpoints

| Breakpoint | Valor |
|------------|-------|
| Mobile | `< 640px` (default) |
| Tablet | `md: 768px` |
| Desktop | `lg: 1024px` |
| Large Desktop | `xl: 1280px` |

## Espaçamentos

| Classe | Valor |
|--------|-------|
| `p-4` | `16px` |
| `py-8` | `32px vertical` |
| `gap-4` | `16px` |
| `gap-6` | `24px` |
| `mb-8` | `32px margin bottom` |

## Border Radius

| Classe | Valor |
|--------|-------|
| `rounded` | 4px |
| `rounded-md` | 6px |
| `rounded-lg` | 8px |
| `rounded-xl` | 12px |
| `rounded-full` | 9999px (círculos, pills) |

## Sombras

| Classe | Uso |
|--------|-----|
| `shadow-sm` | Cards, elementos pequenos |
| `shadow-md` | Modais, elementos médios |
| `shadow-lg` | Dropdowns, elementos grandes |

## Componentes Reutilizáveis

### Badges
- `default` - Background primary
- `secondary` - Background secondary
- `destructive` - Background vermelho
- `outline` - Apenas borda

### Botões
- `default` - Primary background
- `primary` - Primary background (sinônimo)
- `secondary` - Secondary background (brand laranja)
- `destructive` - Vermelho para ações destrutivas
- `outline` - Apenas borda
- `ghost` - Sem background, hover sutil
- `link` - Estilo de link

### Cards
- Background branco (light) / escuro (dark)
- Borda sutil
- Border radius `rounded-lg`
- Sombra `shadow-sm`

---

## Dependências de UI Principais

| Biblioteca | Versão | Uso |
|------------|--------|-----|
| `@radix-ui/react-slot` | - | Componente Slot para asChild |
| `@radix-ui/react-checkbox` | - | Checkbox acessível |
| `@radix-ui/react-avatar` | - | Avatar component |
| `@radix-ui/react-dropdown-menu` | - | Dropdown menus |
| `@radix-ui/react-label` | - | Labels acessíveis |
| `lucide-react` | - | Ícones |
| `tailwindcss` | v4 | CSS Framework |
| `class-variance-authority` | - | Variantes de componentes |
| `react-hook-form` | - | Formulários |
| `@hookform/resolvers/zod` | - | Validação com Zod |
| `@tanstack/react-query` | - | Data fetching |

---

*Documento gerado em: 2026-03-21*
*Projeto: MeAjudaAi.Web.Customer*
