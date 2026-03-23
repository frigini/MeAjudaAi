# Admin Portal (React/Next.js) Frontend Documentation

## Overview
Admin Portal built with React 19 + Next.js 15, migrating from Blazor WASM. Provides administrative interface for managing providers, categories, services, allowed cities, and documents.

---

## Route: `/login`

### Page Code
```tsx
// src/app/login/page.tsx
"use client";

import { useSearchParams } from "next/navigation";
import { signIn } from "next-auth/react";
import { useState } from "react";
import { Shield, AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function LoginPage() {
  const searchParams = useSearchParams();
  const error = searchParams.get("error");
  const [isLoading, setIsLoading] = useState(false);

  const handleSignIn = async () => {
    setIsLoading(true);
    await signIn("keycloak", { callbackUrl: "/dashboard" });
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background to-muted">
      <div className="w-full max-w-md space-y-8 rounded-xl border border-border bg-surface p-8 shadow-lg">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-primary">
            <Shield className="h-8 w-8 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold text-foreground">MeAjudaAí</h1>
          <p className="mt-1 text-sm text-muted-foreground">Portal do Administrador</p>
        </div>

        {error && (
          <div className="flex items-center gap-2 rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
            <AlertCircle className="h-4 w-4" />
            {error === "OAuthSignin" && "Erro ao iniciar autenticação. Tente novamente."}
            {error === "OAuthCallback" && "Erro no processo de autenticação."}
            {error === "OAuthAccountNotLinked" && "Conta não vinculada."}
            {error === "CredentialsSignin" && "Credenciais inválidas."}
            {!["OAuthSignin", "OAuthCallback", "OAuthAccountNotLinked", "CredentialsSignin"].includes(error) &&
              "Erro de autenticação. Tente novamente."}
          </div>
        )}

        <div className="space-y-4">
          <Button onClick={handleSignIn} className="w-full" size="lg" disabled={isLoading}>
            {isLoading ? "Redirecionando..." : "Entrar com Keycloak"}
          </Button>
        </div>

        <p className="text-center text-xs text-muted-foreground">
          Este é um portal restrito. Acesso apenas para administradores autorizados.
        </p>
      </div>
    </div>
  );
}
```

### Design Details
- **Layout**: Centered card on gradient background
- **Gradient**: `from-background to-muted`
- **Card**: `border-border bg-surface rounded-xl shadow-lg p-8 max-w-md`
- **Logo**: 64x64px circle with primary background
- **Button**: Primary variant, full width, large size

---

## Route: `/dashboard`

### Page Code
```tsx
// src/app/(admin)/dashboard/page.tsx
"use client";

import { Users, Clock, CheckCircle, AlertCircle, TrendingUp, Loader2 } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from "recharts";
import { useDashboardStats } from "@/hooks/admin";

const verificationColors = {
  approved: "#22c55e",
  pending: "#f59e0b",
  underReview: "#3b82f6",
  rejected: "#ef4444",
  suspended: "#6b7280",
};

const typeColors = {
  individual: "#8b5cf6",
  company: "#06b6d4",
  freelancer: "#f97316",
  cooperative: "#ec4899",
};

export default function DashboardPage() {
  const { data: stats, isLoading, error } = useDashboardStats();

  // ... (charts and KPI cards)
}
```

### Design Details
- **KPI Cards Grid**: `grid gap-6 md:grid-cols-2 lg:grid-cols-4`
- **Charts Grid**: `grid gap-6 md:grid-cols-2`
- **Card Structure**:
  - CardHeader with title and icon
  - CardContent with large number and trend
- **Charts**: Recharts PieChart with inner radius (donut style)

---

## Route: `/providers`

### Page Code (Table with Pagination)
```tsx
// src/app/(admin)/providers/page.tsx
"use client";

import { useState } from "react";
import { Search, Plus, Eye, CheckCircle, XCircle, Trash2, Loader2, ChevronLeft, ChevronRight } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";

const ITEMS_PER_PAGE = 10;

// Badge variants based on verification status
const getVerificationBadgeVariant = (status?: VerificationStatus) => {
  switch (status) {
    case 2: return "success" as const;
    case 0: return "warning" as const;
    case 3:
    case 4: return "destructive" as const;
    default: return "secondary" as const;
  }
};
```

