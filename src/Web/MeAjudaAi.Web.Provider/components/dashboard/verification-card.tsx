import Link from "next/link";
import { Button } from "../ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../ui/card";

export function VerificationCard() {
  return (
    <Card className="md:col-span-2 lg:col-span-1">
      <CardHeader>
        <CardTitle>Documentos</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <p className="text-sm text-foreground-subtle">
          Sua conta requer envio de documentos para validação final de segurança.
        </p>
        <div className="flex flex-col gap-2 rounded-lg border border-border bg-surface p-4">
          <h4 className="text-sm font-medium">Documentos de Identidade</h4>
          <p className="text-xs text-muted-foreground">
            Envie a frente e o verso do seu RG ou CNH vigente e certificações.
          </p>
        </div>
        <Button variant="primary" className="mt-2 w-full" asChild>
          <Link href="/onboarding/documents">Fazer Upload</Link>
        </Button>
      </CardContent>
    </Card>
  );
}
