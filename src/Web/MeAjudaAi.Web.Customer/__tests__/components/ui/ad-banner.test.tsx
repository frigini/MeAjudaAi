import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AdBanner } from '@/components/ui/ad-banner';

describe('AdBanner', () => {
  it('deve renderizar corretamente', () => {
    render(<AdBanner />);
    expect(screen.getByText(/anuncie aqui/i)).toBeInTheDocument();
  });
});