### Design Details
- **Search Bar**: Card with relative positioning, search icon absolute left
- **Table**: Full width, header with border-b, cells with padding
- **Pagination**: Fixed bottom bar with prev/next buttons and page numbers
- **Action Buttons**: Icon buttons for view, approve, reject, delete

---

## Route: `/providers/[id]`

### Page Code (Detail View)
```tsx
// src/app/(admin)/providers/[id]/page.tsx
"use client";

import { ArrowLeft, Mail, Phone, MapPin, FileText, CheckCircle, XCircle } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
```

### Design Details
- **Back Link**: Inline with ArrowLeft icon
- **Header**: Title, badges row, action buttons row
- **Cards Grid**: `grid gap-6 md:grid-cols-2`
- **Span Columns**: `md:col-span-2` for full-width cards
- **Dialogs**: Approve/Reject confirmation dialogs

---

## Route: `/categories`, `/services`, `/allowed-cities`, `/documents`

All CRUD pages follow the same pattern:

### Common Structure
```tsx
// Search Card
<Card className="mb-6">
  <div className="p-4">
    <div className="relative">
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input className="pl-10" placeholder="Buscar..." />
    </div>
  </div>
</Card>

// Data Table Card
<Card>
  {isLoading && <Loader2 />}
  {error && <div>Error message</div>}
  {!isLoading && !error && (
    <table className="w-full">
      <thead><tr className="border-b border-border">...</tr></thead>
      <tbody>...</tbody>
    </table>
  )}
</Card>

// Dialogs for Create/Edit/Delete
<Dialog open={isOpen} onOpenChange={setIsOpen}>
  <DialogContent>
    <DialogHeader>
      <DialogTitle>...</DialogTitle>
      <DialogDescription>...</DialogDescription>
    </DialogHeader>
    <form>Form fields</form>
    <DialogFooter>
      <Button variant="outline">Cancelar</Button>
      <Button>Confirmar</Button>
    </DialogFooter>
  </DialogContent>
</Dialog>
```

---

## Route: `/settings`

### Page Code (Tabs Layout)
```tsx
// src/app/(admin)/settings/page.tsx
"use client";

import { Settings, User, Bell, Shield, Palette, Save } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";

type SettingsTab = "profile" | "notifications" | "security" | "appearance";

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState<SettingsTab>("profile");

  // Tabs: profile | notifications | security | appearance
}
```

### Design Details
- **Layout**: `grid gap-6 lg:grid-cols-4` (sidebar + content)
- **Tabs Navigation**: Vertical list in Card
- **Active Tab**: `bg-primary text-primary-foreground`
- **Inactive Tab**: `text-muted-foreground hover:bg-muted`

---

## Components

### Button
```tsx
// Variants
primary: "bg-primary text-primary-foreground hover:bg-primary-hover"
secondary: "bg-secondary text-secondary-foreground hover:bg-secondary-hover"
ghost: "border-transparent bg-transparent text-muted-foreground hover:text-foreground hover:bg-muted"
destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90"

// Sizes
sm: "h-9 px-3 text-sm"
md: "h-10 px-4 text-sm"
lg: "h-11 px-6 text-base"
icon: "h-10 w-10"
```

### Card
```tsx
// Container
rounded-xl border border-border bg-surface p-6 shadow-sm

// Header
flex flex-col gap-1.5

// Title
text-lg font-semibold

// Content
pt-2
```

### Badge
```tsx
// Variants
default: "border-transparent bg-primary text-primary-foreground"
secondary: "border-transparent bg-secondary text-secondary-foreground"
destructive: "border-transparent bg-destructive text-destructive-foreground"
success: "border-transparent bg-green-100 text-green-800"
warning: "border-transparent bg-yellow-100 text-yellow-800"
```

