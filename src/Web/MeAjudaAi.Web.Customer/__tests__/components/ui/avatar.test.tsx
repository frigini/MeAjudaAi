import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Avatar } from '@/components/ui/avatar';

describe('Avatar Component', () => {
  it('deve renderizar com iniciais quando não há src', () => {
    render(<Avatar alt="João Silva" />);
    expect(screen.getByText('JS')).toBeInTheDocument();
  });

  it('deve renderizar com tamanho padrão (md)', () => {
    render(<Avatar alt="João Silva" />);
    const container = screen.getByText('JS').closest('div');
    expect(container).toHaveClass('size-10');
  });

  it('deve renderizar com tamanho small', () => {
    render(<Avatar alt="João Silva" size="sm" />);
    const container = screen.getByText('JS').closest('div');
    expect(container).toHaveClass('size-8');
  });

  it('deve renderizar com tamanho large', () => {
    render(<Avatar alt="Maria Santos" size="lg" />);
    const container = screen.getByText('MS').closest('div');
    expect(container).toHaveClass('size-12');
  });

  it('deve renderizar com tamanho extra large', () => {
    render(<Avatar alt="Teste User" size="xl" />);
    const container = screen.getByText('TU').closest('div');
    expect(container).toHaveClass('size-16');
  });

  it('deve usar fallback customizado', () => {
    render(<Avatar alt="João Silva" fallback="XX" />);
    expect(screen.getByText('XX')).toBeInTheDocument();
  });

  it('deve renderizar apenas uma iniciais quando há apenas um nome', () => {
    render(<Avatar alt="João" />);
    expect(screen.getByText('J')).toBeInTheDocument();
  });
});
