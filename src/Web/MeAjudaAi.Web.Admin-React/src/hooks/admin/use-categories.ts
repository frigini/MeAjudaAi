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
    queryFn: () => apiCategoriesGet(),
    select: (data) => data.data,
  });
}

export function useCategoryById(id: string) {
  return useQuery({
    queryKey: categoryKeys.detail(id),
    queryFn: () => apiCategoriesGet2({ path: { id } } as ApiCategoriesGet2Data),
    select: (data) => data.data,
    enabled: !!id,
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ApiCategoriesPostData["body"]) =>
      apiCategoriesPost({ body: data }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      ...data
    }: ApiCategoriesPutData["path"] & { data: ApiCategoriesPutData["body"] }) =>
      apiCategoriesPut({
        path: { id },
        body: data.data as ApiCategoriesPutData["body"],
      }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiCategoriesDelete({ path: { id } } as ApiCategoriesDeleteData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
    },
  });
}
