import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/card';

describe('Card Component', () => {
  it('deve renderizar corretamente', () => {
    render(<Card>Card Content</Card>);
    expect(screen.getByText('Card Content')).toBeInTheDocument();
  });

  it('deve renderizar com padding padrão md', () => {
    render(<Card>Content</Card>);
    const card = screen.getByText('Content').closest('[data-slot="card"]');
    expect(card).toHaveClass('p-6');
  });

  it('deve renderizar com padding small', () => {
    render(<Card padding="sm">Content</Card>);
    const card = screen.getByText('Content').closest('[data-slot="card"]');
    expect(card).toHaveClass('p-4');
  });

  it('deve renderizar com padding none', () => {
    render(<Card padding="none">Content</Card>);
    const card = screen.getByText('Content').closest('[data-slot="card"]');
    expect(card?.className).not.toMatch(/\bp-/);
  });
});

describe('CardHeader Component', () => {
  it('deve renderizar corretamente', () => {
    render(<CardHeader>Header Content</CardHeader>);
    expect(screen.getByText('Header Content')).toBeInTheDocument();
  });
});

describe('CardTitle Component', () => {
  it('deve renderizar corretamente', () => {
    render(<CardTitle>Title</CardTitle>);
    expect(screen.getByText('Title')).toBeInTheDocument();
  });
});

describe('CardContent Component', () => {
  it('deve renderizar corretamente', () => {
    render(<CardContent>Content</CardContent>);
    expect(screen.getByText('Content')).toBeInTheDocument();
  });
});

describe('CardDescription Component', () => {
  it('deve renderizar corretamente', () => {
    render(<CardDescription>Description</CardDescription>);
    expect(screen.getByText('Description')).toBeInTheDocument();
  });
});
