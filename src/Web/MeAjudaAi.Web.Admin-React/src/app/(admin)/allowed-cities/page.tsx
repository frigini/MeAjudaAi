"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { MapPin, Plus, Pencil, Trash2, Loader2, Search, ChevronLeft, ChevronRight } from "lucide-react";
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

const ITEMS_PER_PAGE = 10;

const brazilianStates = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
  "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
  "RS", "RO", "RR", "SC", "SP", "SE", "TO"
];

const citySchema = z.object({
  city: z.string().min(2, "Cidade deve ter pelo menos 2 caracteres").max(100, "Cidade deve ter no máximo 100 caracteres"),
  state: z.string().min(1, "Selecione um estado"),
  serviceRadiusKm: z.coerce.number().min(1, "Raio deve ser pelo menos 1 km").max(500, "Raio máximo é 500 km"),
  isActive: z.boolean(),
});

type CityFormData = z.infer<typeof citySchema>;

export default function AllowedCitiesPage() {
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedCity, setSelectedCity] = useState<AllowedCityDto | null>(null);

  const { data: citiesResponse, isLoading, error } = useAllowedCities();
  const createMutation = useCreateAllowedCity();
  const updateMutation = useUpdateAllowedCity();
  const patchMutation = usePatchAllowedCity();
  const deleteMutation = useDeleteAllowedCity();

  const createForm = useForm<CityFormData>({
    resolver: zodResolver(citySchema),
    defaultValues: { city: "", state: "", serviceRadiusKm: 50, isActive: true },
  });

  const editForm = useForm<CityFormData>({
    resolver: zodResolver(citySchema),
    defaultValues: { city: "", state: "", serviceRadiusKm: 50, isActive: true },
  });

  const cities: AllowedCityDto[] = Array.isArray(citiesResponse)
    ? citiesResponse
    : (citiesResponse as { value?: AllowedCityDto[] })?.value ?? [];

  const filteredCities = cities.filter(
    (c: AllowedCityDto) =>
      (c.cityName?.toLowerCase() ?? "").includes(search.toLowerCase()) ||
      (c.stateSigla?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const unfilteredTotalPages = Math.ceil(filteredCities.length / ITEMS_PER_PAGE);
  const totalPages = Math.max(1, unfilteredTotalPages);
  const safePage = Math.min(currentPage, totalPages);
  const startIndex = (safePage - 1) * ITEMS_PER_PAGE;
  const paginatedCities = filteredCities.slice(startIndex, startIndex + ITEMS_PER_PAGE);

  const handleSearch = (value: string) => {
    setSearch(value);
    setCurrentPage(1);
  };

  const handleOpenCreate = () => {
    createForm.reset({ city: "", state: "", serviceRadiusKm: 50, isActive: true });
    setIsCreateOpen(true);
  };

  const handleOpenEdit = (city: AllowedCityDto) => {
    setSelectedCity(city);
    editForm.reset({
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

  const handleSubmitCreate = async (data: CityFormData) => {
    try {
      await createMutation.mutateAsync({
        body: {
          city: data.city,
          state: data.state,
          country: "Brasil",
          serviceRadiusKm: data.serviceRadiusKm,
          isActive: data.isActive,
        },
      });
      toast.success("Cidade criada com sucesso");
      setIsCreateOpen(false);
    } catch {
      toast.error("Erro ao criar cidade");
    }
  };

  const handleSubmitEdit = async (data: CityFormData) => {
    if (!selectedCity?.id) return;
    try {
      await updateMutation.mutateAsync({
        path: { id: selectedCity.id },
        body: {
          cityName: data.city,
          stateSigla: data.state,
          serviceRadiusKm: data.serviceRadiusKm,
          isActive: data.isActive,
        },
      });
      toast.success("Cidade atualizada com sucesso");
      setIsEditOpen(false);
    } catch {
      toast.error("Erro ao atualizar cidade");
    }
  };

  const handleToggleActive = async (city: AllowedCityDto) => {
    if (!city.id) return;
    try {
      await patchMutation.mutateAsync({
        path: { id: city.id },
        body: { isActive: !city.isActive },
      });
      toast.success(`Cidade ${!city.isActive ? "ativada" : "desativada"} com sucesso`);
    } catch {
      toast.error("Erro ao atualizar cidade");
    }
  };

  const handleDelete = async () => {
    if (!selectedCity?.id) return;
    try {
      await deleteMutation.mutateAsync({ path: { id: selectedCity.id } });
      toast.success("Cidade excluída com sucesso");
      setIsDeleteOpen(false);
    } catch {
      toast.error("Erro ao excluir cidade");
    }
  };

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Cidades Permitidas</h1>
          <p className="text-muted-foreground">Gerencie cidades atendidas pelos prestadores</p>
        </div>
        <Button onClick={handleOpenCreate} disabled title="Criação de cidades temporariamente desabilitada">
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
              onChange={(e) => handleSearch(e.target.value)}
              aria-label="Buscar por cidade ou estado"
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
          <>
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
                  {paginatedCities.map((city: AllowedCityDto) => (
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
                          <Button variant="ghost" size="icon" onClick={() => handleToggleActive(city)} aria-label={city.isActive ? "Desativar cidade" : "Ativar cidade"}>
                            <MapPin className={`h-4 w-4 ${city.isActive ? "text-green-500" : "text-gray-400"}`} />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => handleOpenEdit(city)} aria-label="Editar cidade">
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => handleOpenDelete(city)} aria-label="Excluir cidade">
                            <Trash2 className="h-4 w-4 text-red-500" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {unfilteredTotalPages > 1 && (
              <div className="flex items-center justify-between border-t border-border px-4 py-3">
                <p className="text-sm text-muted-foreground">
                  Mostrando {startIndex + 1} - {Math.min(startIndex + ITEMS_PER_PAGE, filteredCities.length)} de {filteredCities.length}
                </p>
                <div className="flex items-center gap-2">
                  <Button variant="secondary" size="icon" disabled={safePage === 1} onClick={() => setCurrentPage((p) => p - 1)} aria-label="Página anterior">
                    <ChevronLeft className="h-4 w-4" />
                  </Button>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) pageNum = i + 1;
                    else if (safePage <= 3) pageNum = i + 1;
                    else if (safePage >= totalPages - 2) pageNum = totalPages - 4 + i;
                    else pageNum = safePage - 2 + i;
                    return (
                      <Button key={pageNum} variant={safePage === pageNum ? "primary" : "secondary"} size="icon" onClick={() => setCurrentPage(pageNum)} aria-label={`Página ${pageNum}`}>
                        {pageNum}
                      </Button>
                    );
                  })}
                  <Button variant="secondary" size="icon" disabled={safePage === totalPages} onClick={() => setCurrentPage((p) => p + 1)} aria-label="Próxima página">
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
          </>
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
          <form onSubmit={createForm.handleSubmit(handleSubmitCreate)} className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="city" className="text-sm font-medium">Cidade</label>
              <Input id="city" autoFocus {...createForm.register("city")} placeholder="Ex: São Paulo" />
              {createForm.formState.errors.city && <p className="text-sm text-destructive">{createForm.formState.errors.city.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="state" className="text-sm font-medium">Estado</label>
              <select id="state" {...createForm.register("state")} className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm">
                <option value="">Selecione...</option>
                {brazilianStates.map((state) => (
                  <option key={state} value={state}>{state}</option>
                ))}
              </select>
              {createForm.formState.errors.state && <p className="text-sm text-destructive">{createForm.formState.errors.state.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="radius" className="text-sm font-medium">Raio de Serviço (km)</label>
              <Input id="radius" type="number" {...createForm.register("serviceRadiusKm")} min={1} max={500} />
              {createForm.formState.errors.serviceRadiusKm && <p className="text-sm text-destructive">{createForm.formState.errors.serviceRadiusKm.message}</p>}
            </div>
            <div className="flex items-center gap-2">
              <input id="isActive" type="checkbox" {...createForm.register("isActive")} className="h-4 w-4" />
              <label htmlFor="isActive" className="text-sm font-medium">Cidade Ativa</label>
            </div>
            <DialogFooter>
              <Button type="button" variant="secondary" onClick={() => setIsCreateOpen(false)}>Cancelar</Button>
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Criar
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Editar Cidade</DialogTitle>
            <DialogDescription>Atualize os dados da cidade permitida.</DialogDescription>
          </DialogHeader>
          <form onSubmit={editForm.handleSubmit(handleSubmitEdit)} className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="edit-city" className="text-sm font-medium">Cidade</label>
              <Input id="edit-city" {...editForm.register("city")} />
              {editForm.formState.errors.city && <p className="text-sm text-destructive">{editForm.formState.errors.city.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-state" className="text-sm font-medium">Estado</label>
              <select id="edit-state" {...editForm.register("state")} className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm">
                <option value="">Selecione...</option>
                {brazilianStates.map((state) => (
                  <option key={state} value={state}>{state}</option>
                ))}
              </select>
              {editForm.formState.errors.state && <p className="text-sm text-destructive">{editForm.formState.errors.state.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-radius" className="text-sm font-medium">Raio de Serviço (km)</label>
              <Input id="edit-radius" type="number" {...editForm.register("serviceRadiusKm")} min={1} max={500} />
              {editForm.formState.errors.serviceRadiusKm && <p className="text-sm text-destructive">{editForm.formState.errors.serviceRadiusKm.message}</p>}
            </div>
            <div className="flex items-center gap-2">
              <input id="edit-isActive" type="checkbox" {...editForm.register("isActive")} className="h-4 w-4" />
              <label htmlFor="edit-isActive" className="text-sm font-medium">Cidade Ativa</label>
            </div>
            <DialogFooter>
              <Button type="button" variant="secondary" onClick={() => setIsEditOpen(false)}>Cancelar</Button>
              <Button type="submit" disabled={updateMutation.isPending}>
                {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Salvar
              </Button>
            </DialogFooter>
          </form>
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
            <Button variant="secondary" onClick={() => setIsDeleteOpen(false)}>Cancelar</Button>
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
