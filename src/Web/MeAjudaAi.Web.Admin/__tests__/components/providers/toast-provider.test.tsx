import { describe, it, expect, vi } from 'vitest';
import { render } from 'test-support';
import { Toaster } from '@/components/providers/toast-provider';

vi.mock('sonner', () => ({
  Toaster: (props: Record<string, unknown>) => <div data-testid="sonner-toaster" data-position={props.position as string} />
}));

describe('Toaster (Admin)', () => {
  it('deve renderizar com a posição correta', () => {
    const { getByTestId } = render(<Toaster />);
    const toaster = getByTestId('sonner-toaster');
    expect(toaster).toBeInTheDocument();
    expect(toaster).toHaveAttribute('data-position', 'top-right');
  });
});
