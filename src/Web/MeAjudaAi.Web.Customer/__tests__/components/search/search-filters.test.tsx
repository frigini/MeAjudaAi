import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { SearchFilters } from '@/components/search/search-filters';

const mockPush = vi.fn();

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  useSearchParams: vi.fn(() => new URLSearchParams()),
}));

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiCategoriesGet: vi.fn(() => Promise.resolve({ data: { data: [] } })),
}));

describe('SearchFilters', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    render(<SearchFilters />);
  });

  it('deve renderizar o componente de filtros e carregar categorias', async () => {
    const { apiCategoriesGet } = await import('@/lib/api/generated/sdk.gen');
    await waitFor(() => {
      expect(screen.getByText(/distância/i)).toBeInTheDocument();
      expect(apiCategoriesGet).toHaveBeenCalledWith(expect.objectContaining({ activeOnly: true }));
    });
  });

  it('deve renderizar filtro de avaliação', async () => {
    await waitFor(() => {
      expect(screen.getAllByText(/avaliação/i).length).toBeGreaterThan(0);
    });
  });

  it('deve renderizar categorias', async () => {
    await waitFor(() => {
      expect(screen.getByText(/categorias/i)).toBeInTheDocument();
    });
  });

  it('deve renderizar filtro de distância com valores padrão', async () => {
    await waitFor(() => {
      expect(screen.getByDisplayValue('50')).toBeInTheDocument();
    });
  });

  it('deve renderizar opção qualquer avaliação', async () => {
    await waitFor(() => {
      expect(screen.getByText(/qualquer avaliação/i)).toBeInTheDocument();
    });
  });

  it('deve renderizar limite mínimo de 5km', async () => {
    await waitFor(() => {
      expect(screen.getByText('5km')).toBeInTheDocument();
    });
  });

  it('deve renderizar limite máximo de 100km', async () => {
    await waitFor(() => {
      expect(screen.getByText('100km')).toBeInTheDocument();
    });
  });
});
