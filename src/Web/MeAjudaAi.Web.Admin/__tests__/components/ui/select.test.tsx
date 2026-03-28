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

  it('deve chamar onValueChange ao escolher uma opção', async () => {
    const user = userEvent.setup();
    const onValueChange = vi.fn();
    render(
      <Select onValueChange={onValueChange} placeholder="Selecione">
        <SelectItem value="1">Opção 1</SelectItem>
        <SelectItem value="2">Opção 2</SelectItem>
      </Select>
    );

    await user.click(screen.getByText('Selecione'));
    const option2 = await screen.findByText('Opção 2');
    await user.click(option2);

    expect(onValueChange).toHaveBeenCalledWith('2');
  });

  it('SelectItem deve aceitar className customizado', async () => {
    const user = userEvent.setup();
    render(
      <Select placeholder="Selecione">
        <SelectItem value="1" className="custom-item">Opção 1</SelectItem>
      </Select>
    );

    await user.click(screen.getByText('Selecione'));
    const item = await screen.findByText('Opção 1');
    expect(item.closest('div')).toHaveClass('custom-item');
  });
});
