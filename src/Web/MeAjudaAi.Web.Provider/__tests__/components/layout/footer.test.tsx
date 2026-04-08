import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { Footer } from '@/components/layout/footer';

describe('Footer (Provider)', () => {
  it('deve renderizar seções de Missão, Visão e Valores', () => {
    render(<Footer />);
    expect(screen.getByText('Missão')).toBeInTheDocument();
    expect(screen.getByText('Visão')).toBeInTheDocument();
    expect(screen.getByText('Valores')).toBeInTheDocument();
  });

  it('deve exibir informações de contato', () => {
    render(<Footer />);
    expect(screen.getByText('contato@ajudaai.com')).toBeInTheDocument();
    expect(screen.getByText('(11) 99999-9999')).toBeInTheDocument();
    expect(screen.getByText('@ajudaai')).toBeInTheDocument();
  });

  it('deve exibir links com hrefs corretos', () => {
    render(<Footer />);
    expect(screen.getByText('contato@ajudaai.com').closest('a')).toHaveAttribute('href', 'mailto:contato@ajudaai.com');
    expect(screen.getByText('(11) 99999-9999').closest('a')).toHaveAttribute('href', 'tel:+5511999999999');
  });

  it('deve exibir o ano atual no copyright', () => {
    render(<Footer />);
    const currentYear = new Date().getFullYear().toString();
    expect(screen.getByText(new RegExp(currentYear))).toBeInTheDocument();
  });
});
