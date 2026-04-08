import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AuthSelectionDropdown } from '@/components/auth/auth-selection-dropdown';
import userEvent from '@testing-library/user-event';

describe('AuthSelectionDropdown', () => {
  it('deve renderizar botão de cadastro', () => {
    render(<AuthSelectionDropdown />);
    expect(screen.getByRole('button', { name: /cadastre-se grátis/i })).toBeInTheDocument();
  });

  it('deve abrir menu ao clicar no botão', async () => {
    const user = userEvent.setup();
    render(<AuthSelectionDropdown />);
    
    await user.click(screen.getByRole('button', { name: /cadastre-se grátis/i }));
    
    expect(screen.getByText(/escolha a melhor opção/i)).toBeInTheDocument();
  });

  it('deve mostrar opções de cadastro', async () => {
    const user = userEvent.setup();
    render(<AuthSelectionDropdown />);
    
    await user.click(screen.getByRole('button', { name: /cadastre-se grátis/i }));
    
    expect(screen.getByText(/quero ser cliente/i)).toBeInTheDocument();
    expect(screen.getByText(/sou prestador/i)).toBeInTheDocument();
  });

  it('deve ter links corretos', async () => {
    const user = userEvent.setup();
    render(<AuthSelectionDropdown />);
    
    await user.click(screen.getByRole('button', { name: /cadastre-se grátis/i }));
    
    const clientLink = screen.getByRole('menuitem', { name: /quero ser cliente/i });
    const providerLink = screen.getByRole('menuitem', { name: /sou prestador/i });
    
    expect(clientLink).toHaveAttribute('href', '/cadastro/cliente');
    expect(providerLink).toHaveAttribute('href', '/cadastro/prestador');
  });
});
