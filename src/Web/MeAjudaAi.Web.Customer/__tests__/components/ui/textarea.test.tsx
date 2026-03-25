import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Textarea } from '@/components/ui/textarea';

describe('Textarea', () => {
  it('deve renderizar corretamente', () => {
    render(<Textarea />);
    expect(screen.getByRole('textbox')).toBeInTheDocument();
  });

  it('deve renderizar com placeholder', () => {
    render(<Textarea placeholder="Digite algo..." />);
    expect(screen.getByPlaceholderText('Digite algo...')).toBeInTheDocument();
  });

  it('deve renderizar com valor controlado', () => {
    render(<Textarea value="valor controlado" readOnly />);
    expect(screen.getByRole('textbox')).toHaveValue('valor controlado');
  });

  it('deve chamar onChange ao digitar', async () => {
    const handleChange = vi.fn();
    const user = userEvent.setup();

    render(<Textarea onChange={handleChange} />);
    await user.type(screen.getByRole('textbox'), 'test');

    expect(handleChange).toHaveBeenCalledTimes(4);
  });

  it('deve estar desabilitado quando disabled', () => {
    render(<Textarea disabled />);
    expect(screen.getByRole('textbox')).toBeDisabled();
  });
});
