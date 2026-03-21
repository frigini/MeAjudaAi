"use client";

import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { Label } from "../../components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import { useRouter } from "next/navigation";

export default function RegisterPage() {
  const router = useRouter();

  const handleRegister = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Connect to POST /api/v1/providers/public/register
    // Simulando login provisório indo pro onboarding
    router.push("/onboarding/basic-info");
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-raised p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Torne-se um Prestador</CardTitle>
          <p className="text-sm text-foreground-subtle">
            Crie sua conta para oferecer serviços na plataforma MeAjudaAí.
          </p>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleRegister} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="name" required>Nome Completo ou Fantasia</Label>
              <Input id="name" placeholder="Ex: João Silva Reformas" required />
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="document" required>CPF ou CNPJ</Label>
                <Input id="document" placeholder="000.000.000-00" required />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="phone" required>Celular</Label>
                <Input id="phone" type="tel" placeholder="(11) 99999-9999" required />
              </div>
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="email" required>E-mail Profissional</Label>
              <Input id="email" type="email" placeholder="contato@empresa.com" required />
            </div>

            <div className="flex items-center gap-2 pt-2">
              <input type="checkbox" id="terms" required className="size-4 rounded border-border text-primary focus:ring-primary" />
              <Label htmlFor="terms" className="text-xs font-normal">
                Li e aceito os Termos de Uso e Política de Privacidade.
              </Label>
            </div>

            <Button type="submit" className="mt-4 w-full" size="lg">
              Começar agora
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
