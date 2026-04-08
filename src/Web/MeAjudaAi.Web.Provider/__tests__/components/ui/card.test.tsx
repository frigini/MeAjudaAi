import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

describe('Card (Provider)', () => {
  it('should render Card with children', () => {
    render(<Card>Card Content</Card>);
    expect(screen.getByText('Card Content')).toBeInTheDocument();
  });

  it('should render CardHeader, CardTitle, and CardContent', () => {
    render(
      <Card>
        <CardHeader>
          <CardTitle>Title</CardTitle>
        </CardHeader>
        <CardContent>Content</CardContent>
      </Card>
    );

    expect(screen.getByText('Title')).toBeInTheDocument();
    expect(screen.getByText('Content')).toBeInTheDocument();
    expect(screen.getByRole('heading', { level: 3, name: 'Title' })).toBeInTheDocument();
  });

  it('should apply custom className to Card', () => {
    render(<Card className="custom-card">Content</Card>);
    const card = screen.getByText('Content');
    expect(card).toHaveClass('custom-card');
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
