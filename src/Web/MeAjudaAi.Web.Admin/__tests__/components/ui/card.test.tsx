import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

describe('Card (Admin)', () => {
  it('should render Card component', () => {
    render(<Card>Card Content</Card>);
    expect(screen.getByText('Card Content')).toBeInTheDocument();
  });

  it('should render all card sub-components', () => {
    render(
      <Card>
        <CardHeader>
          <CardTitle>Test Title</CardTitle>
        </CardHeader>
        <CardContent>Test Content</CardContent>
      </Card>
    );

    expect(screen.getByText('Test Title')).toBeInTheDocument();
    expect(screen.getByText('Test Content')).toBeInTheDocument();
    expect(screen.getByRole('heading', { level: 3, name: 'Test Title' })).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    render(<Card className="custom-class">Test</Card>);
    expect(screen.getByText('Test')).toHaveClass('custom-class');
  });

  it('should apply custom className to subcomponents', () => {
    render(
      <Card>
        <CardHeader className="header-class">
          <CardTitle className="title-class">Title</CardTitle>
        </CardHeader>
        <CardContent className="content-class">Content</CardContent>
      </Card>
    );

    expect(screen.getByText('Title').closest('div')).toHaveClass('header-class');
    expect(screen.getByText('Title')).toHaveClass('title-class');
    expect(screen.getByText('Content')).toHaveClass('content-class');
  });
});
