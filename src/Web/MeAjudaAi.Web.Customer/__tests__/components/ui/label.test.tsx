import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Label } from '@/components/ui/label';

describe('Label', () => {
  it('deve renderizar corretamente', () => {
    render(<Label>Label Text</Label>);
    expect(screen.getByText('Label Text')).toBeInTheDocument();
  });

  it('deve renderizar como htmlFor associado a input', () => {
    render(<Label htmlFor="email">Email</Label>);
    const label = screen.getByText('Email');
    expect(label).toHaveAttribute('for', 'email');
  });
});
