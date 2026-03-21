"use client";

import { Button } from "../../../components/ui/button";
import { Label } from "../../../components/ui/label";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { twMerge } from "tailwind-merge";

const MOCK_SERVICES = [
  { id: "1", name: "Limpeza Geral" },
  { id: "2", name: "Eletricista" },
  { id: "3", name: "Encanador" },
  { id: "4", name: "Pintor" },
  { id: "5", name: "Montador de Móveis" },
];

export default function ServicesPage() {
  const router = useRouter();
  const [selectedServices, setSelectedServices] = useState<string[]>([]);

  const toggleService = (id: string) => {
    setSelectedServices(prev => 
      prev.includes(id) ? prev.filter(s => s !== id) : [...prev, id]
    );
  };

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Connect to PUT /api/v1/providers/me/services (ProviderServiceDto[])
    router.push("/onboarding/documents");
  };

  return (
    <form onSubmit={handleSave} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Seleção de Serviços</h2>
        <p className="mt-1 text-sm text-foreground-subtle">
          Quais serviços você pretende oferecer na plataforma?
        </p>
      </div>

      <div className="flex flex-col gap-4 border-t border-border pt-6">
        <Label>Categorias Disponíveis</Label>
        
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
          {MOCK_SERVICES.map(service => {
            const isSelected = selectedServices.includes(service.id);
            return (
              <button
                key={service.id}
                type="button"
                onClick={() => toggleService(service.id)}
                className={twMerge(
                  "flex items-center justify-center rounded-lg border p-4 text-sm font-medium transition-colors",
                  isSelected
                    ? "border-primary bg-primary text-primary-foreground"
                    : "border-border bg-surface text-foreground hover:bg-surface-raised"
                )}
              >
                {service.name}
              </button>
            );
          })}
        </div>
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()}>
          Voltar
        </Button>
        <Button type="submit" disabled={selectedServices.length === 0}>
          Salvar e Continuar
        </Button>
      </div>
    </form>
  );
}
