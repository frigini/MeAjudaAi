"use client";

import { Wrench, Plus } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function ServicesPage() {
  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Serviços</h1>
          <p className="text-muted-foreground">Gerencie serviços disponíveis</p>
        </div>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Novo Serviço
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Wrench className="h-5 w-5" />
            Catálogo de Serviços
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">
            Funcionalidade em desenvolvimento.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
