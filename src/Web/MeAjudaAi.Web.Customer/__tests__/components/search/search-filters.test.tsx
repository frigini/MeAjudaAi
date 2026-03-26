import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent, cleanup } from '@testing-library/react';
import { apiCategoriesGet } from '@/lib/api/generated/sdk.gen';
import { SearchFilters } from '@/components/search/search-filters';

const { mockPush, mockUseSearchParams } = vi.hoisted(() => ({
  mockPush: vi.fn(),
  mockUseSearchParams: vi.fn(() => new URLSearchParams()),
}));

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  useSearchParams: mockUseSearchParams,
}));

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiCategoriesGet: vi.fn(() => Promise.resolve({ data: { data: [] } })),
}));

describe('SearchFilters', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    cleanup();
    render(<SearchFilters />);
  });

  it('deve renderizar o componente de filtros e carregar categorias', async () => {
    const { apiCategoriesGet } = await import('@/lib/api/generated/sdk.gen');
    await waitFor(() => {
      expect(screen.getByText(/distância/i)).toBeInTheDocument();
      expect(apiCategoriesGet).toHaveBeenCalledWith({ query: { activeOnly: true } });
    });
  });

  it('deve atualizar o rascunho do raio ao deslizar o slider', async () => {
    const slider = screen.getByRole('slider');
    fireEvent.change(slider, { target: { value: '75' } });
    expect(screen.getByText('75 km')).toBeInTheDocument();
  });

  it('deve atualizar o filtro de raio no mouseUp', async () => {
    const slider = screen.getByRole('slider');
    fireEvent.mouseUp(slider, { target: { value: '75' } });
    
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(expect.stringContaining('radiusInKm=75'), expect.anything());
    }, { timeout: 2000 });
  });

  it('deve atualizar o filtro de avaliação ao selecionar rádio', async () => {
    const radio = screen.getByLabelText(/4\+/);
    fireEvent.click(radio);
    
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(expect.stringContaining('minRating=4'), expect.anything());
    }, { timeout: 2000 });
  });

  it('deve limpar filtro de avaliação ao selecionar "Qualquer avaliação"', async () => {
    // Rerender with parameter to test clear
    mockUseSearchParams.mockReturnValue(new URLSearchParams('minRating=4'));
    cleanup();
    render(<SearchFilters />);
    
    const radio = screen.getByLabelText(/qualquer avaliação/i);
    fireEvent.click(radio);
    
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(expect.not.stringContaining('minRating'), expect.anything());
    }, { timeout: 1000 });
  });

  it('deve carregar e exibir categorias do SDK', async () => {
    const mockCats = [{ id: 'cat-1', name: 'Limpeza' }];
    vi.mocked(apiCategoriesGet).mockResolvedValue({ data: { data: mockCats } } as any);
    
    // Re-render to pick up new mock
    render(<SearchFilters />);
    
    await waitFor(() => {
      expect(screen.getByText('Limpeza')).toBeInTheDocument();
    });
  });

  it('deve atualizar o filtro de categoria ao selecionar', async () => {
    const mockCats = [{ id: 'cat-1', name: 'Limpeza' }];
    vi.mocked(apiCategoriesGet).mockResolvedValue({ data: { data: mockCats } } as any);
    render(<SearchFilters />);
    
    await waitFor(() => screen.getByText('Limpeza'));
    const radio = screen.getByLabelText('Limpeza');
    fireEvent.click(radio);
    
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(expect.stringContaining('categoryId=cat-1'), expect.anything());
    }, { timeout: 1000 });
  });
});
