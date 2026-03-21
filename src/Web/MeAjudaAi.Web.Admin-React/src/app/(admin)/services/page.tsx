"use client";

import { useState } from "react";
import { Wrench, Plus, Pencil, Trash2, Loader2, Search } from "lucide-react";
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
  useServices,
  useCategories,
  useCreateService,
  useUpdateService,
  useDeleteService,
} from "@/hooks/admin";

interface ServiceFormData {
  name: string;
  description: string;
  categoryId: string;
  isActive: boolean;
}

const initialFormData: ServiceFormData = {
  name: "",
  description: "",
  categoryId: "",
  isActive: true,
};

export default function ServicesPage() {
  const [search, setSearch] = useState("");
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedService, setSelectedService] = useState<{ id?: string; name?: string | null } | null>(null);
  const [formData, setFormData] = useState<ServiceFormData>(initialFormData);

  const { data: servicesResponse, isLoading, error } = useServices();
  const { data: categoriesResponse } = useCategories();
  const createMutation = useCreateService();
  const updateMutation = useUpdateService();
  const deleteMutation = useDeleteService();

  const services = servicesResponse?.data?.data ?? [];
  const categories = categoriesResponse?.data?.data ?? [];

  const filteredServices = services.filter(
    (s) => (s.name?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const getCategoryName = (categoryId?: string) => {
    const category = categories.find((c) => c.id === categoryId);
    return category?.name ?? "-";
  };

  const handleOpenCreate = () => {
    setFormData(initialFormData);
    setIsCreateOpen(true);
  };

  const handleOpenEdit = (service: { id?: string; name?: string | null; description?: string | null; categoryId?: string; isActive?: boolean }) => {
    setSelectedService(service);
    setFormData({
      name: service.name ?? "",
      description: service.description ?? "",
      categoryId: service.categoryId ?? "",
      isActive: service.isActive ?? true,
    });
    setIsEditOpen(true);
  };

  const handleOpenDelete = (service: { id?: string; name?: string | null }) => {
    setSelectedService(service);
    setIsDeleteOpen(true);
  };

  const handleSubmitCreate = async () => {
    await createMutation.mutateAsync({
      body: {
        name: formData.name,
        description: formData.description,
        categoryId: formData.categoryId,
        isActive: formData.isActive,
      },
    });
    setIsCreateOpen(false);
  };

  const handleSubmitEdit = async () => {
    if (!selectedService?.id) return;
    await updateMutation.mutateAsync({
      id: selectedService.id,
      name: formData.name,
      description: formData.description,
      categoryId: formData.categoryId,
      isActive: formData.isActive,
    });
    setIsEditOpen(false);
  };

  const handleDelete = async () => {
    if (!selectedService?.id) return;
    await deleteMutation.mutateAsync(selectedService.id);
    setIsDeleteOpen(false);
  };

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Serviços</h1>
          <p className="text-muted-foreground">Gerencie serviços disponíveis</p>
        </div>
        <Button onClick={handleOpenCreate}>
          <Plus className="mr-2 h-4 w-4" />Novo Serviço
        </Button>
      </div>

      <Card className="mb-6">
        <div className="p-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar por nome..."
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
            Erro ao carregar serviços. Tente novamente.
          </div>
        )}

        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border">
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Nome</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Categoria</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Descrição</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Status</th>
                  <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">Ações</th>
                </tr>
              </thead>
              <tbody>
                {filteredServices.map((service) => (
                  <tr key={service.id} className="border-b border-border last:border-b-0">
                    <td className="px-4 py-3 text-sm font-medium">{service.name ?? "-"}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">
                      {getCategoryName(service.categoryId)}
                    </td>
                    <td className="px-4 py-3 text-sm text-muted-foreground max-w-[300px] truncate">
                      {service.description ?? "-"}
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant={service.isActive ? "success" : "secondary"}>
                        {service.isActive ? "Ativo" : "Inativo"}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-2">
                        <Button variant="ghost" size="icon" onClick={() => handleOpenEdit(service)}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={() => handleOpenDelete(service)}>
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

        {!isLoading && !error && filteredServices.length === 0 && (
          <div className="p-8 text-center text-muted-foreground">Nenhum serviço encontrado</div>
        )}
      </Card>

      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Novo Serviço</DialogTitle>
            <DialogDescription>Adicione um novo serviço.</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="name" className="text-sm font-medium">Nome</label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Ex: Instalação de Tomada"
              />
            </div>
            <div className="grid gap-2">
              <label htmlFor="category" className="text-sm font-medium">Categoria</label>
              <select
                id="category"
                value={formData.categoryId}
                onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              >
                <option value="">Selecione...</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>{category.name}</option>
                ))}
              </select>
            </div>
            <div className="grid gap-2">
              <label htmlFor="description" className="text-sm font-medium">Descrição</label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Descrição do serviço..."
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
              <label htmlFor="isActive" className="text-sm font-medium">Serviço Ativo</label>
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
            <DialogTitle>Editar Serviço</DialogTitle>
            <DialogDescription>Atualize os dados do serviço.</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="edit-name" className="text-sm font-medium">Nome</label>
              <Input
                id="edit-name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-category" className="text-sm font-medium">Categoria</label>
              <select
                id="edit-category"
                value={formData.categoryId}
                onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              >
                <option value="">Selecione...</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>{category.name}</option>
                ))}
              </select>
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-description" className="text-sm font-medium">Descrição</label>
              <Input
                id="edit-description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
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
              <label htmlFor="edit-isActive" className="text-sm font-medium">Serviço Ativo</label>
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
            <DialogTitle>Excluir Serviço</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir o serviço <strong>{selectedService?.name}</strong>?
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