### Input
```tsx
flex h-10 w-full rounded-lg border border-input bg-background px-3 py-2 text-sm
focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring
disabled:cursor-not-allowed disabled:opacity-50
```

### Dialog (Base UI)
```tsx
// Overlay
fixed inset-0 z-50 bg-black/50 backdrop-blur-sm

// Content
fixed left-[50%] top-[50%] z-50 translate-x-[-50%] translate-y-[-50%]
w-full max-w-lg rounded-lg border border-border bg-background p-6 shadow-lg

// Animation
data-[state=open]:animate-in
data-[state=closed]:animate-out
```

### Select (Base UI)
```tsx
// Trigger
flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2

// Item
flex w-full cursor-default select-none items-center rounded-sm py-1.5 pl-8 pr-2 text-sm
```

### Theme Toggle
```tsx
// Button with Moon/Sun icons
// Uses useTheme hook from theme-provider
```

---

## CSS Variables (Tailwind v4)

```css
/* src/app/global.css */
@import "tailwindcss";

@theme inline {
  /* Primary - Blue */
  --color-primary: #395873;
  --color-primary-foreground: #ffffff;
  --color-primary-hover: #2E4760;

  /* Secondary */
  --color-secondary: #f5f5f5;
  --color-secondary-foreground: #2e2e2e;

  /* Muted */
  --color-muted: #f5f5f5;
  --color-muted-foreground: #666666;

  /* Destructive */
  --color-destructive: #dc2626;
  --color-destructive-foreground: #ffffff;

  /* Border & Input */
  --color-border: #e0e0e0;
  --color-input: #e0e0e0;

  /* Background & Surface */
  --color-background: #ffffff;
  --color-surface: #ffffff;
  --color-surface-raised: #f5f5f5;

  /* Foreground */
  --color-foreground: #2e2e2e;
  --color-foreground-subtle: #666666;

  /* Card */
  --color-card: #ffffff;
  --color-card-foreground: #2e2e2e;

  /* Ring */
  --color-ring: #395873;

  /* Radius */
  --radius-sm: 0.375rem;
  --radius-md: 0.5rem;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;
}

/* Light Mode */
:root {
  --background: #ffffff;
  --foreground: #2e2e2e;
  --surface: #ffffff;
  --surface-raised: #f5f5f5;
  --foreground-subtle: #666666;
  --border: #e0e0e0;
  --input: #e0e0e0;
}

/* Dark Mode */
@media (prefers-color-scheme: dark) {
  :root {
    --background: #0a0a0a;
    --foreground: #ededed;
    --surface: #1a1a1a;
    --surface-raised: #262626;
    --foreground-subtle: #a3a3a3;
    --border: #404040;
    --input: #404040;
  }
}
```

---

## Sidebar Layout

```tsx
// src/components/layout/sidebar.tsx
<aside className="fixed left-0 top-0 z-40 flex h-screen w-64 flex-col border-r border-border bg-surface">
  {/* Logo */}
  <div className="flex h-16 items-center border-b border-border px-6">
    <Link href="/dashboard" className="flex items-center gap-2">
      <span className="text-xl font-bold text-primary">MeAjudaAí</span>
      <span className="text-xs font-medium text-muted-foreground">Admin</span>
    </Link>
  </div>

  {/* Navigation */}
  <nav className="flex-1 space-y-1 p-4">
    {navItems.map((item) => {
      const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
      return (
        <Link
          key={item.href}
          href={item.href}
          className={twMerge(
            "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
            isActive
              ? "bg-primary text-primary-foreground"
              : "text-foreground-subtle hover:bg-muted hover:text-foreground"
          )}
        >
          <Icon className="h-5 w-5" />
          {item.label}
        </Link>
      );
    })}
  </nav>

  {/* User Info & Theme */}
  <div className="border-t border-border p-4">
    {/* Avatar + Name + Theme Toggle */}
    {/* Logout Button */}
  </div>
</aside>

/* Main Content */
<main className="flex-1 ml-64">
  {/* Pages render here */}
</main>
```

