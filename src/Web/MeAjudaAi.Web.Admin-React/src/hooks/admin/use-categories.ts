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
  isActive: boolean;
};

type CategoryUpdateInput = {
  id: string;
  name: string;
  description?: string;
  displayOrder?: number;
  isActive: boolean;
};

export const categoryKeys = {
  all: ["categories"] as const,
  lists: () => [...categoryKeys.all, "list"] as const,
  list: (filters?: ApiCategoriesGetData["query"]) =>
    [...categoryKeys.lists(), filters] as const,
  details: () => [...categoryKeys.all, "detail"] as const,
  detail: (id: string) => [...categoryKeys.details(), id] as const,
};

function isDataPayload(obj: unknown): obj is { data?: ServiceCategoryDto[] } {
  return obj !== null && typeof obj === "object" && "data" in obj;
}

function normalizeCategoriesResponse(data: unknown): ServiceCategoryDto[] {
  if (!data) return [];
  if (Array.isArray(data)) return data as ServiceCategoryDto[];
  if (isDataPayload(data)) {
    return data.data ?? [];
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
      if (data !== null && typeof data === "object" && "data" in data) {
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
          isActive: input.isActive,
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
    mutationFn: (input: CategoryUpdateInput) => {
      const body: {
        name: string;
        description: string | null;
        displayOrder?: number;
      } = {
        name: input.name,
        description: input.description ?? null,
      };
      if (input.displayOrder !== undefined) {
        body.displayOrder = input.displayOrder;
      }
      return apiCategoriesPut({
        path: { id: input.id },
        body,
      });
    },
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
