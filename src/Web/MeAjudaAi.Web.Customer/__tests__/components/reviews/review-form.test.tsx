import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ReviewForm } from '@/components/reviews/review-form';

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('ReviewForm', () => {
  it('deve renderizar título do formulário', () => {
    render(<ReviewForm providerId="provider-1" />);
    expect(screen.getByText(/avaliar este prestador/i)).toBeInTheDocument();
  });

  it('deve renderizar campos do formulário', () => {
    render(<ReviewForm providerId="provider-1" />);
    expect(screen.getByText(/sua nota/i)).toBeInTheDocument();
    expect(screen.getByText(/seu comentário/i)).toBeInTheDocument();
  });

  it('deve renderizar botão de enviar', () => {
    render(<ReviewForm providerId="provider-1" />);
    expect(screen.getByRole('button', { name: /enviar/i })).toBeInTheDocument();
  });

  it('deve renderizar placeholder do textarea', () => {
    render(<ReviewForm providerId="provider-1" />);
    expect(screen.getByPlaceholderText(/conte como foi sua experiência/i)).toBeInTheDocument();
  });

  it('deve renderizar com className customizada', () => {
    render(<ReviewForm providerId="provider-1" className="custom-class" />);
    expect(screen.getByText(/avaliar este prestador/i)).toBeInTheDocument();
  });
});
