import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { ProfileStatusCard } from '@/components/dashboard/profile-status-card';

describe('ProfileStatusCard', () => {
  it('deve renderizar o título "Status do Perfil"', () => {
    render(<ProfileStatusCard />);
    expect(screen.getByText('Status do Perfil')).toBeInTheDocument();
  });

  it('deve exibir status ativo', () => {
    render(<ProfileStatusCard />);
    expect(screen.getByText('Ativo e visível')).toBeInTheDocument();
  });

  it('deve exibir botão de pausar visibilidade', () => {
    render(<ProfileStatusCard />);
    expect(screen.getByRole('button', { name: /pausar visibilidade/i })).toBeInTheDocument();
  });

  it('deve informar que o perfil está acessível nas buscas', () => {
    render(<ProfileStatusCard />);
    expect(screen.getByText(/acessível/i)).toBeInTheDocument();
  });
});
