import { ProviderDto, UserDto, ServiceDto, ReviewDto } from '../types';

export function createProvider(overrides: Partial<ProviderDto> = {}): ProviderDto {
  return {
    id: 'test-provider-id',
    name: 'Prestador Teste',
    slug: 'prestador-teste',
    email: 'prestador@teste.com',
    phone: '21999999999',
    verificationStatus: 'Pending',
    ...overrides,
  } as ProviderDto;
}

export function createUser(overrides: Partial<UserDto> = {}): UserDto {
  return {
    id: 'test-user-id',
    name: 'Usuário Teste',
    email: 'usuario@teste.com',
    ...overrides,
  } as UserDto;
}

export function createService(overrides: Partial<ServiceDto> = {}): ServiceDto {
  return {
    id: 'test-service-id',
    name: 'Serviço Teste',
    categoryId: 'test-category-id',
    ...overrides,
  } as ServiceDto;
}

export function createReview(overrides: Partial<ReviewDto> = {}): ReviewDto {
  return {
    id: 'test-review-id',
    rating: 5,
    text: 'Excelente serviço!',
    reviewerName: 'Avaliador Teste',
    createdAt: new Date().toISOString(),
    ...overrides,
  } as ReviewDto;
}
