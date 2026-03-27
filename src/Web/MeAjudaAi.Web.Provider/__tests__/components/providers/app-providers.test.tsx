import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@/components/providers/app-providers';

describe('AppProviders', () => {
  it('should render children', () => {
    render(<AppProviders>Test</AppProviders>);
    expect(screen.getByText('Test')).toBeInTheDocument();
  });
});
