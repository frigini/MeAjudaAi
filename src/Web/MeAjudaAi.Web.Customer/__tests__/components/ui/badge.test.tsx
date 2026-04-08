import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Badge } from '@/components/ui/badge';

describe('Badge Component', () => {
  it('deve renderizar corretamente', () => {
    render(<Badge>Badge</Badge>);
    expect(screen.getByText('Badge')).toBeInTheDocument();
  });

  it('deve renderizar com variant secondary por padrão', () => {
    render(<Badge>Default</Badge>);
    const badge = screen.getByText('Default');
    expect(badge).toHaveClass('bg-secondary/10');
  });

  it('deve renderizar com variant primary', () => {
    render(<Badge variant="primary">Primary</Badge>);
    const badge = screen.getByText('Primary');
    expect(badge).toHaveClass('bg-primary/10');
  });

  it('deve renderizar com variant success', () => {
    render(<Badge variant="success">Success</Badge>);
    const badge = screen.getByText('Success');
    expect(badge).toHaveClass('bg-green-50');
  });

  it('deve renderizar com variant warning', () => {
    render(<Badge variant="warning">Warning</Badge>);
    const badge = screen.getByText('Warning');
    expect(badge).toHaveClass('bg-yellow-50');
  });

  it('deve renderizar com variant destructive', () => {
    render(<Badge variant="destructive">Destructive</Badge>);
    const badge = screen.getByText('Destructive');
    expect(badge).toHaveClass('bg-red-50');
  });
});
