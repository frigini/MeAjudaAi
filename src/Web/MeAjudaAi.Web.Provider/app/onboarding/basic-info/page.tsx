"use client";

import { Button } from "../../../components/ui/button";
import { Input } from "../../../components/ui/input";
import { Label } from "../../../components/ui/label";
import { useRouter } from "next/navigation";

export default function BasicInfoPage() {
  const router = useRouter();

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Connect to PUT /api/v1/providers/me/profile map to UpdateProviderProfileRequest
    router.push("/onboarding/services");
  };

  return (
    <form onSubmit={handleSave} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Informações Básicas</h2>
        <p className="mt-1 text-sm text-foreground-subtle">
          Complete os dados do seu perfil de negócio (BusinessProfile) para que os clientes te conheçam melhor.
        </p>
      </div>

      <div className="flex flex-col gap-4 border-t border-border pt-6">
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="description">Descrição do Negócio / Biografia</Label>
          <textarea
            id="description"
            rows={4}
            className="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            placeholder="Conte um pouco sobre sua experiência e diferenciais..."
          />
        </div>

        <h3 className="text-sm font-semibold text-foreground pt-4">Endereço Principal</h3>
        
        <div className="grid grid-cols-6 gap-4">
          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="zipCode">CEP</Label>
            <Input id="zipCode" placeholder="00000-000" />
          </div>
          <div className="col-span-4 flex flex-col gap-1.5">
            <Label htmlFor="street">Rua / Avenida</Label>
            <Input id="street" placeholder="Av. Principal" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="number">Número</Label>
            <Input id="number" placeholder="123" />
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="neighborhood">Bairro</Label>
            <Input id="neighborhood" placeholder="Centro" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="city">Cidade</Label>
            <Input id="city" placeholder="São Paulo" />
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="state">Estado</Label>
            <Input id="state" placeholder="SP" />
          </div>
        </div>
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()}>
          Voltar
        </Button>
        <Button type="submit">
          Salvar e Continuar
        </Button>
      </div>
    </form>
  );
}
