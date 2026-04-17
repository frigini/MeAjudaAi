import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import Loading from '@/app/(main)/prestador/[id]/loading';

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key === 'provider.loadingProfile' ? 'Carregando perfil do prestador...' : key,
  }),
}));

describe('Provider Profile Loading Component', () => {
  it('renders loading status correctly', () => {
    render(<Loading />);
    
    // Check if role="status" exists (it replaces aria-live="polite")
    const container = screen.getByRole('status');
    expect(container).toBeInTheDocument();
    
    // Check if screen reader text is present
    expect(screen.getByText('Carregando perfil do prestador...')).toBeInTheDocument();
  });

  it('contains skeletons', () => {
    const { container } = render(<Loading />);
    
    // Skeletons have data-slot="skeleton"
    const skeletons = container.querySelectorAll('[data-slot="skeleton"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });
});
