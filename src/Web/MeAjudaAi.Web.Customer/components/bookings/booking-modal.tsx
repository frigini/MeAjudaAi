"use client";

import React, { useState } from "react";
import * as Dialog from "@radix-ui/react-dialog";
import { X, Calendar as CalendarIcon, Clock, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { format, addDays } from "date-fns";
import { useQuery, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { useSession } from "next-auth/react";
import { z } from "zod";

const AvailabilitySchema = z.object({
    slots: z.array(z.object({
        start: z.string(),
        end: z.string()
    }))
});

interface BookingModalProps {
    providerId: string;
    providerName: string;
    serviceId: string;
    trigger?: React.ReactNode;
}

interface TimeSlot {
    start: string;
    end: string;
}

export function BookingModal({ providerId, providerName, serviceId, trigger }: BookingModalProps) {
    const { data: session } = useSession();
    const [open, setOpen] = useState(false);
    
    // Inicializa com amanhã em fuso local para evitar problemas de parsing UTC
    const [selectedDate, setSelectedDate] = useState<Date>(() => {
        const d = new Date();
        d.setDate(d.getDate() + 1);
        d.setHours(0, 0, 0, 0);
        return d;
    });
    
    const [selectedSlot, setSelectedSlot] = useState<TimeSlot | null>(null);

    const combineDateAndTime = (date: Date, timeString: string) => {
        const [hours, minutes, seconds] = timeString.split(":").map(Number);
        const combinedDate = new Date(date);
        combinedDate.setHours(hours, minutes, seconds || 0, 0);
        return format(combinedDate, "yyyy-MM-dd'T'HH:mm:ssXXX");
    };

    const parseSlotTime = (timeString: string) => {
        if (!timeString) return new Date();
        if (timeString.includes("T")) return new Date(timeString);
        
        const [hours, minutes, seconds] = timeString.split(":").map(Number);
        const d = new Date(selectedDate);
        d.setHours(hours, minutes, seconds || 0, 0);
        return d;
    };

    // Consulta disponibilidade
    const { data: availability, isLoading: isLoadingAvailability } = useQuery({
        queryKey: ["provider-availability", providerId, format(selectedDate, "yyyy-MM-dd")],
        queryFn: async () => {
            const apiUrl = process.env.NEXT_PUBLIC_API_URL;
            const res = await fetch(`${apiUrl}/api/v1/bookings/availability/${providerId}?date=${format(selectedDate, "yyyy-MM-dd")}`, {
                headers: session?.accessToken ? { "Authorization": `Bearer ${session.accessToken}` } : {}
            });
            if (!res.ok) throw new Error("Falha ao carregar disponibilidade");
            const data = await res.json();
            return AvailabilitySchema.parse(data);
        },
        enabled: open && !!providerId,
    });

    // Mutação para criar agendamento
    const createBooking = useMutation({
        mutationFn: async () => {
            if (!session) {
                throw new Error("Sua sessão expirou. Faça login novamente.");
            }
            if (!selectedSlot) throw new Error("Selecione um horário.");
            
            const clientId = session?.user?.id;
            const accessToken = session?.accessToken;

            if (!clientId || !accessToken) {
                throw new Error("Você precisa estar autenticado para realizar um agendamento.");
            }
            const apiUrl = process.env.NEXT_PUBLIC_API_URL;
            const res = await fetch(`${apiUrl}/api/v1/bookings`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${accessToken}`
                },
                body: JSON.stringify({
                    providerId,
                    serviceId,
                    start: combineDateAndTime(selectedDate, selectedSlot.start),
                    end: combineDateAndTime(selectedDate, selectedSlot.end)
                })
            });

            if (!res.ok) {
                const error = await res.json();
                throw new Error(error.detail || error.message || "Erro ao criar agendamento");
            }
            return res.json();
        },
        onSuccess: () => {
            toast.success("Solicitação de agendamento enviada com sucesso!");
            setOpen(false);
            setSelectedSlot(null);
        },
        onError: (error: Error) => {
            toast.error(error.message || "Erro ao criar agendamento");
        }
    });

    const handleDateChange = (dateString: string) => {
        // Parsing manual para evitar o "dia anterior" em fusos negativos (UTC vs Local)
        const [year, month, day] = dateString.split('-').map(Number);
        const newDate = new Date(year, month - 1, day, 0, 0, 0, 0);
        setSelectedDate(newDate);
        setSelectedSlot(null);
    };

    const isConfirmDisabled = !selectedSlot || !serviceId || createBooking.isPending || !session?.user?.id || !session?.accessToken;

    return (
        <Dialog.Root open={open} onOpenChange={setOpen}>
            <Dialog.Trigger asChild>
                {trigger || <Button className="w-full bg-[#E0702B] hover:bg-[#C55A1F] text-white font-bold py-6 text-lg shadow-lg transition-all hover:scale-[1.02] active:scale-[0.98]">Agendar Horário</Button>}
            </Dialog.Trigger>
            <Dialog.Portal>
                <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 animate-in fade-in duration-200" />
                <Dialog.Content className="fixed left-[50%] top-[50%] z-50 grid w-full max-w-md translate-x-[-50%] translate-y-[-50%] gap-4 border bg-white p-6 shadow-2xl duration-200 animate-in fade-in zoom-in-95 sm:rounded-2xl">
                    <div className="flex flex-col space-y-1.5 text-center sm:text-left">
                        <Dialog.Title className="text-xl font-bold text-[#002D62]">Agendar com {providerName}</Dialog.Title>
                        <Dialog.Description className="text-sm text-muted-foreground">
                            Escolha a data e o horário desejado para o atendimento.
                        </Dialog.Description>
                    </div>

                    <div className="grid gap-6 py-4">
                        <div className="space-y-2">
                            <label className="text-sm font-bold flex items-center gap-2">
                                <CalendarIcon className="h-4 w-4 text-[#E0702B]" /> Selecione a Data
                            </label>
                            <input 
                                type="date" 
                                min={format(addDays(new Date(), 1), "yyyy-MM-dd")}
                                value={format(selectedDate, "yyyy-MM-dd")}
                                onChange={(e) => handleDateChange(e.target.value)}
                                className="w-full p-2 border rounded-md focus:ring-2 focus:ring-[#E0702B] outline-none"
                            />
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-bold flex items-center gap-2">
                                <Clock className="h-4 w-4 text-[#E0702B]" /> Horários Disponíveis
                            </label>
                            {isLoadingAvailability ? (
                                <div className="flex items-center justify-center py-8">
                                    <Loader2 className="h-6 w-6 animate-spin text-primary" />
                                </div>
                            ) : (availability && availability.slots.length > 0) ? (
                                <div className="grid grid-cols-2 gap-2 max-h-[200px] overflow-y-auto p-1">
                                    {availability.slots.map((slot: TimeSlot, i: number) => (
                                        <button
                                            key={i}
                                            onClick={() => setSelectedSlot(slot)}
                                            className={`p-2 text-[11px] font-medium border rounded-md transition-colors ${
                                                selectedSlot === slot 
                                                    ? "bg-[#002D62] text-white border-[#002D62]" 
                                                    : "hover:border-[#E0702B] hover:bg-[#E0702B]/5 text-gray-700"
                                            }`}
                                        >
                                            {format(parseSlotTime(slot.start), "HH:mm")} - {format(parseSlotTime(slot.end), "HH:mm")}
                                        </button>
                                    ))}
                                </div>
                            ) : (
                                <p className="text-sm text-muted-foreground italic text-center py-8 bg-gray-50 rounded-lg border border-dashed">
                                    Nenhum horário disponível para esta data.
                                </p>
                            )}
                        </div>
                    </div>

                    <div className="flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-2 gap-2">
                        {!serviceId && (
                            <p className="text-[10px] text-red-500 self-center mr-auto">
                                Nenhum serviço disponível para agendamento.
                            </p>
                        )}
                        <Dialog.Close asChild>
                            <Button variant="ghost">Cancelar</Button>
                        </Dialog.Close>
                        <Button 
                            disabled={isConfirmDisabled}
                            onClick={() => createBooking.mutate()}
                            className="bg-[#E0702B] hover:bg-[#C55A1F] text-white font-bold"
                        >
                            {createBooking.isPending ? "Confirmando..." : "Confirmar Agendamento"}
                        </Button>
                    </div>

                    <Dialog.Close className="absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:pointer-events-none data-[state=open]:bg-accent data-[state=open]:text-muted-foreground">
                        <X className="h-4 w-4" />
                        <span className="sr-only">Fechar</span>
                    </Dialog.Close>
                </Dialog.Content>
            </Dialog.Portal>
        </Dialog.Root>
    );
}
