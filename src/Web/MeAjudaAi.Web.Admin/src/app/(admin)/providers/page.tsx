"use client";

import { useState } from "react";
import { Search, Plus, Pencil, Trash2, Eye, CheckCircle, XCircle } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";

const mockProviders = [
  {
    id: "1",
    name: "João Silva",
    email: "joao@email.com",
    phone: "(11) 99999-9999",
    type: "Pessoa Física",
    status: "Aprovado",
    city: "São Paulo",
  },
  {
    id: "2",
    name: "Maria Santos",
    email: "maria@email.com",
    phone: "(11) 88888-8888",
    type: "Pessoa Física",
    status: "Pendente",
    city: "Campinas",
  },
  {
    id: "3",
    name: "Tech Solutions Ltda",
    email: "contato@techsolutions.com",
    phone: "(11) 77777-7777",
    type: "Empresa",
    status: "Aprovado",
    city: "São Paulo",
  },
];

const statusVariant = (status: string) => {
  switch (status) {
    case "Aprovado":
      return "success";
    case "Pendente":
      return "warning";
    case "Rejeitado":
      return "destructive";
    default:
      return "secondary";
  }
};

export default function ProvidersPage() {
  const [search, setSearch] = useState("");

  const filteredProviders = mockProviders.filter(
    (p) =>
      p.name.toLowerCase().includes(search.toLowerCase()) ||
      p.email.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Prestadores</h1>
          <p className="text-muted-foreground">Gerencie os prestadores de serviços</p>
        </div>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Novo Prestador
        </Button>
      </div>

      <Card className="mb-6">
        <div className="p-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar por nome ou email..."
              className="pl-10"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>
      </Card>

      <Card>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  Nome
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  Email
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  Telefone
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  Tipo
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  Status
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  Cidade
                </th>
                <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredProviders.map((provider) => (
                <tr key={provider.id} className="border-b border-border last:border-b-0">
                  <td className="px-4 py-3 text-sm font-medium">{provider.name}</td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">{provider.email}</td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">{provider.phone}</td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">{provider.type}</td>
                  <td className="px-4 py-3">
                    <Badge variant={statusVariant(provider.status)}>{provider.status}</Badge>
                  </td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">{provider.city}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      <Button variant="ghost" size="icon">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <CheckCircle className="h-4 w-4 text-green-500" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <XCircle className="h-4 w-4 text-red-500" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <Trash2 className="h-4 w-4 text-red-500" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {filteredProviders.length === 0 && (
          <div className="p-8 text-center text-muted-foreground">
            Nenhum prestador encontrado
          </div>
        )}
      </Card>
    </div>
  );
}
