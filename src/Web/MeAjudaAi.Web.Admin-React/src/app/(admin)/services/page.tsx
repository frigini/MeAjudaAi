"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Wrench, Plus, Pencil, Trash2, Loader2, Search, ChevronLeft, ChevronRight } from "lucide-react";
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

const ITEMS_PER_PAGE = 10;

const serviceSchema = z.object({
  name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres").max(100, "Nome deve ter no máximo 100 caracteres"),
  description: z.string().max(500, "Descrição deve ter no máximo 500 caracteres").optional(),
  categoryId: z.string().min(1, "Selecione uma categoria"),
  isActive: z.boolean(),
});

type ServiceFormData = z.infer<typeof serviceSchema>;

export default function ServicesPage() {
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedService, setSelectedService] = useState<{ id?: string; name?: string | null } | null>(null);

  const { data: servicesResponse, isLoading, error } = useServices();
  const { data: categoriesResponse } = useCategories();
  const createMutation = useCreateService();
  const updateMutation = useUpdateService();
  const deleteMutation = useDeleteService();

  const createForm = useForm<ServiceFormData>({
    resolver: zodResolver(serviceSchema),
    defaultValues: { name: "", description: "", categoryId: "", isActive: true },
  });

  const editForm = useForm<ServiceFormData>({
    resolver: zodResolver(serviceSchema),
    defaultValues: { name: "", description: "", categoryId: "", isActive: true },
  });

  const services = servicesResponse?.data?.data ?? [];
  const categories = categoriesResponse?.data?.data ?? [];

  const filteredServices = services.filter(
    (s) => (s.name?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const totalPages = Math.ceil(filteredServices.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const paginatedServices = filteredServices.slice(startIndex, startIndex + ITEMS_PER_PAGE);

  const getCategoryName = (categoryId?: string) => {
    const category = categories.find((c) => c.id === categoryId);
    return category?.name ?? "-";
  };

  const handleSearch = (value: string) => {
    setSearch(value);
    setCurrentPage(1);
  };

  const handleOpenCreate = () => {
    createForm.reset({ name: "", description: "", categoryId: "", isActive: true });
    setIsCreateOpen(true);
  };

  const handleOpenEdit = (service: { id?: string; name?: string | null; description?: string | null; categoryId?: string; isActive?: boolean }) => {
    setSelectedService(service);
    editForm.reset({
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

  const handleSubmitCreate = async (data: ServiceFormData) => {
    try {
      await createMutation.mutateAsync({
        body: {
          name: data.name,
          description: data.description ?? "",
          categoryId: data.categoryId,
          isActive: data.isActive,
        },
      });
      toast.success("Serviço criado com sucesso");
      setIsCreateOpen(false);
    } catch {
      toast.error("Erro ao criar serviço");
    }
  };

  const handleSubmitEdit = async (data: ServiceFormData) => {
    if (!selectedService?.id) return;
    try {
      await updateMutation.mutateAsync({
        id: selectedService.id,
        name: data.name,
        description: data.description ?? "",
        categoryId: data.categoryId,
        isActive: data.isActive,
      });
      toast.success("Serviço atualizado com sucesso");
      setIsEditOpen(false);
    } catch {
      toast.error("Erro ao atualizar serviço");
    }
  };

  const handleDelete = async () => {
    if (!selectedService?.id) return;
    try {
      await deleteMutation.mutateAsync(selectedService.id);
      toast.success("Serviço excluído com sucesso");
      setIsDeleteOpen(false);
    } catch {
      toast.error("Erro ao excluir serviço");
    }
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
              onChange={(e) => handleSearch(e.target.value)}
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
          <>
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
                  {paginatedServices.map((service) => (
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

            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t border-border px-4 py-3">
                <p className="text-sm text-muted-foreground">
                  Mostrando {startIndex + 1} - {Math.min(startIndex + ITEMS_PER_PAGE, filteredServices.length)} de {filteredServices.length}
                </p>
                <div className="flex items-center gap-2">
                  <Button variant="outline" size="icon" disabled={currentPage === 1} onClick={() => setCurrentPage((p) => p - 1)}>
                    <ChevronLeft className="h-4 w-4" />
                  </Button>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) pageNum = i + 1;
                    else if (currentPage <= 3) pageNum = i + 1;
                    else if (currentPage >= totalPages - 2) pageNum = totalPages - 4 + i;
                    else pageNum = currentPage - 2 + i;
                    return (
                      <Button key={pageNum} variant={currentPage === pageNum ? "default" : "outline"} size="icon" onClick={() => setCurrentPage(pageNum)}>
                        {pageNum}
                      </Button>
                    );
                  })}
                  <Button variant="outline" size="icon" disabled={currentPage === totalPages} onClick={() => setCurrentPage((p) => p + 1)}>
                    <ChevronRight className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
          </>
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
          <form onSubmit={createForm.handleSubmit(handleSubmitCreate)} className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="name" className="text-sm font-medium">Nome</label>
              <Input id="name" {...createForm.register("name")} placeholder="Ex: Instalação de Tomada" />
              {createForm.formState.errors.name && <p className="text-sm text-destructive">{createForm.formState.errors.name.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="category" className="text-sm font-medium">Categoria</label>
              <select id="category" {...createForm.register("categoryId")} className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm">
                <option value="">Selecione...</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>{category.name}</option>
                ))}
              </select>
              {createForm.formState.errors.categoryId && <p className="text-sm text-destructive">{createForm.formState.errors.categoryId.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="description" className="text-sm font-medium">Descrição</label>
              <Input id="description" {...createForm.register("description")} placeholder="Descrição do serviço..." />
              {createForm.formState.errors.description && <p className="text-sm text-destructive">{createForm.formState.errors.description.message}</p>}
            </div>
            <div className="flex items-center gap-2">
              <input id="isActive" type="checkbox" {...createForm.register("isActive")} className="h-4 w-4" />
              <label htmlFor="isActive" className="text-sm font-medium">Serviço Ativo</label>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>Cancelar</Button>
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
            <DialogTitle>Editar Serviço</DialogTitle>
            <DialogDescription>Atualize os dados do serviço.</DialogDescription>
          </DialogHeader>
          <form onSubmit={editForm.handleSubmit(handleSubmitEdit)} className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="edit-name" className="text-sm font-medium">Nome</label>
              <Input id="edit-name" {...editForm.register("name")} />
              {editForm.formState.errors.name && <p className="text-sm text-destructive">{editForm.formState.errors.name.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-category" className="text-sm font-medium">Categoria</label>
              <select id="edit-category" {...editForm.register("categoryId")} className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm">
                <option value="">Selecione...</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>{category.name}</option>
                ))}
              </select>
              {editForm.formState.errors.categoryId && <p className="text-sm text-destructive">{editForm.formState.errors.categoryId.message}</p>}
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-description" className="text-sm font-medium">Descrição</label>
              <Input id="edit-description" {...editForm.register("description")} />
              {editForm.formState.errors.description && <p className="text-sm text-destructive">{editForm.formState.errors.description.message}</p>}
            </div>
            <div className="flex items-center gap-2">
              <input id="edit-isActive" type="checkbox" {...editForm.register("isActive")} className="h-4 w-4" />
              <label htmlFor="edit-isActive" className="text-sm font-medium">Serviço Ativo</label>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsEditOpen(false)}>Cancelar</Button>
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
