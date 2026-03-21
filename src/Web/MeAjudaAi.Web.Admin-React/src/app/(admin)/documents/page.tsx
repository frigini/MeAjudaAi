"use client";

import { FileText, Upload, Info } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function DocumentsPage() {
  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Documentos</h1>
          <p className="text-muted-foreground">Gerencie documentos dos prestadores</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><FileText className="h-5 w-5" />Gestão de Documentos</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <div className="mb-4 rounded-full bg-muted p-4">
              <Info className="h-8 w-8 text-muted-foreground" />
            </div>
            <h3 className="mb-2 text-lg font-medium">Documentos por Prestador</h3>
            <p className="mb-6 max-w-md text-sm text-muted-foreground">
              A gestão de documentos é feita através da visualização detalhada de cada prestador.
              Acesse a lista de prestadores e clique em um deles para ver e gerenciar seus documentos.
            </p>
            <Button variant="outline">
              <Upload className="mr-2 h-4 w-4" />Ir para Prestadores
            </Button>
          </div>
        </CardContent>
      </Card>

      <div className="mt-6 grid gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="pt-6">
            <div className="text-2xl font-bold">CNPJ</div>
            <p className="text-sm text-muted-foreground">Documento de empresa</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-2xl font-bold">RG/CPF</div>
            <p className="text-sm text-muted-foreground">Documento de identidade</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-2xl font-bold">Comprovante</div>
            <p className="text-sm text-muted-foreground">Comprovante de residência</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
