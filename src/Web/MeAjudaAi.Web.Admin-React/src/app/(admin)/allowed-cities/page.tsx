"use client";

import { useState } from "react";
import { MapPin, Plus, Pencil, Trash2, Loader2, Search } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  useAllowedCities,
  useCreateAllowedCity,
  useUpdateAllowedCity,
  usePatchAllowedCity,
  useDeleteAllowedCity,
} from "@/hooks/admin";
import type { AllowedCityDto } from "@/lib/types";

const brazilianStates = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
  "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
  "RS", "RO", "RR", "SC", "SP", "SE", "TO"
];

interface CityFormData {
  city: string;
  state: string;
  serviceRadiusKm: number;
  isActive: boolean;
}

const initialFormData: CityFormData = {
  city: "",
  state: "",
  serviceRadiusKm: 50,
  isActive: true,
};

export default function AllowedCitiesPage() {
  const [search, setSearch] = useState("");
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedCity, setSelectedCity] = useState<AllowedCityDto | null>(null);
  const [formData, setFormData] = useState<CityFormData>(initialFormData);

  const { data: citiesResponse, isLoading, error } = useAllowedCities();
  const createMutation = useCreateAllowedCity();
  const updateMutation = useUpdateAllowedCity();
  const patchMutation = usePatchAllowedCity();
  const deleteMutation = useDeleteAllowedCity();

  const cities = citiesResponse?.data?.value ?? [];

  const filteredCities = cities.filter(
    (c) =>
      (c.cityName?.toLowerCase() ?? "").includes(search.toLowerCase()) ||
      (c.stateSigla?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const handleOpenCreate = () => {
    setFormData(initialFormData);
    setIsCreateOpen(true);
  };

  const handleOpenEdit = (city: AllowedCityDto) => {
    setSelectedCity(city);
    setFormData({
      city: city.cityName ?? "",
      state: city.stateSigla ?? "",
      serviceRadiusKm: city.serviceRadiusKm ?? 50,
      isActive: city.isActive ?? true,
    });
    setIsEditOpen(true);
  };

  const handleOpenDelete = (city: AllowedCityDto) => {
    setSelectedCity(city);
    setIsDeleteOpen(true);
  };

  const handleSubmitCreate = async () => {
    await createMutation.mutateAsync({
      body: {
        city: formData.city,
        state: formData.state,
        country: "Brasil",
        serviceRadiusKm: formData.serviceRadiusKm,
        isActive: formData.isActive,
      },
    });
    setIsCreateOpen(false);
  };

  const handleSubmitEdit = async () => {
    if (!selectedCity?.id) return;
    await updateMutation.mutateAsync({
      id: selectedCity.id,
      data: {
        data: {
          cityName: formData.city,
          stateSigla: formData.state,
          serviceRadiusKm: formData.serviceRadiusKm,
          isActive: formData.isActive,
        },
      },
    });
    setIsEditOpen(false);
  };

  const handleToggleActive = async (city: AllowedCityDto) => {
    if (!city.id) return;
    await patchMutation.mutateAsync({
      id: city.id,
      data: {
        isActive: !city.isActive,
      },
    });
  };

  const handleDelete = async () => {
    if (!selectedCity?.id) return;
    await deleteMutation.mutateAsync(selectedCity.id);
    setIsDeleteOpen(false);
  };

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Cidades Permitidas</h1>
          <p className="text-muted-foreground">Gerencie cidades atendidas pelos prestadores</p>
        </div>
        <Button onClick={handleOpenCreate}>
          <Plus className="mr-2 h-4 w-4" />Nova Cidade
        </Button>
      </div>

      <Card className="mb-6">
        <div className="p-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar por cidade ou estado..."
              className="pl-10"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>
      </Card>

      <Card>
        {isLoading && (
          <div className="flex items-center justify-center p-8">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        )}

        {error && (
          <div className="p-8 text-center text-destructive">
            Erro ao carregar cidades. Tente novamente.
          </div>
        )}

        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border">
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Cidade</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Estado</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Raio de Serviço</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Status</th>
                  <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">Ações</th>
                </tr>
              </thead>
              <tbody>
                {filteredCities.map((city) => (
                  <tr key={city.id} className="border-b border-border last:border-b-0">
                    <td className="px-4 py-3 text-sm font-medium">{city.cityName ?? "-"}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">{city.stateSigla ?? "-"}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">{city.serviceRadiusKm ?? 50} km</td>
                    <td className="px-4 py-3">
                      <Badge variant={city.isActive ? "success" : "secondary"}>
                        {city.isActive ? "Ativa" : "Inativa"}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleToggleActive(city)}
                        >
                          <MapPin className={`h-4 w-4 ${city.isActive ? "text-green-500" : "text-gray-400"}`} />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={() => handleOpenEdit(city)}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={() => handleOpenDelete(city)}>
                          <Trash2 className="h-4 w-4 text-red-500" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {!isLoading && !error && filteredCities.length === 0 && (
          <div className="p-8 text-center text-muted-foreground">Nenhuma cidade encontrada</div>
        )}
      </Card>

      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Nova Cidade Permitida</DialogTitle>
            <DialogDescription>Adicione uma nova cidade para atendimento dos prestadores.</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="city" className="text-sm font-medium">Cidade</label>
              <Input
                id="city"
                value={formData.city}
                onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                placeholder="Ex: São Paulo"
              />
            </div>
            <div className="grid gap-2">
              <label htmlFor="state" className="text-sm font-medium">Estado</label>
              <select
                id="state"
                value={formData.state}
                onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              >
                <option value="">Selecione...</option>
                {brazilianStates.map((state) => (
                  <option key={state} value={state}>{state}</option>
                ))}
              </select>
            </div>
            <div className="grid gap-2">
              <label htmlFor="radius" className="text-sm font-medium">Raio de Serviço (km)</label>
              <Input
                id="radius"
                type="number"
                value={formData.serviceRadiusKm}
                onChange={(e) => setFormData({ ...formData, serviceRadiusKm: Number(e.target.value) })}
                min={1}
                max={500}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="isActive"
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="h-4 w-4"
              />
              <label htmlFor="isActive" className="text-sm font-medium">Cidade Ativa</label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsCreateOpen(false)}>Cancelar</Button>
            <Button onClick={handleSubmitCreate} disabled={createMutation.isPending}>
              {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Criar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Editar Cidade</DialogTitle>
            <DialogDescription>Atualize os dados da cidade permitida.</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="edit-city" className="text-sm font-medium">Cidade</label>
              <Input
                id="edit-city"
                value={formData.city}
                onChange={(e) => setFormData({ ...formData, city: e.target.value })}
              />
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-state" className="text-sm font-medium">Estado</label>
              <select
                id="edit-state"
                value={formData.state}
                onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              >
                <option value="">Selecione...</option>
                {brazilianStates.map((state) => (
                  <option key={state} value={state}>{state}</option>
                ))}
              </select>
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-radius" className="text-sm font-medium">Raio de Serviço (km)</label>
              <Input
                id="edit-radius"
                type="number"
                value={formData.serviceRadiusKm}
                onChange={(e) => setFormData({ ...formData, serviceRadiusKm: Number(e.target.value) })}
                min={1}
                max={500}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="edit-isActive"
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="h-4 w-4"
              />
              <label htmlFor="edit-isActive" className="text-sm font-medium">Cidade Ativa</label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsEditOpen(false)}>Cancelar</Button>
            <Button onClick={handleSubmitEdit} disabled={updateMutation.isPending}>
              {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Salvar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={isDeleteOpen} onOpenChange={setIsDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Excluir Cidade</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir a cidade <strong>{selectedCity?.cityName}</strong>?
              Esta ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDeleteOpen(false)}>Cancelar</Button>
            <Button variant="destructive" onClick={handleDelete} disabled={deleteMutation.isPending}>
              {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Excluir
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
