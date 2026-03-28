import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import userEvent from '@testing-library/user-event';
import { Button } from '@/components/ui/button';

describe('Button (Admin)', () => {
  it('deve renderizar com texto', () => {
    render(<Button>Clique</Button>);
    expect(screen.getByRole('button', { name: /clique/i })).toBeInTheDocument();
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(<Button disabled>Disabled</Button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('deve chamar onClick quando clicado', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();
    render(<Button onClick={handleClick}>Click</Button>);
    await user.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('não deve chamar onClick quando desabilitado', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();
    render(<Button onClick={handleClick} disabled>Click</Button>);
    await user.click(screen.getByRole('button'));
    expect(handleClick).not.toHaveBeenCalled();
  });

  it.each`
    name                | props                              | dataSlot | expectedClasses
    ${'destructive'}   | ${{ variant: 'destructive' }}    | ${true}  | ${'bg-destructive'}
    ${'ghost'}          | ${{ variant: 'ghost' }}          | ${true}  | ${'bg-transparent'}
    ${'sm size'}        | ${{ size: 'sm' }}                | ${true}  | ${'h-9'}
  `('deve renderizar $name com estilo correto', ({ props, dataSlot, expectedClasses }) => {
    render(<Button {...props}>Test</Button>);
    const button = screen.getByRole('button');
    if (dataSlot) {
      expect(button).toHaveAttribute('data-slot', 'button');
    }
    expect(button).toHaveClass(expectedClasses);
  });

  it('deve aceitar props adicionais', () => {
    render(<Button id="test-button">Props</Button>);
    expect(screen.getByRole('button')).toHaveAttribute('id', 'test-button');
  });
});
