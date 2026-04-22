import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { BookingModal } from "@/components/bookings/booking-modal";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { toast } from "sonner";

// Mock next-auth
vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
}));

// Mock sonner
vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

// Mock Lucide components (prevents ESM issues in some environments)
vi.mock("lucide-react", () => ({
  X: () => <div data-testid="icon-x" />,
  Calendar: () => <div data-testid="icon-calendar" />,
  Clock: () => <div data-testid="icon-clock" />,
  Loader2: () => <div data-testid="icon-loader" />,
}));

const createQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
});

const createWrapper = (queryClient: QueryClient) => ({ children }: { children: React.ReactNode }) => (
  <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
);

describe("BookingModal", () => {
  const defaultProps = {
    providerId: "provider-123",
    providerName: "Test Provider",
    serviceId: "service-456",
  };

  beforeEach(() => {
    vi.clearAllMocks();
    
    process.env.NEXT_PUBLIC_API_URL = "http://localhost:3000";

    (useSession as any).mockReturnValue({
      data: {
        user: { id: "client-123" },
        accessToken: "fake-token",
      },
      status: "authenticated",
    });

    // Mock global fetch
    global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ slots: [] })
    });
  });

  it("should render trigger button with default text", () => {
    render(<BookingModal {...defaultProps} />, { wrapper: createWrapper(createQueryClient()) });
    expect(screen.getByText("Agendar Horário")).toBeDefined();
  });

  it("should open modal when trigger is clicked", async () => {
    render(<BookingModal {...defaultProps} />, { wrapper: createWrapper(createQueryClient()) });
    
    const trigger = screen.getByText("Agendar Horário");
    fireEvent.click(trigger);
    
    await waitFor(() => {
      expect(screen.getByText(`Agendar com ${defaultProps.providerName}`)).toBeDefined();
    });
  });

  it("should display available slots when loaded from API", async () => {
    const mockAvailability = {
      slots: [
        { start: "10:00:00", end: "11:00:00" }
      ]
    };

    (global.fetch as any).mockResolvedValueOnce({
      ok: true,
      json: async () => mockAvailability,
    });

    render(<BookingModal {...defaultProps} />, { wrapper: createWrapper(createQueryClient()) });
    fireEvent.click(screen.getByText("Agendar Horário"));

    await waitFor(() => {
      expect(screen.getByText(/10:00/)).toBeDefined();
    });
  });

  it("should call create booking API when confirmed", async () => {
    const mockAvailability = {
      slots: [
        { start: "10:00:00", end: "11:00:00" }
      ]
    };

    (global.fetch as any)
      .mockResolvedValueOnce({
        ok: true,
        json: async () => mockAvailability,
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: "booking-123" }),
      });

    render(<BookingModal {...defaultProps} />, { wrapper: createWrapper(createQueryClient()) });
    fireEvent.click(screen.getByText("Agendar Horário"));

    const slotBtn = await waitFor(() => screen.getByText(/10:00/));
    fireEvent.click(slotBtn);

    const confirmBtn = await screen.findByText("Confirmar Agendamento");
    // Ensure button is not disabled
    expect(confirmBtn).not.toBeDisabled();
    fireEvent.click(confirmBtn);

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/v1/bookings"),
        expect.objectContaining({ 
          method: "POST",
          body: expect.stringContaining('"serviceId":"service-456"')
        })
      );
      expect(toast.success).toHaveBeenCalled();
    });
  });

  it("should show empty state when no slots available", async () => {
    (global.fetch as any).mockResolvedValueOnce({
      ok: true,
      json: async () => ({ slots: [] }),
    });

    render(<BookingModal {...defaultProps} />, { wrapper: createWrapper(createQueryClient()) });
    fireEvent.click(screen.getByText("Agendar Horário"));

    await waitFor(() => {
      expect(screen.getByText("Nenhum horário disponível para esta data.")).toBeDefined();
    });
  });
});
