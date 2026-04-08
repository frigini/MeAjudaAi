import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { ProfileServices } from '@/components/profile/profile-services';

const mockServices = ['Limpeza Residencial', 'Passar Roupa', 'Cozinha Básica'];

describe('ProfileServices', () => {
  it('deve renderizar o título da seção', () => {
    render(<ProfileServices services={[]} />);
    expect(screen.getByText(/Meus serviços/i)).toBeInTheDocument();
  });

  it('deve renderizar a lista de serviços', () => {
    render(<ProfileServices services={mockServices} />);
    expect(screen.getByText('Limpeza Residencial')).toBeInTheDocument();
    expect(screen.getByText('Passar Roupa')).toBeInTheDocument();
    expect(screen.getByText('Cozinha Básica')).toBeInTheDocument();
  });

  it('deve exibir o input para adicionar novo serviço', () => {
    render(<ProfileServices services={[]} />);
    expect(screen.getByPlaceholderText(/Digite um novo serviço aqui/i)).toBeInTheDocument();
  });

  it('deve exibir o botão de adicionar', () => {
    render(<ProfileServices services={[]} />);
    expect(screen.getByRole('button', { name: /Adicionar serviço/i })).toBeInTheDocument();
  });
});
