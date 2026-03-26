import { render, screen } from '@testing-library/react';
import SearchPage from '@/app/(main)/buscar/page';
import { apiProvidersGet4, apiCategoryGet } from '@/lib/api/generated/sdk.gen';
import { geocodeCity } from '@/lib/services/geocoding';
import { describe, it, expect, vi, beforeEach } from 'vitest';

// Mock components
vi.mock('@/components/search/search-filters', () => ({
  SearchFilters: () => <div data-testid="search-filters">Filters</div>
}));

vi.mock('@/components/search/service-tags', () => ({
  ServiceTags: () => <div data-testid="service-tags">Tags</div>
}));

vi.mock('@/components/service/service-card', () => ({
  ServiceCard: ({ name }: { name: string }) => <div data-testid="service-card">{name}</div>
}));

vi.mock('@/components/search/ad-card', () => ({
  AdCard: () => <div data-testid="ad-card">Ad</div>
}));

// Mock SDK
vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiProvidersGet4: vi.fn(),
  apiCategoryGet: vi.fn(),
}));

// Mock services
vi.mock('@/lib/services/geocoding', () => ({
  geocodeCity: vi.fn(),
}));

vi.mock('@/lib/api/auth-headers', () => ({
  getAuthHeaders: vi.fn(() => Promise.resolve({ Authorization: 'Bearer token' })),
}));

describe('SearchPage', () => {
  const mockProviders = [
    { providerId: '1', name: 'Provider 1', averageRating: 4.5, totalReviews: 10, serviceIds: ['s1'], city: 'Linhares', state: 'ES' },
    { providerId: '2', name: 'Provider 2', averageRating: 4.0, totalReviews: 5, serviceIds: ['s2'], city: 'Linhares', state: 'ES' },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(apiProvidersGet4).mockResolvedValue({ data: { items: mockProviders }, error: null } as any);
    vi.mocked(geocodeCity).mockResolvedValue(null);
  });

  it('deve renderizar a página com resultados padrão', async () => {
    const searchParams = Promise.resolve({});
    const result = await SearchPage({ searchParams });
    render(result);

    expect(screen.getByText('Provider 1')).toBeInTheDocument();
    expect(screen.getByText('Provider 2')).toBeInTheDocument();
    expect(screen.getByTestId('ad-card')).toBeInTheDocument();
  });

  it('deve utilizar geocoding se a cidade for fornecida', async () => {
    vi.mocked(geocodeCity).mockResolvedValue({ latitude: -20, longitude: -40, displayName: 'Vitória, ES' });
    
    const searchParams = Promise.resolve({ city: 'Vitória' });
    const result = await SearchPage({ searchParams });
    render(result);

    expect(geocodeCity).toHaveBeenCalledWith('Vitória');
    expect(apiProvidersGet4).toHaveBeenCalledWith(expect.objectContaining({
      query: expect.objectContaining({
        latitude: -20,
        longitude: -40,
      })
    }));
  });

  it('deve filtrar por categoria e buscar serviços associados', async () => {
    vi.mocked(apiCategoryGet).mockResolvedValue({ 
      data: { data: [{ id: 's1' }, { id: 's2' }] },
      error: null 
    } as any);

    const searchParams = Promise.resolve({ categoryId: 'cat-1' });
    const result = await SearchPage({ searchParams });
    render(result);

    expect(apiCategoryGet).toHaveBeenCalled();
    expect(apiProvidersGet4).toHaveBeenCalledWith(expect.objectContaining({
      query: expect.objectContaining({
        serviceIds: ['s1', 's2']
      })
    }));
  });

  it('deve mostrar estado vazio quando não há resultados', async () => {
    vi.mocked(apiProvidersGet4).mockResolvedValue({ data: { items: [] }, error: null } as any);

    const searchParams = Promise.resolve({ q: 'inexistente' });
    const result = await SearchPage({ searchParams });
    render(result);

    expect(screen.getByText(/nenhum prestador encontrado/i)).toBeInTheDocument();
  });

  it('deve lançar erro se a API de busca falhar', async () => {
    vi.mocked(apiProvidersGet4).mockResolvedValue({ data: null, error: { message: 'Erro' } } as any);

    const searchParams = Promise.resolve({});
    await expect(SearchPage({ searchParams })).rejects.toThrow('Failed to fetch providers');
  });

  it('deve lançar erro se a API de categorias falhar', async () => {
    vi.mocked(apiCategoryGet).mockRejectedValue(new Error('API Error'));

    const searchParams = Promise.resolve({ categoryId: 'cat-1' });
    await expect(SearchPage({ searchParams })).rejects.toThrow('Failed to validate category filter');
  });
});
