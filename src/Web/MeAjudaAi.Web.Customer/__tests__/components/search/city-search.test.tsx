import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { CitySearch } from '@/components/search/city-search';
import userEvent from '@testing-library/user-event';
import * as nextNavigation from 'next/navigation';

vi.mock('next/navigation', () => ({
  useRouter: vi.fn(() => ({
    push: vi.fn(),
  })),
}));

describe('CitySearch', () => {
  it('deve renderizar o componente', () => {
    render(<CitySearch />);
    expect(screen.getByRole('combobox')).toBeInTheDocument();
  });

  it('deve renderizar opções de cidade', () => {
    render(<CitySearch />);
    expect(screen.getByText('Muriaé - MG')).toBeInTheDocument();
    expect(screen.getByText('Ubá - MG')).toBeInTheDocument();
    expect(screen.getByText('Cataguases - MG')).toBeInTheDocument();
  });

  it('deve desabilitar botão quando nenhuma cidade selecionada', () => {
    render(<CitySearch />);
    const button = screen.getByRole('button', { name: /buscar prestadores/i });
    expect(button).toBeDisabled();
  });

  it('deve habilitar botão quando cidade selecionada', async () => {
    const user = userEvent.setup();
    render(<CitySearch />);
    
    const select = screen.getByRole('combobox');
    await user.selectOptions(select, 'muriae-mg');
    
    const button = screen.getByRole('button', { name: /buscar prestadores/i });
    expect(button).toBeEnabled();
  });

  it('deve navegar para página de busca ao clicar no botão', async () => {
    const mockPush = vi.fn();
    vi.mocked(nextNavigation.useRouter).mockReturnValue({ push: mockPush } as any);
    
    const user = userEvent.setup();
    render(<CitySearch />);
    
    const select = screen.getByRole('combobox');
    await user.selectOptions(select, 'muriae-mg');
    
    const button = screen.getByRole('button', { name: /buscar prestadores/i });
    await user.click(button);
    
    expect(mockPush).toHaveBeenCalledWith('/buscar?cidade=muriae-mg');
  });
});
