"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiServicesGet,
  apiServicesGet2,
  apiServicesPost,
  apiServicesPut,
  apiServicesDelete,
  apiCategoryGet,
} from "@/lib/api/generated";
import type {
  ApiServicesGetData,
  ApiServicesGet2Data,
  ApiServicesPostData,
  ApiServicesPutData,
  ApiServicesDeleteData,
} from "@/lib/api/generated";

export const serviceKeys = {
  all: ["services"] as const,
  lists: () => [...serviceKeys.all, "list"] as const,
  list: (filters?: ApiServicesGetData["query"]) =>
    [...serviceKeys.lists(), filters] as const,
  details: () => [...serviceKeys.all, "detail"] as const,
  detail: (id: string) => [...serviceKeys.details(), id] as const,
};

export function useServices(categoryId?: string) {
  return useQuery({
    queryKey: serviceKeys.list({ categoryId } as any),
    queryFn: () => categoryId ? apiCategoryGet({ path: { categoryId } } as any) : apiServicesGet(),
    select: (data: any) => data.data ?? data,
  });
}

export function useServiceById(id: string) {
  return useQuery({
    queryKey: serviceKeys.detail(id),
    queryFn: () => apiServicesGet2({ path: { id } } as any) as any,
    select: (data: any) => data.data ?? data,
    enabled: !!id,
  });
}

export function useCreateService() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiServicesPost(data.body ? data : { body: data } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: serviceKeys.lists() });
    },
  });
}

export function useUpdateService() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiServicesPut(data.path ? data : {
        path: { id: data.id },
        body: data.body ?? data,
      } as any),
    onSuccess: (_, variables: any) => {
      queryClient.invalidateQueries({ queryKey: serviceKeys.detail(variables?.path?.id ?? variables.id) });
      queryClient.invalidateQueries({ queryKey: serviceKeys.lists() });
    },
  });
}

export function useDeleteService() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiServicesDelete(data.path ? data : { path: { id: data } } as any),
    onSuccess: (_, id: any) => {
      queryClient.invalidateQueries({ queryKey: serviceKeys.detail(id?.path?.id ?? id) });
      queryClient.invalidateQueries({ queryKey: serviceKeys.lists() });
    },
  });
}
