import { render, screen } from '@testing-library/react';
import SearchPage from '@/app/(main)/buscar/page';
import { describe, it, expect, vi, beforeEach } from 'vitest';

// ─── Mock next/navigation ───────────────────────────────────────────────────
const mockGet = vi.fn();
vi.mock('next/navigation', () => ({
  useSearchParams: () => ({ get: mockGet }),
  useRouter: () => ({ push: vi.fn() }),
  usePathname: () => '/buscar',
}));

// ─── Mock @tanstack/react-query ──────────────────────────────────────────────
const mockUseQuery = vi.fn();
vi.mock('@tanstack/react-query', () => ({
  useQuery: (opts: any) => mockUseQuery(opts),
}));

// ─── Mock child components ───────────────────────────────────────────────────
vi.mock('@/components/search/search-filters', () => ({
  SearchFilters: () => <div data-testid="search-filters">Filters</div>,
}));

vi.mock('@/components/search/service-tags', () => ({
  ServiceTags: () => <div data-testid="service-tags">Tags</div>,
}));

vi.mock('@/components/service/service-card', () => ({
  ServiceCard: ({ name }: { name: string }) => <div data-testid="service-card">{name}</div>,
}));

vi.mock('@/components/search/ad-card', () => ({
  AdCard: () => <div data-testid="ad-card">Ad</div>,
}));

// ─── Mock SDK / services ─────────────────────────────────────────────────────
vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiProvidersGet4: vi.fn(),
  apiCategoryGet: vi.fn(),
}));

vi.mock('@/lib/services/geocoding', () => ({
  geocodeCity: vi.fn(),
}));

vi.mock('@/lib/api/auth-headers', () => ({
  getAuthHeaders: vi.fn(() => Promise.resolve({ Authorization: 'Bearer token' })),
}));

// ─── Mock mapper ─────────────────────────────────────────────────────────────
vi.mock('@/lib/api/mappers', () => ({
  mapSearchableProviderToProvider: (p: any) => ({
    id: p.providerId,
    name: p.name,
    averageRating: p.averageRating,
    reviewCount: p.totalReviews,
    services: [],
    avatarUrl: null,
    description: '',
  }),
}));

// ─── Helpers ─────────────────────────────────────────────────────────────────
function setSearchParam(key: string, value: string | null) {
  mockGet.mockImplementation((k: string) => (k === key ? value : null));
}

function setSearchParams(params: Record<string, string | null>) {
  mockGet.mockImplementation((k: string) => params[k] ?? null);
}

const mockProviders = [
  { providerId: '1', name: 'Provider 1', averageRating: 4.5, totalReviews: 10, serviceIds: ['s1'], city: 'Linhares', state: 'ES' },
  { providerId: '2', name: 'Provider 2', averageRating: 4.0, totalReviews: 5, serviceIds: ['s2'], city: 'Linhares', state: 'ES' },
];

describe('SearchPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Default: no search params
    mockGet.mockReturnValue(null);
  });

  it('deve renderizar a página com resultados padrão', () => {
    mockUseQuery.mockReturnValue({
      data: { items: mockProviders },
      isLoading: false,
      error: null,
    });

    render(<SearchPage />);

    expect(screen.getByText('Provider 1')).toBeInTheDocument();
    expect(screen.getByText('Provider 2')).toBeInTheDocument();
    expect(screen.getByTestId('ad-card')).toBeInTheDocument();
  });

  it('deve utilizar query key correta quando cidade é fornecida', () => {
    setSearchParam('city', 'Vitória');
    mockUseQuery.mockReturnValue({
      data: { items: mockProviders },
      isLoading: false,
      error: null,
    });

    render(<SearchPage />);

    // useQuery must have been called with city in the queryKey
    expect(mockUseQuery).toHaveBeenCalledWith(
      expect.objectContaining({
        queryKey: expect.arrayContaining(['Vitória']),
      })
    );
  });

  it('deve filtrar por categoria quando categoryId é fornecido', () => {
    setSearchParam('categoryId', 'cat-1');
    mockUseQuery.mockReturnValue({
      data: { items: mockProviders },
      isLoading: false,
      error: null,
    });

    render(<SearchPage />);

    expect(mockUseQuery).toHaveBeenCalledWith(
      expect.objectContaining({
        queryKey: expect.arrayContaining(['cat-1']),
      })
    );
  });

  it('deve mostrar estado vazio quando não há resultados', () => {
    setSearchParam('q', 'inexistente');
    mockUseQuery.mockReturnValue({
      data: { items: [] },
      isLoading: false,
      error: null,
    });

    render(<SearchPage />);

    expect(screen.getByText(/nenhum prestador encontrado/i)).toBeInTheDocument();
  });

  it('deve mostrar estado de carregamento', () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    });

    render(<SearchPage />);

    expect(screen.getByText(/buscando os melhores profissionais/i)).toBeInTheDocument();
  });

  it('deve mostrar mensagem de erro quando a API falha', () => {
    mockUseQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('API Error'),
    });

    render(<SearchPage />);

    expect(screen.getByText(/erro na busca/i)).toBeInTheDocument();
  });
});
