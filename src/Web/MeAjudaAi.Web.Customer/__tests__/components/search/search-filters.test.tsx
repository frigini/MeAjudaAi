import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { SearchFilters } from '@/components/search/search-filters';

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  useSearchParams: vi.fn(() => new URLSearchParams()),
}));

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiCategoriesGet: vi.fn(() => Promise.resolve({ data: { data: [] } })),
}));

describe('SearchFilters', () => {
  it('deve renderizar o componente de filtros', () => {
    render(<SearchFilters />);
    expect(screen.getByText(/distância/i)).toBeInTheDocument();
  });

  it('deve renderizar filtro de avaliação', () => {
    render(<SearchFilters />);
    expect(screen.getAllByText(/avaliação/i).length).toBeGreaterThan(0);
  });

  it('deve renderizar categorias', () => {
    render(<SearchFilters />);
    expect(screen.getByText(/categorias/i)).toBeInTheDocument();
  });

  it('deve renderizar filtro de distância com valores padrão', () => {
    render(<SearchFilters />);
    expect(screen.getByDisplayValue('50')).toBeInTheDocument();
  });

  it('deve renderizar opção qualquer avaliação', () => {
    render(<SearchFilters />);
    expect(screen.getByText(/qualquer avaliação/i)).toBeInTheDocument();
  });

  it('deve renderizar limite mínimo de 5km', () => {
    render(<SearchFilters />);
    expect(screen.getByText('5km')).toBeInTheDocument();
  });

  it('deve renderizar limite máximo de 100km', () => {
    render(<SearchFilters />);
    expect(screen.getByText('100km')).toBeInTheDocument();
  });
});
