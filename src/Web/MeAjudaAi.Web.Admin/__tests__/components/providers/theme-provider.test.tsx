import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, act } from 'test-support';
import { ThemeProvider, useTheme } from '@/components/providers/theme-provider';
import { useEffect } from 'react';

// Helper component to test useTheme hook
const ThemeTestComponent = () => {
  const { theme, setTheme, toggleTheme } = useTheme();
  return (
    <div>
      <span data-testid="current-theme">{theme}</span>
      <button onClick={() => setTheme('dark')} data-testid="set-dark">Set Dark</button>
      <button onClick={() => setTheme('light')} data-testid="set-light">Set Light</button>
      <button onClick={toggleTheme} data-testid="toggle-theme">Toggle</button>
    </div>
  );
};

describe('ThemeProvider (Admin)', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('dark');
    vi.clearAllMocks();
  });

  it('deve usar light como default se nada estiver no localStorage', () => {
    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    // Initial render before useEffect might be light
    // But once mounted, it should still be light if no preference
    expect(screen.getByTestId('current-theme')).toHaveTextContent('light');
  });

  it('deve carregar o tema do localStorage', async () => {
    localStorage.setItem('theme', 'dark');
    
    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('deve alternar o tema quando toggleTheme é chamado', () => {
    render(
      <ThemeProvider>
        <ThemeTestComponent />
      </ThemeProvider>
    );

    const toggleBtn = screen.getByTestId('toggle-theme');
    
    act(() => {
      toggleBtn.click();
    });

    expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
    expect(localStorage.getItem('theme')).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);

    act(() => {
      toggleBtn.click();
    });

    expect(screen.getByTestId('current-theme')).toHaveTextContent('light');
    expect(localStorage.getItem('theme')).toBe('light');
    expect(document.documentElement.classList.contains('dark')).toBe(false);
  });

  it('deve lançar erro se useTheme for usado fora do provider', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => render(<ThemeTestComponent />)).toThrow('useTheme must be used within a ThemeProvider');
    
    consoleSpy.mockRestore();
  });
});
