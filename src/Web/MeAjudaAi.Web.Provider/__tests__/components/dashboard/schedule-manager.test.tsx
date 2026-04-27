import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { ScheduleManager } from "@/components/dashboard/schedule-manager";
import { toast } from "sonner";

// Mock sonner
vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe("ScheduleManager", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('fetch', vi.fn());
  });

  it("should render correctly with all days of week", () => {
    render(<ScheduleManager />);
    expect(screen.getByText("Segunda-feira")).toBeDefined();
    expect(screen.getByText("Terça-feira")).toBeDefined();
    expect(screen.getByText("Domingo")).toBeDefined();
  });

  it("should add a new time slot when button is clicked", () => {
    render(<ScheduleManager />);
    
    // Segunda-feira é o primeiro Card. Pegamos o botão de adicionar dentro dele.
    const addButtons = screen.getAllByRole("button", { name: /Adicionar/i });
    fireEvent.click(addButtons[0]);

    expect(screen.getByDisplayValue("08:00")).toBeDefined();
    expect(screen.getByDisplayValue("12:00")).toBeDefined();
  });

  it("should remove a time slot when delete button is clicked", async () => {
    render(<ScheduleManager />);
    
    const addButtons = screen.getAllByRole("button", { name: /Adicionar/i });
    fireEvent.click(addButtons[0]);

    const deleteButton = screen.getByRole("button", { name: /remover/i });
    fireEvent.click(deleteButton);

    expect(screen.queryByDisplayValue("08:00")).toBeNull();
  });

  it("should call toast success when save button is clicked", async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      json: async () => ({ success: true }),
    } as Response);

    render(<ScheduleManager />);
    
    const saveButton = screen.getByText("Salvar Alterações");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith("Agenda atualizada com sucesso!");
    }, { timeout: 2000 });
  });
});
