import type {
  MeAjudaAiModulesProvidersApplicationDtosProviderDto,
  MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto,
  MeAjudaAiModulesProvidersApplicationDtosDocumentDto,
  MeAjudaAiModulesServiceCatalogsApplicationDtosServiceCategoryDto,
  MeAjudaAiModulesLocationsApplicationDtosAllowedCityDto,
  MeAjudaAiModulesUsersApplicationDtosUserDto,
} from "./api/generated";

export type ProviderDto = MeAjudaAiModulesProvidersApplicationDtosProviderDto;
export type BusinessProfileDto = MeAjudaAiModulesProvidersApplicationDtosBusinessProfileDto;
export type DocumentDto = MeAjudaAiModulesProvidersApplicationDtosDocumentDto;
export type ServiceCategoryDto = MeAjudaAiModulesServiceCatalogsApplicationDtosServiceCategoryDto;
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
