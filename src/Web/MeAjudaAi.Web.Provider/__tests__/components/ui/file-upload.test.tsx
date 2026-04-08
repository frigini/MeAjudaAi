import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { FileUpload } from '@/components/ui/file-upload';

describe('FileUpload', () => {
  const mockOnFileSelect = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render upload area', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    expect(screen.getByText(/clique para enviar/i)).toBeInTheDocument();
  });

  it('should render label', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    expect(screen.getByText('Documento')).toBeInTheDocument();
  });

  it('should render description when provided', () => {
    render(<FileUpload label="Documento" description="Descrição do arquivo" onFileSelect={mockOnFileSelect} />);
    expect(screen.getByText('Descrição do arquivo')).toBeInTheDocument();
  });

  it('should render with required indicator', () => {
    render(<FileUpload label="Documento" required onFileSelect={mockOnFileSelect} />);
    expect(screen.getByText('Documento')).toBeInTheDocument();
    expect(screen.getByText('*')).toBeInTheDocument();
  });

  it('should call onFileSelect when file is selected via input', async () => {
    const user = userEvent.setup();
    const file = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await user.upload(input, file);
    
    expect(mockOnFileSelect).toHaveBeenCalledWith(file);
  });

  it('should show selected file name', async () => {
    const user = userEvent.setup();
    const file = new File(['test'], 'my-document.pdf', { type: 'application/pdf' });
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await user.upload(input, file);
    
    expect(screen.getByText('my-document.pdf')).toBeInTheDocument();
  });

  it('should show selected file size', async () => {
    const user = userEvent.setup();
    const file = new File(['test content'], 'test.pdf', { type: 'application/pdf' });
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await user.upload(input, file);
    
    expect(screen.getByText(/mb/i)).toBeInTheDocument();
  });

  it('should remove file when remove button is clicked', async () => {
    const user = userEvent.setup();
    const file = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    await user.upload(input, file);
    
    expect(screen.getByText('test.pdf')).toBeInTheDocument();
    
    const removeButton = screen.getByRole('button');
    await user.click(removeButton);
    
    expect(screen.queryByText('test.pdf')).not.toBeInTheDocument();
    expect(screen.getByText(/clique para enviar/i)).toBeInTheDocument();
  });

  it('should handle drag enter', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    const dropZone = screen.getByText(/clique para enviar/i).closest('.border-dashed');
    
    fireEvent.dragEnter(dropZone!);
    expect(dropZone).toHaveClass('border-primary');
  });

  it('should handle drag over', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    const dropZone = screen.getByText(/clique para enviar/i).closest('.border-dashed');
    
    fireEvent.dragOver(dropZone!);
    expect(dropZone).toHaveClass('border-primary'); // Should set isDragging to true
  });

  it('should handle drag leave', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    const dropZone = screen.getByText(/clique para enviar/i).closest('.border-dashed');
    
    fireEvent.dragEnter(dropZone!);
    fireEvent.dragLeave(dropZone!);
    expect(dropZone).not.toHaveClass('border-primary');
  });

  it('should not call onFileSelect when drop has no files', async () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    const dropZone = screen.getByText(/clique para enviar/i).closest('.border-dashed');
    
    fireEvent.drop(dropZone!, {
      dataTransfer: {
        files: [],
      },
    });
    
    expect(mockOnFileSelect).not.toHaveBeenCalled();
  });

  it('should handle drop', async () => {
    const user = userEvent.setup();
    const file = new File(['test'], 'dropped.pdf', { type: 'application/pdf' });
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    
    const dropZone = screen.getByText(/clique para enviar/i).closest('.border-dashed');
    
    fireEvent.drop(dropZone!, {
      dataTransfer: {
        files: [file],
      },
    });
    
    expect(mockOnFileSelect).toHaveBeenCalledWith(file);
  });

  it('should apply custom className', () => {
    render(<FileUpload label="Documento" className="custom-class" onFileSelect={mockOnFileSelect} />);
    const label = screen.getByText('Documento');
    const container = label.parentElement;
    expect(container).toHaveClass('custom-class');
  });

  it('should accept custom accept prop', () => {
    render(<FileUpload label="Documento" accept=".png,.jpg" onFileSelect={mockOnFileSelect} />);
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    expect(input).toHaveAttribute('accept', '.png,.jpg');
  });
});
