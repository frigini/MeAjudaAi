import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Input } from '@/components/ui/input';

describe('Input Component', () => {
  it('deve renderizar corretamente', () => {
    render(<Input />);
    expect(screen.getByRole('textbox')).toBeInTheDocument();
  });

  it('deve renderizar com label', () => {
    render(<Input label="Email" />);
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
  });

  it('deve renderizar com placeholder', () => {
    render(<Input placeholder="Digite seu email" />);
    expect(screen.getByPlaceholderText(/digite seu email/i)).toBeInTheDocument();
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(<Input disabled />);
    expect(screen.getByRole('textbox')).toBeDisabled();
  });

  it('deve mostrar mensagem de erro', () => {
    render(<Input error="Campo obrigatório" />);
    expect(screen.getByText(/campo obrigatório/i)).toBeInTheDocument();
  });

  it('deve chamar onChange quando modificado', async () => {
    const handleChange = vi.fn();
    const user = userEvent.setup();

    render(<Input onChange={handleChange} />);
    await user.type(screen.getByRole('textbox'), 'test');

    expect(handleChange).toHaveBeenCalled();
  });

  it('deve aceitar valor controlado', () => {
    render(<Input value="valor controlado" readOnly />);
    expect(screen.getByRole('textbox')).toHaveValue('valor controlado');
  });

  it('deve renderizar com variant error', () => {
    render(<Input error="Erro" />);
    const input = screen.getByRole('textbox');
    expect(input).toHaveClass('border-destructive');
  });
});
