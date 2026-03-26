import { render, screen, fireEvent } from '@testing-library/react';
import { CitySearch } from '@/components/search/city-search';
import { useRouter } from 'next/navigation';
import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('next/navigation', () => ({
  useRouter: vi.fn(),
}));

describe('CitySearch', () => {
  const mockPush = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useRouter).mockReturnValue({ push: mockPush } as any);
  });

  it('deve renderizar o seletor de cidades', () => {
    render(<CitySearch />);
    expect(screen.getByLabelText(/selecionar cidade/i)).toBeInTheDocument();
  });

  it('deve atualizar o estado ao selecionar uma cidade', () => {
    render(<CitySearch />);
    const select = screen.getByLabelText(/selecionar cidade/i);
    fireEvent.change(select, { target: { value: 'muriae-mg' } });
    expect(select).toHaveValue('muriae-mg');
  });

  it('deve navegar para a página de busca ao clicar no botão', () => {
    render(<CitySearch />);
    const select = screen.getByLabelText(/selecionar cidade/i);
    const button = screen.getByLabelText(/buscar prestadores/i);
    
    expect(button).toBeDisabled();
    
    fireEvent.change(select, { target: { value: 'uba-mg' } });
    expect(button).not.toBeDisabled();
    
    fireEvent.click(button);
    expect(mockPush).toHaveBeenCalledWith('/buscar?cidade=uba-mg');
  });
});
