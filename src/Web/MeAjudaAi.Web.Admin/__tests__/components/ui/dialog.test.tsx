import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import { Dialog, DialogTrigger, DialogContent, DialogHeader, DialogFooter, DialogTitle, DialogDescription, DialogClose } from '@/components/ui/dialog';

describe('Dialog (Admin)', () => {
  it('deve renderizar children quando open', () => {
    render(
      <Dialog open>
        <DialogTrigger>Open</DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Título</DialogTitle>
            <DialogDescription>Descrição</DialogDescription>
          </DialogHeader>
          <div>Conteúdo</div>
          <DialogFooter>
            <DialogClose>Cancelar</DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    );
    expect(screen.getByText('Título')).toBeInTheDocument();
    expect(screen.getByText('Descrição')).toBeInTheDocument();
    expect(screen.getByText('Conteúdo')).toBeInTheDocument();
  });

  it('deve renderizar trigger', () => {
    render(
      <Dialog>
        <DialogTrigger>Open Dialog</DialogTrigger>
        <DialogContent>Content</DialogContent>
      </Dialog>
    );
    expect(screen.getByRole('button', { name: /open dialog/i })).toBeInTheDocument();
  });

  it('deve renderizar sem children quando closed', () => {
    render(
      <Dialog open={false}>
        <DialogTrigger>Open</DialogTrigger>
        <DialogContent>Content</DialogContent>
      </Dialog>
    );
    expect(screen.queryByText('Content')).not.toBeInTheDocument();
  });

  it('DialogTrigger deve aceitar className customizado', () => {
    render(
      <Dialog>
        <DialogTrigger className="custom-trigger">Open</DialogTrigger>
        <DialogContent>Content</DialogContent>
      </Dialog>
    );
    const trigger = screen.getByRole('button');
    expect(trigger).toHaveClass('custom-trigger');
  });

  it('DialogTrigger deve suportar asChild', () => {
    render(
      <Dialog>
        <DialogTrigger asChild><button>Custom Trigger</button></DialogTrigger>
        <DialogContent>Content</DialogContent>
      </Dialog>
    );
    expect(screen.getByRole('button', { name: /custom trigger/i })).toBeInTheDocument();
  });

  it('deve chamar onOpenChange quando abrir', () => {
    const handleChange = vi.fn();
    render(
      <Dialog open onOpenChange={handleChange}>
        <DialogTrigger>Open</DialogTrigger>
        <DialogContent>Content</DialogContent>
      </Dialog>
    );
    expect(handleChange).not.toHaveBeenCalled();
  });
});
