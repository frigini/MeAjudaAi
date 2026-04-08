import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { Badge } from '@/components/ui/badge';

const badgeVariants = [
  ['default', 'bg-primary'],
  ['secondary', 'bg-secondary'],
  ['destructive', 'bg-destructive'],
  ['success', 'bg-green-100'],
  ['warning', 'bg-yellow-100'],
] as const;

describe('Badge (Admin)', () => {
  it('should render Badge component', () => {
    render(<Badge>Test Badge</Badge>);
    expect(screen.getByText('Test Badge')).toBeInTheDocument();
  });

  test.each(badgeVariants)('should render %s variant with correct classes', (variant, expectedClass) => {
    render(<Badge variant={variant}>Test</Badge>);
    expect(screen.getByText('Test')).toHaveClass(expectedClass);
  });

  it('should apply custom className', () => {
    render(<Badge className="custom-class">Test</Badge>);
    expect(screen.getByText('Test')).toHaveClass('custom-class');
  });
});
