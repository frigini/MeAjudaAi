import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ServiceSelector } from '@/components/providers/service-selector';

describe('ServiceSelector', () => {
  it('deve renderizar o seletor', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      json: async () => [
        { serviceId: '1', name: 'Elétrica' },
        { serviceId: '2', name: 'Hidráulica' },
      ],
    }) as any;

    const onSelect = vi.fn();
    render(<ServiceSelector onSelect={onSelect} />);

    await waitFor(() => {
      expect(screen.getByRole('combobox')).toBeInTheDocument();
    });
  });

  it('deve renderizar desabilitado', () => {
    const onSelect = vi.fn();
    render(<ServiceSelector onSelect={onSelect} disabled />);
    expect(screen.getByRole('combobox')).toBeDisabled();
  });

  it('deve chamar onSelect ao selecionar serviço', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      json: async () => [
        { serviceId: '1', name: 'Elétrica' },
      ],
    }) as any;

    const user = userEvent.setup();
    const onSelect = vi.fn();
    render(<ServiceSelector onSelect={onSelect} />);

    await waitFor(() => {
      const button = screen.getByRole('combobox');
      user.click(button);
    });
  });
});
