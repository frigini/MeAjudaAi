import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen } from 'test-support';
import userEvent from '@testing-library/user-event';
import { ThemeToggle } from '@/components/ui/theme-toggle';

const { toggleTheme } = vi.hoisted(() => ({
  toggleTheme: vi.fn(),
}));

vi.mock('@/components/providers/theme-provider', () => ({
  useTheme: () => ({ theme: 'light', toggleTheme }),
}));

describe('ThemeToggle', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });
  it('deve renderizar botão de toggle de tema', () => {
    render(<ThemeToggle />);
    expect(screen.getByRole('button')).toBeInTheDocument();
  });

  it('deve ter aria-label descritivo no modo light', () => {
    render(<ThemeToggle />);
    expect(screen.getByRole('button')).toHaveAttribute('aria-label', 'Switch to dark mode');
  });

  it('deve ter aria-pressed=false no modo light', () => {
    render(<ThemeToggle />);
    expect(screen.getByRole('button')).toHaveAttribute('aria-pressed', 'false');
  });

  it('deve chamar toggleTheme ao clicar', async () => {
    const user = userEvent.setup();
    render(<ThemeToggle />);
    const button = screen.getByRole('button');
    await user.click(button);
    expect(toggleTheme).toHaveBeenCalled();
  });
});

