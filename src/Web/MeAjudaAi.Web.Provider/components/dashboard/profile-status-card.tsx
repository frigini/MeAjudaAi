import { Button } from "../ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../ui/card";

export function ProfileStatusCard() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Status do Perfil</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div className="flex items-center gap-3">
          <div className="h-2 w-2 rounded-full bg-emerald-500" />
          <p className="text-sm font-medium">Ativo e visível</p>
        </div>
        <p className="text-sm text-muted-foreground">
          Seu perfil está acessível nas buscas da sua região.
        </p>
        <Button variant="secondary" className="mt-2 w-full">
          Pausar Visibilidade
        </Button>
      </CardContent>
    </Card>
  );
}
