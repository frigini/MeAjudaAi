import { render } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { Skeleton } from '@/components/ui/skeleton';

describe('Skeleton Component', () => {
  it('renders correctly with default classes', () => {
    const { container } = render(<Skeleton className="w-[100px] h-[20px]" />);
    const skeleton = container.firstChild as HTMLElement;
    
    expect(skeleton).toBeInTheDocument();
    expect(skeleton).toHaveClass('animate-pulse');
    expect(skeleton).toHaveClass('rounded-md');
    expect(skeleton).toHaveClass('bg-muted');
  });

  it('applies custom className', () => {
    const { container } = render(<Skeleton className="custom-class" />);
    const skeleton = container.firstChild as HTMLElement;
    
    expect(skeleton).toHaveClass('custom-class');
  });

  it('overrides conflicting classes using twMerge', () => {
    const { container } = render(<Skeleton className="rounded-none" />);
    const skeleton = container.firstChild as HTMLElement;
    
    expect(skeleton).toHaveClass('rounded-none');
    expect(skeleton).not.toHaveClass('rounded-md');
  });
});
