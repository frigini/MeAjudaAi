import { render, screen, fireEvent } from '@testing-library/react';
import { HowItWorks } from '@/components/home/how-it-works';
import { describe, it, expect, vi } from 'vitest';

// Mock Next.js Image to avoid lint/optimization warnings in tests
vi.mock('next/image', () => ({
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  default: ({ alt, src }: any) => (
    // eslint-disable-next-line @next/next/no-img-element
    <img alt={alt} src={src} />
  )
}));

describe('HowItWorks', () => {
  it('deve renderizar o título e estar inicialmente fechado', () => {
    render(<HowItWorks />);
    expect(screen.getByText(/como funciona\?/i)).toBeInTheDocument();
    
    // As seções de passos devem estar escondidas (max-h-0)
    const button = screen.getByRole('button', { name: /como funciona\?/i });
    expect(button).toHaveAttribute('aria-expanded', 'false');
  });

  it('deve expandir ao clicar no botão de interrogação', () => {
    render(<HowItWorks />);
    const button = screen.getByRole('button', { name: /como funciona\?/i });
    
    fireEvent.click(button);
    expect(button).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText(/1 - Faça seu cadastro/i)).toBeInTheDocument();
  });

  it('deve fechar ao clicar no botão esconder', () => {
    render(<HowItWorks />);
    const toggleButton = screen.getByRole('button', { name: /como funciona\?/i });
    
    // Abrir
    fireEvent.click(toggleButton);
    const hideButton = screen.getByRole('button', { name: /esconder/i });
    expect(hideButton).toBeInTheDocument();
    
    // Fechar
    fireEvent.click(hideButton);
    expect(toggleButton).toHaveAttribute('aria-expanded', 'false');
  });
});
