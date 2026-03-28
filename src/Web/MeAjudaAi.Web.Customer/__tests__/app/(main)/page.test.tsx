import { render, screen } from '@testing-library/react';
import HomePage from '@/app/(main)/page';
import { describe, it, expect, vi } from 'vitest';

// Mock components to avoid deep rendering issues
vi.mock('@/components/search/city-search', () => ({
  CitySearch: () => <div data-testid="city-search">City Search</div>
}));

vi.mock('@/components/home/how-it-works', () => ({
  HowItWorks: () => <div data-testid="how-it-works">How It Works</div>
}));

vi.mock('@/components/ui/ad-banner', () => ({
  AdBanner: () => <div data-testid="ad-banner">Ad Banner</div>
}));

// Mock Next.js Image to avoid lint/optimization warnings in tests
vi.mock('next/image', () => ({
  default: ({ alt, src }: { alt: string; src: string }) => (
    // eslint-disable-next-line @next/next/no-img-element
    <img alt={alt} src={src} />
  )
}));

describe('HomePage', () => {
  it('deve renderizar a landing page com todas as seções principais', () => {
    render(<HomePage />);

    expect(screen.getByText(/Conectando quem precisa com/i)).toBeInTheDocument();
    expect(screen.getByText(/quem sabe fazer/i)).toBeInTheDocument();
    expect(screen.getByTestId('city-search')).toBeInTheDocument();
    expect(screen.getByTestId('how-it-works')).toBeInTheDocument();
    expect(screen.getByTestId('ad-banner')).toBeInTheDocument();
    expect(screen.getByText(/Conheça o MeAjudaAí/i)).toBeInTheDocument();
    expect(screen.getByText(/Você é prestador de serviço\?/i)).toBeInTheDocument();
  });

  it('deve ter um link para cadastro de prestadores', () => {
    render(<HomePage />);
    const registerLink = screen.getByRole('link', { name: /cadastre-se grátis/i });
    expect(registerLink).toHaveAttribute('href', '/cadastro/prestador');
  });
});
