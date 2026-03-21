"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiServicesGet,
  apiServicesGet2,
  apiServicesPost,
  apiServicesPut,
  apiServicesDelete,
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
    queryKey: serviceKeys.list({ categoryId }),
    queryFn: () => apiServicesGet({ query: { categoryId } } as ApiServicesGetData),
    select: (data) => data.data,
  });
}

export function useServiceById(id: string) {
  return useQuery({
    queryKey: serviceKeys.detail(id),
    queryFn: () => apiServicesGet2({ path: { id } } as ApiServicesGet2Data),
    select: (data) => data.data,
    enabled: !!id,
  });
}

export function useCreateService() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ApiServicesPostData["body"]) =>
      apiServicesPost({ body: data }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: serviceKeys.lists() });
    },
  });
}

export function useUpdateService() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...body }: { id: string } & ApiServicesPutData["body"]) =>
      apiServicesPut({
        path: { id },
        body: body as ApiServicesPutData["body"],
      }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: serviceKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: serviceKeys.lists() });
    },
  });
}

export function useDeleteService() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiServicesDelete({ path: { id } } as ApiServicesDeleteData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: serviceKeys.lists() });
    },
  });
}
