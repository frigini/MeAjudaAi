import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Footer } from '@/components/layout/footer';

describe('Footer', () => {
  it('deve renderizar missão', () => {
    render(<Footer />);
    expect(screen.getByText('Missão')).toBeInTheDocument();
  });

  it('deve renderizar visão', () => {
    render(<Footer />);
    expect(screen.getByText('Visão')).toBeInTheDocument();
  });

  it('deve renderizar valores', () => {
    render(<Footer />);
    expect(screen.getByText('Valores')).toBeInTheDocument();
  });

  it('deve renderizar contatos', () => {
    render(<Footer />);
    expect(screen.getByText('Contatos')).toBeInTheDocument();
  });

  it('deve renderizar email de contato', () => {
    render(<Footer />);
    expect(screen.getByText('contato@ajudaai.com')).toBeInTheDocument();
  });

  it('deve renderizar copyright', () => {
    render(<Footer />);
    expect(screen.getByText(new RegExp(`© ${new Date().getFullYear()}`))).toBeInTheDocument();
  });
});
