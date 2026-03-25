import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { HowItWorks } from '@/components/home/how-it-works';

vi.mock('next/image', () => ({
  default: ({ src, alt, ...props }: { src: string; alt: string }) => (
    <img src={src} alt={alt} {...props} />
  ),
}));

describe('HowItWorks', () => {
  it('deve renderizar o título', () => {
    render(<HowItWorks />);
    expect(screen.getByText(/como funciona\?/i)).toBeInTheDocument();
  });

  it('deve renderizar o texto de descrição', () => {
    render(<HowItWorks />);
    expect(screen.getByText(/expanda esse menu para conhecer o processo/i)).toBeInTheDocument();
  });

  it('deve renderizar botão de toggle', () => {
    render(<HowItWorks />);
    expect(screen.getByRole('button', { name: /como funciona\?/i })).toBeInTheDocument();
  });

  it('deve ter atributo aria-expanded inicial como false', () => {
    render(<HowItWorks />);
    const button = screen.getByRole('button', { name: /como funciona\?/i });
    expect(button).toHaveAttribute('aria-expanded', 'false');
  });
});
