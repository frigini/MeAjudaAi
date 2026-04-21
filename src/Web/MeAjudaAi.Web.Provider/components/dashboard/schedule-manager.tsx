"use client";

import React, { useState } from "react";
import { Plus, Trash2, Save, Clock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";

// Tipos temporários (serão substituídos pelos do OpenAPI se possível)
interface TimeSlot {
    start: string; // HH:mm
    end: string;   // HH:mm
}

interface DayAvailability {
    dayOfWeek: number;
    slots: TimeSlot[];
}

const DAYS_OF_WEEK = [
    { id: 1, name: "Segunda-feira" },
    { id: 2, name: "Terça-feira" },
    { id: 3, name: "Quarta-feira" },
    { id: 4, name: "Quinta-feira" },
    { id: 5, name: "Sexta-feira" },
    { id: 6, name: "Sábado" },
    { id: 0, name: "Domingo" },
];

export function ScheduleManager() {
    const [availabilities, setAvailabilities] = useState<DayAvailability[]>(
        DAYS_OF_WEEK.map(d => ({ dayOfWeek: d.id, slots: [] }))
    );
    const [isLoading, setIsLoading] = useState(false);

    const addSlot = (dayId: number) => {
        setAvailabilities(prev => prev.map(day => {
            if (day.dayOfWeek === dayId) {
                return {
                    ...day,
                    slots: [...day.slots, { start: "08:00", end: "12:00" }]
                };
            }
            return day;
        }));
    };

    const removeSlot = (dayId: number, index: number) => {
        setAvailabilities(prev => prev.map(day => {
            if (day.dayOfWeek === dayId) {
                const newSlots = [...day.slots];
                newSlots.splice(index, 1);
                return { ...day, slots: newSlots };
            }
            return day;
        }));
    };

    const updateSlot = (dayId: number, index: number, field: keyof TimeSlot, value: string) => {
        setAvailabilities(prev => prev.map(day => {
            if (day.dayOfWeek === dayId) {
                const newSlots = [...day.slots];
                newSlots[index] = { ...newSlots[index], [field]: value };
                return { ...day, slots: newSlots };
            }
            return day;
        }));
    };

    const handleSave = async () => {
        setIsLoading(true);
        try {
            // TODO: Integrar com a API SetProviderSchedule
            // await api.bookings.setProviderSchedule({ availabilities });
            console.log("Saving availabilities:", availabilities);
            
            await new Promise(resolve => setTimeout(resolve, 1000)); // Mock delay
            toast.success("Agenda atualizada com sucesso!");
        } catch (error) {
            toast.error("Erro ao salvar agenda. Tente novamente.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight text-[#002D62]">Minha Agenda</h2>
                    <p className="text-sm text-muted-foreground">
                        Defina os horários em que você está disponível para atender clientes.
                    </p>
                </div>
                <Button 
                    onClick={handleSave} 
                    disabled={isLoading}
                    className="bg-[#E0702B] hover:bg-[#C55A1F] text-white"
                >
                    {isLoading ? "Salvando..." : (
                        <span className="flex items-center gap-2">
                            <Save className="h-4 w-4" /> Salvar Alterações
                        </span>
                    )}
                </Button>
            </div>

            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {DAYS_OF_WEEK.map((dayInfo) => {
                    const dayData = availabilities.find(a => a.dayOfWeek === dayInfo.id);
                    return (
                        <Card key={dayInfo.id} className="border-[#002D62]/10">
                            <CardHeader className="pb-3">
                                <div className="flex items-center justify-between">
                                    <CardTitle className="text-lg font-semibold">{dayInfo.name}</CardTitle>
                                    <Button 
                                        variant="outline" 
                                        size="sm" 
                                        onClick={() => addSlot(dayInfo.id)}
                                        className="h-8 border-[#002D62]/20 text-[#002D62] hover:bg-[#002D62]/5"
                                    >
                                        <Plus className="h-4 w-4 mr-1" /> Adicionar
                                    </Button>
                                </div>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                {dayData?.slots.length === 0 ? (
                                    <p className="text-sm text-muted-foreground italic text-center py-4">
                                        Nenhum horário definido
                                    </p>
                                ) : (
                                    dayData?.slots.map((slot, index) => (
                                        <div key={index} className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-100 group transition-all hover:border-[#E0702B]/30">
                                            <div className="grid grid-cols-2 gap-2 flex-1">
                                                <div className="space-y-1">
                                                    <Label className="text-[10px] uppercase font-bold text-slate-500">Início</Label>
                                                    <input 
                                                        type="time" 
                                                        value={slot.start}
                                                        onChange={(e) => updateSlot(dayInfo.id, index, 'start', e.target.value)}
                                                        className="w-full text-sm bg-transparent border-none focus:ring-0 p-0 font-medium"
                                                    />
                                                </div>
                                                <div className="space-y-1">
                                                    <Label className="text-[10px] uppercase font-bold text-slate-500">Fim</Label>
                                                    <input 
                                                        type="time" 
                                                        value={slot.end}
                                                        onChange={(e) => updateSlot(dayInfo.id, index, 'end', e.target.value)}
                                                        className="w-full text-sm bg-transparent border-none focus:ring-0 p-0 font-medium"
                                                    />
                                                </div>
                                            </div>
                                            <Button 
                                                variant="ghost" 
                                                size="icon" 
                                                onClick={() => removeSlot(dayInfo.id, index)}
                                                className="h-8 w-8 text-destructive opacity-0 group-hover:opacity-100 transition-opacity"
                                            >
                                                <Trash2 className="h-4 w-4" />
                                            </Button>
                                        </div>
                                    ))
                                )}
                            </CardContent>
                        </Card>
                    );
                })}
            </div>

            <Card className="bg-[#002D62]/5 border-none">
                <CardContent className="p-4 flex gap-3 items-start">
                    <Clock className="h-5 w-5 text-[#002D62] mt-0.5" />
                    <div className="text-sm text-[#002D62]/80">
                        <p className="font-semibold">Dica:</p>
                        <p>Os horários configurados aqui permitem que os clientes agendem seus serviços automaticamente. Certifique-se de manter sua agenda sempre atualizada.</p>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
