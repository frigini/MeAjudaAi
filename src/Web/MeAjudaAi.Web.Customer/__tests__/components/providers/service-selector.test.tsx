import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ServiceSelector } from '@/components/providers/service-selector';

describe('ServiceSelector', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(global, 'fetch');
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('deve renderizar o seletor', async () => {
    fetchSpy.mockResolvedValueOnce({
      ok: true,
      json: async () => [
        { serviceId: '1', name: 'Elétrica' },
        { serviceId: '2', name: 'Hidráulica' },
      ],
    });

    const onSelect = vi.fn();
    render(<ServiceSelector onSelect={onSelect} />);

    await waitFor(() => {
      expect(screen.getByRole('combobox')).toBeInTheDocument();
    });
  });

  it('deve renderizar desabilitado', () => {
    fetchSpy.mockResolvedValueOnce({
      ok: true,
      json: async () => [],
    });
    const onSelect = vi.fn();
    render(<ServiceSelector onSelect={onSelect} disabled />);
    expect(screen.getByRole('combobox')).toBeDisabled();
  });

  it('deve chamar onSelect ao selecionar serviço', async () => {
    fetchSpy.mockResolvedValueOnce({
      ok: true,
      json: async () => [
        { serviceId: '1', name: 'Elétrica' },
      ],
    });

    const user = userEvent.setup();
    const onSelect = vi.fn();
    render(<ServiceSelector onSelect={onSelect} />);

    await waitFor(() => {
      expect(screen.getByRole('combobox')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('combobox'));

    await waitFor(() => {
      expect(screen.getByText('Elétrica')).toBeInTheDocument();
    });

    await user.click(screen.getByText('Elétrica'));

    expect(onSelect).toHaveBeenCalledWith('1');
  });
});
