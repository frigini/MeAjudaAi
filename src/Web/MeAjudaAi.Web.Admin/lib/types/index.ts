import type {
  MeAjudaAiModulesProvidersApplicationDtosProviderDto,
  MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto,
  MeAjudaAiModulesProvidersApplicationDtosDocumentDto,
  MeAjudaAiModulesServiceCatalogsApplicationDtosServiceCategoryDto,
  MeAjudaAiModulesLocationsApplicationDtosAllowedCityDto,
  MeAjudaAiModulesUsersApplicationDtosUserDto,
} from "../api/generated";
import { z } from "zod";
import { zMeAjudaAiModulesProvidersApplicationDtosProviderDto } from "../api/generated/zod.gen";

export type ProviderDto = MeAjudaAiModulesProvidersApplicationDtosProviderDto;
export type BusinessProfileDto = MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto;
export type DocumentDto = MeAjudaAiModulesProvidersApplicationDtosDocumentDto;
export type ServiceCategoryDto = MeAjudaAiModulesServiceCatalogsApplicationDtosServiceCategoryDto;
export type AllowedCityDto = MeAjudaAiModulesLocationsApplicationDtosAllowedCityDto;
export type UserDto = MeAjudaAiModulesUsersApplicationDtosUserDto;

type ProviderSchemaType = z.infer<typeof zMeAjudaAiModulesProvidersApplicationDtosProviderDto>;

export type ProviderType = NonNullable<ProviderSchemaType["type"]>;
export type ProviderStatus = NonNullable<ProviderSchemaType["status"]>;
export type VerificationStatus = NonNullable<ProviderSchemaType["verificationStatus"]>;
export type ProviderTier = NonNullable<ProviderSchemaType["tier"]>;

export const EProviderType = {
  None: 0,
  Individual: 1,
  Company: 2,
  Cooperative: 3,
  Freelancer: 4,
} as const;

export const EProviderStatus = {
  None: 0,
  PendingBasicInfo: 1,
  PendingDocumentVerification: 2,
  Active: 3,
  Suspended: 4,
  Rejected: 5,
} as const;

export const EVerificationStatus = {
  None: 0,
  Pending: 1,
  InProgress: 2,
  Verified: 3,
  Rejected: 4,
  Suspended: 5,
} as const;

export const EProviderTier = {
  Free: 0,
  Basic: 1,
  Premium: 2,
  Enterprise: 3,
} as const;

export const providerTypeLabels: Record<ProviderType, string> = {
  0: "Não definido",
  1: "Pessoa Física",
  2: "Empresa",
  3: "Cooperativa",
  4: "Freelancer",
};

export const providerStatusLabels: Record<ProviderStatus, string> = {
  0: "Não cadastrado",
  1: "Pendente (Dados Básicos)",
  2: "Em Análise",
  3: "Ativo",
  4: "Suspenso",
  5: "Rejeitado",
};

export const verificationStatusLabels: Record<VerificationStatus, string> = {
  0: "Não Iniciado",
  1: "Pendente",
  2: "Em Análise",
  3: "Verificado",
  4: "Rejeitado",
  5: "Suspenso",
};

export const providerTierLabels: Record<ProviderTier, string> = {
  0: "Grátis",
  1: "Básico",
  2: "Premium",
  3: "Enterprise",
};

export const BRAZILIAN_STATES = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
  "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
  "RS", "RO", "RR", "SC", "SP", "SE", "TO"
] as const;

export const CATEGORY_STATUS_LABELS = {
  ACTIVE: { label: "Ativa", variant: "success" },
  INACTIVE: { label: "Inativa", variant: "secondary" },
} as const;

export const ROLES = {
  ADMIN: "admin",
  USER: "user",
} as const;

export const APP_ROUTES = {
  DASHBOARD: "/dashboard",
  PROVIDERS: "/providers",
  DOCUMENTS: "/documents",
  CATEGORIES: "/categories",
  SERVICES: "/services",
  CITIES: "/allowed-cities",
  SETTINGS: "/settings",
} as const;

export const APP_ROUTE_LABELS = {
  DASHBOARD: "Dashboard",
  PROVIDERS: "Prestadores",
  DOCUMENTS: "Documentos",
  CATEGORIES: "Categorias",
  SERVICES: "Serviços",
  CITIES: "Cidades",
  SETTINGS: "Configurações",
} as const;
