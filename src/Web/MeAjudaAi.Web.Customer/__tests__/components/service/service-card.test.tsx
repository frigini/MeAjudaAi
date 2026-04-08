import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ServiceCard } from '@/components/service/service-card';

const mockService = {
  id: 'service-1',
  name: 'João Silva',
  avatarUrl: '/avatar.jpg',
  description: 'Prestador de serviços de elétrica e hidráulica',
  services: ['Elétrica', 'Hidráulica'],
  rating: 4.5,
  reviewCount: 10,
};

describe('ServiceCard', () => {
  it('deve renderizar nome do prestador', () => {
    render(<ServiceCard {...mockService} />);
    expect(screen.getByText('João Silva')).toBeInTheDocument();
  });

  it('deve renderizar descrição', () => {
    render(<ServiceCard {...mockService} />);
    expect(screen.getByText('Prestador de serviços de elétrica e hidráulica')).toBeInTheDocument();
  });

  it('deve renderizar serviços', () => {
    render(<ServiceCard {...mockService} />);
    expect(screen.getByText('Elétrica')).toBeInTheDocument();
    expect(screen.getByText('Hidráulica')).toBeInTheDocument();
  });

  it('deve renderizar avaliação', () => {
    render(<ServiceCard {...mockService} />);
    expect(screen.getByText('10 comentários')).toBeInTheDocument();
  });

  it('deve renderizar avaliação singular', () => {
    render(<ServiceCard {...mockService} reviewCount={1} />);
    expect(screen.getByText('1 comentário')).toBeInTheDocument();
  });

  it('deve renderizar link para perfil', () => {
    render(<ServiceCard {...mockService} />);
    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/prestador/service-1');
  });

  it('deve renderizar sem avatar', () => {
    const { avatarUrl, ...mockWithoutAvatar } = mockService;
    render(<ServiceCard {...mockWithoutAvatar} />);
    expect(screen.getByText('JO')).toBeInTheDocument();
  });
});
