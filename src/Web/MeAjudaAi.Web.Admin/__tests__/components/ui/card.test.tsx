import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/card';

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
        <CardHeader className="header-class" data-testid="card-header">
          <CardTitle className="title-class">Title</CardTitle>
        </CardHeader>
        <CardContent className="content-class">Content</CardContent>
      </Card>
    );

    expect(screen.getByTestId('card-header')).toHaveClass('header-class');
    expect(screen.getByText('Title')).toHaveClass('title-class');
    expect(screen.getByText('Content')).toHaveClass('content-class');
  });

  it('should render CardDescription with correct styling', () => {
    render(
      <Card>
        <CardHeader>
          <CardTitle>Title</CardTitle>
          <CardDescription>Description text</CardDescription>
        </CardHeader>
      </Card>
    );

    expect(screen.getByText('Description text')).toBeInTheDocument();
    expect(screen.getByText('Description text')).toHaveClass('text-sm', 'text-muted-foreground');
  });
});
