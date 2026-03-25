import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { UserMenu } from '@/components/layout/user-menu';
import userEvent from '@testing-library/user-event';

const mockUseSession = vi.fn();
const mockUseProviderStatus = vi.fn();

vi.mock('next-auth/react', () => ({
  useSession: () => mockUseSession(),
  signOut: vi.fn(() => Promise.resolve()),
}));

vi.mock('@/hooks/use-provider-status', () => ({
  useProviderStatus: () => mockUseProviderStatus(),
}));

vi.mock('@/components/auth/auth-selection-dropdown', () => ({
  AuthSelectionDropdown: () => <div data-testid="auth-dropdown">Auth Dropdown</div>,
}));

describe('UserMenu', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar estado de loading', () => {
    mockUseSession.mockReturnValue({ data: null, status: 'loading' });
    mockUseProviderStatus.mockReturnValue({ data: null, isLoading: false, isError: false });
    render(<UserMenu />);
    expect(screen.getByTestId('user-menu-loading')).toBeInTheDocument();
  });

  it('deve renderizar opções de login para usuário não autenticado', () => {
    mockUseSession.mockReturnValue({ data: null, status: 'unauthenticated' });
    mockUseProviderStatus.mockReturnValue({ data: null, isLoading: false, isError: false });
    
    render(<UserMenu />);
    expect(screen.getByTestId('auth-dropdown')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /login/i })).toBeInTheDocument();
  });

  it('deve renderizar menu dropdown para usuário autenticado', async () => {
    const user = userEvent.setup();
    mockUseSession.mockReturnValue({
      data: {
        user: {
          name: 'João Silva',
          email: 'joao@test.com',
          image: 'https://example.com/avatar.jpg',
        },
        accessToken: 'mock-token',
      },
      status: 'authenticated',
    });
    mockUseProviderStatus.mockReturnValue({ data: null, isLoading: false, isError: false });

    render(<UserMenu />);
    
    await user.click(screen.getByRole('button', { name: /joão silva/i }));
    
    await waitFor(() => {
      expect(screen.getByText(/meu perfil/i)).toBeInTheDocument();
    });
  });
});
