import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Rating } from '@/components/ui/rating';

describe('Rating Component', () => {
  it('deve renderizar com valor padrão', () => {
    const { container } = render(<Rating value={3} />);
    const stars = container.querySelectorAll('[data-testid="rating-star"]');
    expect(stars).toHaveLength(5);
  });

  it('deve renderizar com max de 10 estrelas', () => {
    const { container } = render(<Rating value={5} max={10} />);
    const stars = container.querySelectorAll('[data-testid="rating-star"]');
    expect(stars).toHaveLength(10);
  });

  it('deve renderizar no modo readonly', () => {
    render(<Rating value={4} readOnly />);
    expect(screen.getByRole('img')).toBeInTheDocument();
  });

  it('deve chamar onChange ao clicar', async () => {
    const handleChange = vi.fn();
    const user = userEvent.setup();

    const { container } = render(<Rating value={0} onChange={handleChange} />);
    
    const stars = container.querySelectorAll('[data-testid="rating-star"]');
    await user.click(stars[2]);

    expect(handleChange).toHaveBeenCalledWith(3);
  });

  it('deve renderizar com tamanho small', () => {
    const { container } = render(<Rating value={3} size="sm" />);
    const star = container.querySelector('svg');
    expect(star).toHaveClass('w-3', 'h-3');
  });

  it('deve renderizar com tamanho large', () => {
    const { container } = render(<Rating value={3} size="lg" />);
    const star = container.querySelector('svg');
    expect(star).toHaveClass('w-6', 'h-6');
  });
});
