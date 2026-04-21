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

// Mock fetch
const globalFetch = vi.fn();
global.fetch = globalFetch;

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
});

const wrapper = ({ children }: { children: React.ReactNode }) => (
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
    queryClient.clear();
    
    (useSession as any).mockReturnValue({
      data: {
        user: { id: "client-123" },
        accessToken: "fake-token",
      },
      status: "authenticated",
    });
  });

  it("should render trigger button", () => {
    render(<BookingModal {...defaultProps} />, { wrapper });
    expect(screen.getByText("Solicitar Agendamento")).toBeDefined();
  });

  it("should open modal when trigger is clicked", async () => {
    render(<BookingModal {...defaultProps} />, { wrapper });
    
    const trigger = screen.getByText("Solicitar Agendamento");
    fireEvent.click(trigger);
    
    await waitFor(() => {
      expect(screen.getByText(`Agendar com ${defaultProps.providerName}`)).toBeDefined();
    });
  });

  it("should display available slots when loaded", async () => {
    const mockAvailability = {
      dayOfWeek: "Monday",
      slots: [
        { start: "2026-04-22T10:00:00Z", end: "2026-04-22T11:00:00Z" }
      ]
    };

    globalFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockAvailability,
    });

    render(<BookingModal {...defaultProps} />, { wrapper });
    fireEvent.click(screen.getByText("Solicitar Agendamento"));

    await waitFor(() => {
      expect(screen.getByText("10:00 - 11:00")).toBeDefined();
    });
  });

  it("should call create booking API when a slot is clicked", async () => {
    const mockAvailability = {
      slots: [
        { start: "2026-04-22T10:00:00Z", end: "2026-04-22T11:00:00Z" }
      ]
    };

    globalFetch
      .mockResolvedValueOnce({
        ok: true,
        json: async () => mockAvailability,
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: "booking-123" }),
      });

    render(<BookingModal {...defaultProps} />, { wrapper });
    fireEvent.click(screen.getByText("Solicitar Agendamento"));

    const slotBtn = await waitFor(() => screen.getByText("10:00 - 11:00"));
    fireEvent.click(slotBtn);

    const confirmBtn = screen.getByText("Confirmar Agendamento");
    fireEvent.click(confirmBtn);

    await waitFor(() => {
      expect(globalFetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/v1/bookings"),
        expect.objectContaining({ method: "POST" })
      );
      expect(toast.success).toHaveBeenCalled();
    });
  });
});
