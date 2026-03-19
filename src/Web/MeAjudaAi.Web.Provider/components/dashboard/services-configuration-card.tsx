import { Button } from "../ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../ui/card";

export function ServicesConfigurationCard() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Serviços</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <p className="text-sm text-muted-foreground">
          Configure seus serviços oferecidos, fotos, e região de atuação.
        </p>
        <Button variant="primary" className="mt-auto w-full">
          Gerenciar Serviços
        </Button>
      </CardContent>
    </Card>
  );
}
