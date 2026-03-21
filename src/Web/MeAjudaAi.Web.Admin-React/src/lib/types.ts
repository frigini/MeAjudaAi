import type {
  MeAjudaAiModulesProvidersApplicationDtosProviderDto,
  MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto,
  MeAjudaAiModulesProvidersApplicationDtosDocumentDto,
  MeAjudaAiModulesProvidersApplicationDtosServiceCategoryDto,
  MeAjudaAiModulesLocationsApplicationDtosAllowedCityDto,
  MeAjudaAiModulesUsersApplicationDtosUserDto,
} from "./api/generated";

export type ProviderDto = MeAjudaAiModulesProvidersApplicationDtosProviderDto;
export type BusinessProfileDto = MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto;
export type DocumentDto = MeAjudaAiModulesProvidersApplicationDtosDocumentDto;
export type ServiceCategoryDto = MeAjudaAiModulesProvidersApplicationDtosServiceCategoryDto;
export type AllowedCityDto = MeAjudaAiModulesLocationsApplicationDtosAllowedCityDto;
export type UserDto = MeAjudaAiModulesUsersApplicationDtosUserDto;

export type ProviderType = 0 | 1 | 2 | 3 | 4;
export type ProviderStatus = 0 | 1 | 2 | 3 | 4 | 5;
export type VerificationStatus = 0 | 1 | 2 | 3 | 4 | 5;
export type ProviderTier = 0 | 1 | 2 | 3;

export const EProviderType = {
  None: 0,
  Individual: 1,
  Company: 2,
  Cooperative: 3,
  Freelancer: 4,
} as const;

export const EProviderStatus = {
  Pending: 0,
  BasicInfoRequired: 1,
  BasicInfoSubmitted: 2,
  DocumentsRequired: 3,
  DocumentsSubmitted: 4,
  Active: 5,
} as const;

export const EVerificationStatus = {
  Pending: 0,
  UnderReview: 1,
  Approved: 2,
  Rejected: 3,
  Suspended: 4,
  BasicInfoCorrectionRequired: 5,
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
  0: "Pendente",
  1: "Dados Básicos Necessários",
  2: "Dados Básicos Enviados",
  3: "Documentos Necessários",
  4: "Documentos Enviados",
  5: "Ativo",
};

export const verificationStatusLabels: Record<VerificationStatus, string> = {
  0: "Pendente",
  1: "Em Análise",
  2: "Aprovado",
  3: "Rejeitado",
  4: "Suspenso",
  5: "Correção de Dados Necessária",
};

export const providerTierLabels: Record<ProviderTier, string> = {
  0: "Grátis",
  1: "Básico",
  2: "Premium",
  3: "Enterprise",
};