### Navigation Items
```tsx
const navItems = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/providers", label: "Prestadores", icon: Users },
  { href: "/documents", label: "Documentos", icon: FileText },
  { href: "/categories", label: "Categorias", icon: FolderTree },
  { href: "/services", label: "Serviços", icon: Wrench },
  { href: "/allowed-cities", label: "Cidades", icon: MapPin },
  { href: "/settings", label: "Configurações", icon: Settings },
];
```

---

## Icons (Lucide React)

Available icons used throughout the application:

| Icon | Usage |
|------|-------|
| `LayoutDashboard` | Dashboard nav |
| `Users` | Providers nav, KPI |
| `FileText` | Documents nav, documents section |
| `FolderTree` | Categories nav |
| `Wrench` | Services nav |
| `MapPin` | Cities nav, toggle active |
| `Settings` | Settings nav |
| `LogOut` | Logout button |
| `Search` | Search input |
| `Plus` | Create buttons |
| `Pencil` | Edit buttons |
| `Trash2` | Delete buttons |
| `Eye` | View buttons |
| `CheckCircle` | Approve, success states |
| `XCircle` | Reject, suspended states |
| `Loader2` | Loading spinner |
| `ChevronLeft` | Pagination prev |
| `ChevronRight` | Pagination next |
| `Mail` | Email field |
| `Phone` | Phone field |
| `ArrowLeft` | Back link |
| `Moon/Sun` | Theme toggle |
| `Shield` | Login logo |
| `AlertCircle` | Error messages |
| `TrendingUp` | Dashboard trends |
| `Clock` | Pending items |
| `AlertCircle` | Rejected KPI |

---

## Form Validation (Zod Schemas)

### Category Schema
```tsx
const categorySchema = z.object({
  name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres").max(100, "Nome deve ter no máximo 100 caracteres"),
  description: z.string().max(500, "Descrição deve ter no máximo 500 caracteres").optional(),
  isActive: z.boolean(),
});
```

### Service Schema
```tsx
const serviceSchema = z.object({
  name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres").max(100, "Nome deve ter no máximo 100 caracteres"),
  description: z.string().max(500, "Descrição deve ter no máximo 500 caracteres").optional(),
  categoryId: z.string().min(1, "Selecione uma categoria"),
  isActive: z.boolean(),
});
```

### City Schema
```tsx
const citySchema = z.object({
  city: z.string().min(2, "Cidade deve ter pelo menos 2 caracteres").max(100, "Cidade deve ter no máximo 100 caracteres"),
  state: z.string().min(1, "Selecione um estado"),
  serviceRadiusKm: z.coerce.number().min(1, "Raio deve ser pelo menos 1 km").max(500, "Raio máximo é 500 km"),
  isActive: z.boolean(),
});
```

---

## State Labels

### Provider Types
```tsx
export const providerTypeLabels: Record<ProviderType, string> = {
  0: "Não definido",
  1: "Pessoa Física",
  2: "Empresa",
  3: "Cooperativa",
  4: "Freelancer",
};
```

### Verification Status
```tsx
export const verificationStatusLabels: Record<VerificationStatus, string> = {
  0: "Pendente",
  1: "Em Análise",
  2: "Aprovado",
  3: "Rejeitado",
  4: "Suspenso",
  5: "Correção de Dados Necessária",
};
```

### Provider Status
```tsx
export const providerStatusLabels: Record<ProviderStatus, string> = {
  0: "Pendente",
  1: "Dados Básicos Necessários",
  2: "Dados Básicos Enviados",
  3: "Documentos Necessários",
  4: "Documentos Enviados",
  5: "Ativo",
};
```

### Provider Tiers
```tsx
export const providerTierLabels: Record<ProviderTier, string> = {
  0: "Grátis",
  1: "Básico",
  2: "Premium",
  3: "Enterprise",
};
```

---

## Tech Stack

| Category | Technology |
|----------|------------|
| Framework | Next.js 15 (React 19) |
| Language | TypeScript (strict mode) |
| Styling | Tailwind CSS v4 |
| Components | @base-ui/react |
| State | TanStack Query + useState |
| Forms | react-hook-form + Zod |
| Icons | Lucide React |
| Charts | Recharts |
| Auth | next-auth + Keycloak |
| Toasts | Sonner |
| Utilities | tailwind-merge, class-variance-authority |

