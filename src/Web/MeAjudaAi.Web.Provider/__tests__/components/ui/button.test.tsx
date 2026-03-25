import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import userEvent from '@testing-library/user-event';
import { Button } from '@/components/ui/button';

describe('Button (Provider)', () => {
  it('deve renderizar com texto', () => {
    render(<Button>Ação</Button>);
    expect(screen.getByRole('button', { name: /ação/i })).toBeInTheDocument();
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(<Button disabled>Disabled</Button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('deve chamar onClick ao clicar', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();
    render(<Button onClick={handleClick}>Click</Button>);
    await user.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('deve renderizar variante secondary', () => {
    render(<Button variant="secondary">Secundário</Button>);
    // tv() expands to actual CSS classes — verify renders correctly
    expect(screen.getByRole('button')).toBeInTheDocument();
    expect(screen.getByRole('button')).toHaveAttribute('data-slot', 'button');
  });

  it('deve renderizar como slot (asChild)', () => {
    render(<Button asChild><a href="/test">Link</a></Button>);
    expect(screen.getByRole('link', { name: /link/i })).toBeInTheDocument();
  });
});
