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

export const categoryKeys = {
  all: ["categories"] as const,
  lists: () => [...categoryKeys.all, "list"] as const,
  list: (filters?: ApiCategoriesGetData["query"]) =>
    [...categoryKeys.lists(), filters] as const,
  details: () => [...categoryKeys.all, "detail"] as const,
  detail: (id: string) => [...categoryKeys.details(), id] as const,
};

export function useCategories() {
  return useQuery({
    queryKey: categoryKeys.lists(),
    queryFn: () => apiCategoriesGet() as any,
    select: (data: any) => data.data ?? data,
  });
}

export function useCategoryById(id: string) {
  return useQuery({
    queryKey: categoryKeys.detail(id),
    queryFn: () => apiCategoriesGet2({ path: { id } } as any),
    select: (data: any) => data.data ?? data,
    enabled: !!id,
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiCategoriesPost(data.body ? data : { body: data } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiCategoriesPut(data.path ? data : {
        path: { id: data.id },
        body: data.body ?? data,
      }),
    onSuccess: (_, variables: any) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(variables?.path?.id ?? variables.id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ["services"] });
    },
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiCategoriesDelete(data.path ? data : { path: { id: data } } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}
