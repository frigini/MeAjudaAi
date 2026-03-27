import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { FileUpload } from '@/components/ui/file-upload';

describe('FileUpload', () => {
  const mockOnFileSelect = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should Render upload area', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    expect(screen.getByText(/clique para enviar/i)).toBeInTheDocument();
  });

  it('should Render label', () => {
    render(<FileUpload label="Documento" onFileSelect={mockOnFileSelect} />);
    expect(screen.getByText('Documento')).toBeInTheDocument();
  });
});
