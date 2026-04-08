import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AdCard } from '@/components/search/ad-card';

describe('AdCard', () => {
  it('deve renderizar corretamente', () => {
    render(<AdCard />);
    expect(screen.getByText(/Publicidade/)).toBeInTheDocument();
  });

  it('deve renderizar texto do anuncio', () => {
    render(<AdCard />);
    expect(screen.getByText(/O SEU CLIENTE TAMBÉM VIU ESTE ANÚNCIO/)).toBeInTheDocument();
  });

  it('deve renderizar link para anuncie', () => {
    render(<AdCard />);
    expect(screen.getByRole('link', { name: /anuncie aqui/i })).toBeInTheDocument();
  });
});
