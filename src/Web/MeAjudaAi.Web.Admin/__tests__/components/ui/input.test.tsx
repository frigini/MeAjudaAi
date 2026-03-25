import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import userEvent from '@testing-library/user-event';
import { Input } from '@/components/ui/input';

describe('Input (Admin)', () => {
  it('deve renderizar campo de input', () => {
    render(<Input placeholder="Digite aqui" />);
    expect(screen.getByPlaceholderText('Digite aqui')).toBeInTheDocument();
  });

  it('deve aceitar digitação', async () => {
    const user = userEvent.setup();
    render(<Input placeholder="Digite" />);
    const input = screen.getByPlaceholderText('Digite');
    await user.type(input, 'teste');
    expect(input).toHaveValue('teste');
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(<Input disabled placeholder="Desabilitado" />);
    expect(screen.getByPlaceholderText('Desabilitado')).toBeDisabled();
  });
});
