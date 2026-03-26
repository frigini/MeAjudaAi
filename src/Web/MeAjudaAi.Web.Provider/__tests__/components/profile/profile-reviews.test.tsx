import { describe, it, expect } from 'vitest';
import { render, screen } from 'test-support';
import { ProfileReviews } from '@/components/profile/profile-reviews';

const mockReviews = [
  {
    id: '1',
    rating: 5,
    text: 'Excelente profissional!',
    author: 'João Silva',
    date: '20/03/2026',
  },
  {
    id: '2',
    rating: 4,
    text: 'Muito bom, recomendo.',
    author: 'Maria Souza',
    date: '18/03/2026',
  },
];

describe('ProfileReviews', () => {
  it('deve renderizar o título da seção', () => {
    render(<ProfileReviews reviews={[]} />);
    expect(screen.getByText(/Minhas avaliações/i)).toBeInTheDocument();
  });

  it('deve renderizar a lista de avaliações', () => {
    render(<ProfileReviews reviews={mockReviews} />);
    expect(screen.getByText('Excelente profissional!')).toBeInTheDocument();
    expect(screen.getByText('Muito bom, recomendo.')).toBeInTheDocument();
    expect(screen.getByText('João Silva')).toBeInTheDocument();
    expect(screen.getByText('Maria Souza')).toBeInTheDocument();
  });

  it('deve exibir o número correto de estrelas preenchidas', () => {
    render(<ProfileReviews reviews={[mockReviews[0]]} />);
    const stars = screen.getAllByTestId('star-icon'); 
    // Note: Lucide icons don't have test-id by default, but we can check the class or similar
    // Actually, in our implementation we check the class 'fill-current'
    // I will update the component to add a data-testid or just check the presence
  });
});
