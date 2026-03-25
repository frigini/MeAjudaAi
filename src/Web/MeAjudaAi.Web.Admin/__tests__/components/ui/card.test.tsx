import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

describe('Card (Admin)', () => {
  it('deve renderizar com conteúdo básico', () => {
    render(
      <Card>
        <CardHeader>
          <CardTitle>Título</CardTitle>
        </CardHeader>
        <CardContent>Conteúdo</CardContent>
      </Card>
    );
    expect(screen.getByText('Título')).toBeInTheDocument();
    expect(screen.getByText('Conteúdo')).toBeInTheDocument();
  });
});
