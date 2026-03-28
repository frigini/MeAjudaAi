import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import userEvent from '@testing-library/user-event';
import { Select, SelectItem } from '@/components/ui/select';

describe('Select (Admin)', () => {
  it('deve renderizar com placeholder', () => {
    render(
      <Select placeholder="Selecione">
        <SelectItem value="1">Opção 1</SelectItem>
      </Select>
    );
    expect(screen.getByText('Selecione')).toBeInTheDocument();
  });

  it('deve aceitar value controlado', () => {
    render(
      <Select value="1" placeholder="Selecione">
        <SelectItem value="1">Opção 1</SelectItem>
      </Select>
    );
    expect(screen.getByText('Opção 1')).toBeInTheDocument();
  });

  it('deve aceitar defaultValue não controlado', () => {
    render(
      <Select defaultValue="1" placeholder="Selecione">
        <SelectItem value="1">Opção 1</SelectItem>
      </Select>
    );
    expect(screen.getByText('Opção 1')).toBeInTheDocument();
  });

  it('deve estar desabilitado quando disabled=true', () => {
    render(
      <Select disabled placeholder="Selecione">
        <SelectItem value="1">Opção 1</SelectItem>
      </Select>
    );
    const trigger = screen.getByText('Selecione').closest('button');
    expect(trigger).toBeDisabled();
  });

  it('deve aceitar className customizado no Select', () => {
    render(
      <Select className="custom-select" placeholder="Selecione">
        <SelectItem value="1">Opção 1</SelectItem>
      </Select>
    );
    const trigger = screen.getByText('Selecione').closest('button');
    expect(trigger).toHaveClass('custom-select');
  });

  it('deve chamar onValueChange ao selecionar opção', async () => {
    const handleChange = vi.fn();
    const user = userEvent.setup();
    
    // Stub PointerEvent with proper implementation
    class MockPointerEvent {
      hasPointerCapture = vi.fn(() => false);
      releasePointerCapture = vi.fn();
    }
    vi.stubGlobal('PointerEvent', MockPointerEvent);
    
    render(
      <Select placeholder="Selecione" onValueChange={handleChange}>
        <SelectItem value="1">Opção 1</SelectItem>
      </Select>
    );
    const trigger = screen.getByText('Selecione').closest('button');
    await user.click(trigger!);
    await user.click(screen.getByText('Opção 1'));
    expect(handleChange).toHaveBeenCalledWith('1');
    expect(handleChange).toHaveBeenCalledTimes(1);
  });
});