---

## File Structure

```
src/Web/MeAjudaAi.Web.Admin-React/
├── src/
│   ├── app/
│   │   ├── global.css                    # Tailwind v4 theme
│   │   ├── layout.tsx                    # Root layout with providers
│   │   ├── page.tsx                      # Redirects to /dashboard
│   │   ├── login/
│   │   │   └── page.tsx                  # Login page
│   │   └── (admin)/
│   │       ├── layout.tsx                # Admin layout with sidebar
│   │       ├── dashboard/page.tsx
│   │       ├── providers/
│   │       │   ├── page.tsx
│   │       │   └── [id]/page.tsx
│   │       ├── documents/page.tsx
│   │       ├── categories/page.tsx
│   │       ├── services/page.tsx
│   │       ├── allowed-cities/page.tsx
│   │       └── settings/page.tsx
│   ├── components/
│   │   ├── layout/
│   │   │   └── sidebar.tsx
│   │   ├── providers/
│   │   │   ├── app-providers.tsx
│   │   │   ├── theme-provider.tsx
│   │   │   └── toast-provider.tsx
│   │   └── ui/
│   │       ├── button.tsx
│   │       ├── badge.tsx
│   │       ├── card.tsx
│   │       ├── input.tsx
│   │       ├── dialog.tsx
│   │       ├── select.tsx
│   │       └── theme-toggle.tsx
│   ├── hooks/admin/
│   │   ├── index.ts
│   │   ├── use-providers.ts
│   │   ├── use-allowed-cities.ts
│   │   ├── use-categories.ts
│   │   ├── use-users.ts
│   │   ├── use-services.ts
│   │   └── use-dashboard.ts
│   ├── lib/
│   │   ├── auth/auth.ts
│   │   ├── types.ts
│   │   └── api/generated/
│   └── middleware.ts
├── tailwind.config.js
├── tsconfig.json
└── next.config.js
```

---

## Brazilian States List

```tsx
const brazilianStates = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
  "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
  "RS", "RO", "RR", "SC", "SP", "SE", "TO"
];
```

---

## Chart Colors

### Verification Status
```tsx
const verificationColors = {
  approved: "#22c55e",      // green
  pending: "#f59e0b",        // yellow
  underReview: "#3b82f6",   // blue
  rejected: "#ef4444",       // red
  suspended: "#6b7280",      // gray
};
```

### Provider Types
```tsx
const typeColors = {
  individual: "#8b5cf6",    // purple
  company: "#06b6d4",       // cyan
  freelancer: "#f97316",    // orange
  cooperative: "#ec4899",    // pink
};
```

---

## Pagination

```tsx
const ITEMS_PER_PAGE = 10;

// Calculate
const totalPages = Math.ceil(filteredItems.length / ITEMS_PER_PAGE);
const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
const paginatedItems = filteredItems.slice(startIndex, startIndex + ITEMS_PER_PAGE);

// Reset on search
const handleSearch = (value: string) => {
  setSearch(value);
  setCurrentPage(1);
};
```

---

## Toast Notifications (Sonner)

```tsx
import { toast } from "sonner";

// Success
toast.success("Categoria criada com sucesso");

// Error
toast.error("Erro ao criar categoria");
```

---

## API Types

All API types are generated from OpenAPI spec using `@hey-api/openapi-ts`.

### Key Types
```tsx
export type ProviderDto = MeAjudaAiModulesProvidersApplicationDtosProviderDto;
export type BusinessProfileDto = MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto;
export type DocumentDto = MeAjudaAiModulesProvidersApplicationDtosDocumentDto;
export type ServiceCategoryDto = MeAjudaAiModulesProvidersApplicationDtosServiceCategoryDto;
export type AllowedCityDto = MeAjudaAiModulesLocationsApplicationDtosAllowedCityDto;
export type UserDto = MeAjudaAiModulesUsersApplicationDtosUserDto;
```
