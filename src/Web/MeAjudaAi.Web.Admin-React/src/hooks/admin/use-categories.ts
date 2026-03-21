"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiCategoriesGet,
  apiCategoriesGet2,
  apiCategoriesPost,
  apiCategoriesPut,
  apiCategoriesDelete,
} from "@/lib/api/generated";
import type {
  ApiCategoriesGetData,
  ApiCategoriesGet2Data,
  ApiCategoriesPostData,
  ApiCategoriesPutData,
  ApiCategoriesDeleteData,
} from "@/lib/api/generated";
import type { ServiceCategoryDto } from "@/lib/types";

type CategoryCreateInput = {
  name: string;
  description?: string;
  displayOrder?: number;
};

type CategoryUpdateInput = {
  id: string;
  name: string;
  description?: string;
  displayOrder?: number;
};

export const categoryKeys = {
  all: ["categories"] as const,
  lists: () => [...categoryKeys.all, "list"] as const,
  list: (filters?: ApiCategoriesGetData["query"]) =>
    [...categoryKeys.lists(), filters] as const,
  details: () => [...categoryKeys.all, "detail"] as const,
  detail: (id: string) => [...categoryKeys.details(), id] as const,
};

function normalizeCategoriesResponse(data: unknown): ServiceCategoryDto[] {
  if (!data) return [];
  if (Array.isArray(data)) return data as ServiceCategoryDto[];
  if ("data" in (data as object)) {
    const d = data as { data?: ServiceCategoryDto[] };
    return d.data ?? [];
  }
  return [];
}

export function useCategories() {
  return useQuery({
    queryKey: categoryKeys.lists(),
    queryFn: () => apiCategoriesGet(),
    select: (data) => normalizeCategoriesResponse(data),
  });
}

export function useCategoryById(id: string) {
  return useQuery({
    queryKey: categoryKeys.detail(id),
    queryFn: () => apiCategoriesGet2({ path: { id } }),
    select: (data) => {
      if (!data) return undefined;
      if ("data" in (data as object)) {
        return (data as { data?: ServiceCategoryDto }).data;
      }
      return data as ServiceCategoryDto;
    },
    enabled: !!id,
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CategoryCreateInput) =>
      apiCategoriesPost({
        body: {
          name: input.name,
          description: input.description ?? null,
          displayOrder: input.displayOrder ?? 0,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CategoryUpdateInput) =>
      apiCategoriesPut({
        path: { id: input.id },
        body: {
          name: input.name,
          description: input.description ?? null,
          displayOrder: input.displayOrder ?? 0,
        },
      }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ["services"] });
    },
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiCategoriesDelete({ path: { id } }),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ["services"] });
    },
  });
}
