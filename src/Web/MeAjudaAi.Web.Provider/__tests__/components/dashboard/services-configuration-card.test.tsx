import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ServicesConfigurationCard } from '@/components/dashboard/services-configuration-card';

describe('ServicesConfigurationCard', () => {
  it('should render card title', () => {
    render(<ServicesConfigurationCard />);
    expect(screen.getByText('Serviços')).toBeInTheDocument();
  });

  it('should render description', () => {
    render(<ServicesConfigurationCard />);
    expect(screen.getByText(/Configure seus serviços/i)).toBeInTheDocument();
  });

  it('should render manage button', () => {
    render(<ServicesConfigurationCard />);
    expect(screen.getByRole('button', { name: /Gerenciar Serviços/i })).toBeInTheDocument();
  });
});
