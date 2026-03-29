"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiCategoriesGet,
  apiCategoriesGet2,
  apiCategoriesPost,
  apiCategoriesPut,
  apiCategoriesDelete,
  apiActivatePost2,
  apiDeactivatePost2,
} from "@/lib/api/generated";
import type {
  ApiCategoriesGetData,
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

function isDataPayload(obj: unknown): obj is { data?: unknown } {
  return obj !== null && typeof obj === "object" && "data" in obj;
}

function isServiceCategoryDto(obj: unknown): obj is ServiceCategoryDto {
  return obj !== null && typeof obj === 'object' && 'id' in obj && 'name' in obj;
}

function normalizeCategoriesResponse(response: unknown): ServiceCategoryDto[] {
  if (!response) return [];
  
  if (isDataPayload(response)) {
    const inner = response.data;
    if (Array.isArray(inner)) return inner as ServiceCategoryDto[];
    if (isDataPayload(inner) && Array.isArray(inner.data)) return inner.data as ServiceCategoryDto[];
  }
  
  if (Array.isArray(response)) return response as ServiceCategoryDto[];
  return [];
}

export function useCategories() {
  return useQuery({
    queryKey: categoryKeys.lists(),
    queryFn: () => apiCategoriesGet(),
    select: (res) => normalizeCategoriesResponse(res),
  });
}

export function useCategoryById(id: string) {
  return useQuery({
    queryKey: categoryKeys.detail(id),
    queryFn: () => apiCategoriesGet2({ path: { id } }),
    select: (res) => {
      if (!res) return undefined;
      if (isDataPayload(res)) {
        const inner = res.data;
        if (isDataPayload(inner) && isServiceCategoryDto(inner.data)) return inner.data;
        if (isServiceCategoryDto(inner)) return inner;
      }
      if (isServiceCategoryDto(res)) return res;
      return undefined;
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

export function useActivateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiActivatePost2({ path: { id } }),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}

export function useDeactivateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiDeactivatePost2({ path: { id } }),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}
