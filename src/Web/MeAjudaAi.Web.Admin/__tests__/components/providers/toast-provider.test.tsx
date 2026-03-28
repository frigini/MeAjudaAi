import { describe, it, expect } from 'vitest';
import { render } from 'test-support';
import { Toaster } from '@/components/providers/toast-provider';

describe('Toaster (Admin)', () => {
  it('deve renderizar sem erros', () => {
    const { container } = render(<Toaster />);
    expect(container).toBeInTheDocument();
  });
});
