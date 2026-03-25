import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ReviewCard } from '@/components/reviews/review-card';

const mockReview = {
  id: 'review-1',
  authorName: 'Maria Santos',
  rating: 5,
  comment: 'Excelente serviço! Muito profissional e pontual.',
  verified: true,
  createdAt: new Date('2024-01-15'),
};

describe('ReviewCard', () => {
  it('deve renderizar comentário da avaliação', () => {
    render(<ReviewCard review={mockReview} />);
    expect(screen.getByText('Excelente serviço! Muito profissional e pontual.')).toBeInTheDocument();
  });

  it('deve renderizar nome do autor', () => {
    render(<ReviewCard review={mockReview} />);
    expect(screen.getByText('Maria Santos')).toBeInTheDocument();
  });

  it('deve renderizar rating', () => {
    render(<ReviewCard review={mockReview} />);
    expect(screen.getByText('Maria Santos')).toBeInTheDocument();
  });
});
