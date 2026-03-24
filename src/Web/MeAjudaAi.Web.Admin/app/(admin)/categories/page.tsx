"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Plus, Pencil, Trash2, Loader2, Search } from "lucide-react";
import { Card } from "@/components/ui/card";
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
  useCategories,
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategory,
  useActivateCategory,
  useDeactivateCategory,
} from "@/hooks/admin";
import type { ServiceCategoryDto } from "@/lib/types";
import { CATEGORY_STATUS_LABELS } from "@/lib/types";

const categorySchema = z.object({
  name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres").max(100, "Nome deve ter no máximo 100 caracteres"),
  description: z.string().max(500, "Descrição deve ter no máximo 500 caracteres").optional(),
  displayOrder: z.number().int().min(0, "Ordem deve ser >= 0").optional(),
});

type CategoryFormData = z.infer<typeof categorySchema>;

export default function CategoriesPage() {
  const [search, setSearch] = useState("");
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<ServiceCategoryDto | null>(null);
  const [togglingId, setTogglingId] = useState<string | null>(null);

  const { data: categoriesResponse, isLoading, error } = useCategories();
  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const deleteMutation = useDeleteCategory();
  const activateMutation = useActivateCategory();
  const deactivateMutation = useDeactivateCategory();

  const createForm = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: { name: "", description: "", displayOrder: 0 },
  });

  const editForm = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: { name: "", description: "", displayOrder: 0 },
  });

  const categories: ServiceCategoryDto[] = categoriesResponse ?? [];

  const filteredCategories = categories.filter(
    (c: ServiceCategoryDto) => (c.name?.toLowerCase() ?? "").includes(search.toLowerCase())
  );

  const handleOpenCreate = () => {
    createForm.reset({ name: "", description: "", displayOrder: 0 });
    setIsCreateOpen(true);
  };

  const handleOpenEdit = (category: ServiceCategoryDto) => {
    setSelectedCategory(category);
    editForm.reset({
      name: category.name ?? "",
      description: category.description ?? "",
      displayOrder: category.displayOrder ?? 0,
    });
    setIsEditOpen(true);
  };

  const handleOpenDelete = (category: ServiceCategoryDto) => {
    setSelectedCategory(category);
    setIsDeleteOpen(true);
  };

  const handleSubmitCreate = async (data: CategoryFormData) => {
    try {
      await createMutation.mutateAsync({
        name: data.name,
        description: data.description ?? "",
        displayOrder: data.displayOrder ?? 0,
      });
      toast.success("Categoria criada com sucesso");
      setIsCreateOpen(false);
    } catch {
      toast.error("Erro ao criar categoria");
    }
  };

  const handleSubmitEdit = async (data: CategoryFormData) => {
    if (!selectedCategory?.id) return;
    try {
      await updateMutation.mutateAsync({
        id: selectedCategory.id,
        name: data.name,
        description: data.description ?? "",
        displayOrder: data.displayOrder,
      });
      toast.success("Categoria atualizada com sucesso");
      setIsEditOpen(false);
    } catch {
      toast.error("Erro ao atualizar categoria");
    }
  };

  const handleDelete = async () => {
    if (!selectedCategory?.id) return;
    try {
      await deleteMutation.mutateAsync(selectedCategory.id);
      toast.success("Categoria excluída com sucesso");
      setIsDeleteOpen(false);
    } catch {
      toast.error("Erro ao excluir categoria");
    }
  };

  return (
    <div className="p-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Categorias</h1>
          <p className="text-muted-foreground">Gerencie categorias de serviços</p>
        </div>
        <Button onClick={handleOpenCreate}>
          <Plus className="mr-2 h-4 w-4" />Nova Categoria
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
              aria-label="Buscar categorias por nome"
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
            Erro ao carregar categorias. Tente novamente.
          </div>
        )}

        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border">
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Nome</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Descrição</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Ordem</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">Status</th>
                  <th className="px-4 py-3 text-right text-sm font-medium text-muted-foreground">Ações</th>
                </tr>
              </thead>
              <tbody>
                {filteredCategories.map((category: ServiceCategoryDto) => (
                  <tr key={category.id} className="border-b border-border last:border-b-0">
                    <td className="px-4 py-3 text-sm font-medium">{category.name ?? "-"}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground max-w-[300px] truncate">
                      {category.description ?? "-"}
                    </td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">
                      {category.displayOrder ?? 0}
                    </td>
                    <td className="px-4 py-3">
                      <Badge 
                        variant={category.isActive ? CATEGORY_STATUS_LABELS.ACTIVE.variant : CATEGORY_STATUS_LABELS.INACTIVE.variant}
                        className={`cursor-pointer hover:opacity-80 ${togglingId === category.id ? "opacity-50 pointer-events-none" : ""}`}
                        onClick={async () => {
                          if (!category.id || togglingId) return;
                          setTogglingId(category.id);
                          try {
                            if (category.isActive) {
                              await deactivateMutation.mutateAsync(category.id);
                              toast.success("Categoria desativada");
                            } else {
                              await activateMutation.mutateAsync(category.id);
                              toast.success("Categoria ativada");
                            }
                          } catch {
                            toast.error("Erro ao alterar status");
                          } finally {
                            setTogglingId(null);
                          }
                        }}
                      >
                        {category.isActive ? CATEGORY_STATUS_LABELS.ACTIVE.label : CATEGORY_STATUS_LABELS.INACTIVE.label}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-2">
                        <Button variant="ghost" size="icon" onClick={() => handleOpenEdit(category)} aria-label="Editar categoria">
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={() => handleOpenDelete(category)} aria-label="Excluir categoria">
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

        {!isLoading && !error && filteredCategories.length === 0 && (
          <div className="p-8 text-center text-muted-foreground">Nenhuma categoria encontrada</div>
        )}
      </Card>

      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Nova Categoria</DialogTitle>
            <DialogDescription>Adicione uma nova categoria de serviço.</DialogDescription>
          </DialogHeader>
          <form onSubmit={createForm.handleSubmit(handleSubmitCreate)} className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="name" className="text-sm font-medium">Nome</label>
              <Input
                id="name"
                {...createForm.register("name")}
                placeholder="Ex: Eletricista"
              />
              {createForm.formState.errors.name && (
                <p className="text-sm text-destructive">{createForm.formState.errors.name.message}</p>
              )}
            </div>
            <div className="grid gap-2">
              <label htmlFor="description" className="text-sm font-medium">Descrição</label>
              <Input
                id="description"
                {...createForm.register("description")}
                placeholder="Descrição da categoria..."
              />
              {createForm.formState.errors.description && (
                <p className="text-sm text-destructive">{createForm.formState.errors.description.message}</p>
              )}
            </div>
            <div className="grid gap-2">
              <label htmlFor="displayOrder" className="text-sm font-medium">Ordem de Exibição</label>
              <Input
                id="displayOrder"
                type="number"
                {...createForm.register("displayOrder", { valueAsNumber: true })}
                placeholder="0"
              />
              {createForm.formState.errors.displayOrder && (
                <p className="text-sm text-destructive">{createForm.formState.errors.displayOrder.message}</p>
              )}
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
            <DialogTitle>Editar Categoria</DialogTitle>
            <DialogDescription>Atualize os dados da categoria.</DialogDescription>
          </DialogHeader>
          <form onSubmit={editForm.handleSubmit(handleSubmitEdit)} className="grid gap-4 py-4">
            <div className="grid gap-2">
              <label htmlFor="edit-name" className="text-sm font-medium">Nome</label>
              <Input
                id="edit-name"
                {...editForm.register("name")}
              />
              {editForm.formState.errors.name && (
                <p className="text-sm text-destructive">{editForm.formState.errors.name.message}</p>
              )}
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-description" className="text-sm font-medium">Descrição</label>
              <Input
                id="edit-description"
                {...editForm.register("description")}
              />
              {editForm.formState.errors.description && (
                <p className="text-sm text-destructive">{editForm.formState.errors.description.message}</p>
              )}
            </div>
            <div className="grid gap-2">
              <label htmlFor="edit-displayOrder" className="text-sm font-medium">Ordem de Exibição</label>
              <Input
                id="edit-displayOrder"
                type="number"
                {...editForm.register("displayOrder", { valueAsNumber: true })}
                placeholder="0"
              />
              {editForm.formState.errors.displayOrder && (
                <p className="text-sm text-destructive">{editForm.formState.errors.displayOrder.message}</p>
              )}
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
            <DialogTitle>Excluir Categoria</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir a categoria <strong>{selectedCategory?.name}</strong>?
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
