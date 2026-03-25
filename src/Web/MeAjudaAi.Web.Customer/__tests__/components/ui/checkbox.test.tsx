import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Checkbox } from '@/components/ui/checkbox';

describe('Checkbox', () => {
  it('deve renderizar corretamente', () => {
    render(<Checkbox />);
    expect(screen.getByRole('checkbox')).toBeInTheDocument();
  });

  it('deve estar unchecked por padrão', () => {
    render(<Checkbox />);
    expect(screen.getByRole('checkbox')).not.toBeChecked();
  });

  it('deve estar checked quando prop passed', () => {
    render(<Checkbox checked />);
    expect(screen.getByRole('checkbox')).toBeChecked();
  });

  it('deve chamar onCheckedChange ao clicar', async () => {
    const handleChange = vi.fn();
    const user = userEvent.setup();

    render(<Checkbox onCheckedChange={handleChange} />);
    await user.click(screen.getByRole('checkbox'));

    expect(handleChange).toHaveBeenCalledWith(true);
  });

  it('deve estar desabilitado quando disabled', () => {
    render(<Checkbox disabled />);
    expect(screen.getByRole('checkbox')).toBeDisabled();
  });
});
