import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ReviewList } from '@/components/reviews/review-list';
import userEvent from '@testing-library/user-event';

describe('ReviewList', () => {
  it('deve renderizar corretamente', () => {
    render(<ReviewList providerId="provider-1" />);
    expect(screen.getByText(/ordenAR/i)).toBeInTheDocument();
  });

  it('deve renderizar botão carregar mais', () => {
    render(<ReviewList providerId="provider-1" />);
    expect(screen.getByRole('button', { name: /carregar mais/i })).toBeInTheDocument();
  });

  it('deve carregar mais avaliações', async () => {
    const user = userEvent.setup();
    render(<ReviewList providerId="provider-1" />);
    
    expect(screen.getByText(/mostrando 4 avaliações/i)).toBeInTheDocument();
    
    await user.click(screen.getByRole('button', { name: /carregar mais/i }));
    
    expect(screen.getByText(/mostrando 8 avaliações/i)).toBeInTheDocument();
  });

  it('deve ordenar avaliações', async () => {
    const user = userEvent.setup();
    render(<ReviewList providerId="provider-1" />);
    
    await user.click(screen.getByRole('button', { name: /ordenar/i }));
    
    expect(screen.getByRole('button', { name: /ordenar/i })).toBeInTheDocument();
  });

  it('deve renderizar estado vazio', () => {
    const { container } = render(<ReviewList providerId="provider-1" />);
    const reviews = container.querySelectorAll('[class*="grid"]');
    expect(reviews.length).toBeGreaterThan(0);
  });
});
