import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ReviewList } from '@/components/reviews/review-list';

describe('ReviewList', () => {
  it('deve renderizar corretamente', () => {
    render(<ReviewList providerId="provider-1" />);
    expect(screen.getByText(/ordenAR/i)).toBeInTheDocument();
  });

  it('deve renderizar botão carregar mais', () => {
    render(<ReviewList providerId="provider-1" />);
    expect(screen.getByRole('button', { name: /carregar mais/i })).toBeInTheDocument();
  });
});
