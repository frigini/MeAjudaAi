export function createProvider(overrides = {}) {
  return {
    id: 'test-provider-id',
    name: 'Prestador Teste',
    slug: 'prestador-teste',
    email: 'prestador@teste.com',
    phone: '21999999999',
    verificationStatus: 'Pending',
    ...overrides,
  };
}

export function createUser(overrides = {}) {
  return {
    id: 'test-user-id',
    name: 'Usuário Teste',
    email: 'usuario@teste.com',
    ...overrides,
  };
}

export function createService(overrides = {}) {
  return {
    id: 'test-service-id',
    name: 'Serviço Teste',
    categoryId: 'test-category-id',
    ...overrides,
  };
}

export function createReview(overrides = {}) {
  return {
    id: 'test-review-id',
    rating: 5,
    text: 'Excelente serviço!',
    reviewerName: 'Avaliador Teste',
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}
