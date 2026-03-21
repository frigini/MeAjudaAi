"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiProvidersGet,
  apiProviderGet,
  apiProvidersPost,
  apiProvidersPut,
  apiProvidersDelete,
  apiProvidersGet2,
  apiProvidersGet3,
  apiActivatePost,
  apiDeactivatePost,
} from "@/lib/api/generated";
import type {
  ApiProvidersGetData,
  ApiProvidersGet2Data,
  ApiProvidersGet3Data,
  ApiProviderGetData,
  ApiProvidersPostData,
  ApiProvidersPutData,
  ApiProvidersDeleteData,
  ApiActivatePostData,
  ApiDeactivatePostData,
} from "@/lib/api/generated";
import type { ApiProvidersGet2Error, ApiProvidersGet4Error } from "@/lib/api/generated";

export const providerKeys = {
  all: ["providers"] as const,
  lists: () => [...providerKeys.all, "list"] as const,
  list: (filters: ApiProvidersGetData["query"]) =>
    [...providerKeys.lists(), filters] as const,
  details: () => [...providerKeys.all, "detail"] as const,
  detail: (id: string) => [...providerKeys.details(), id] as const,
  byStatus: (status: string) =>
    [...providerKeys.all, "byStatus", status] as const,
  byType: (type: string) => [...providerKeys.all, "byType", type] as const,
};

export function useProviders(filters?: ApiProvidersGetData["query"]) {
  return useQuery({
    queryKey: providerKeys.list(filters),
    queryFn: () => apiProvidersGet({ query: filters }),
    select: (data) => data.data,
  });
}

export function useProviderById(id: string) {
  return useQuery({
    queryKey: providerKeys.detail(id),
    queryFn: () => apiProviderGet({ path: { id } }),
    select: (data) => data.data,
    enabled: !!id,
  });
}

export function useProvidersByStatus(status: string) {
  return useQuery({
    queryKey: providerKeys.byStatus(status),
    queryFn: () =>
      apiProvidersGet2({
        query: { verificationStatus: status },
      } as ApiProvidersGet2Data),
    select: (data) => data.data,
    enabled: !!status,
  });
}

export function useProvidersByType(type: string) {
  return useQuery({
    queryKey: providerKeys.byType(type),
    queryFn: () =>
      apiProvidersGet3({
        query: { type },
      } as ApiProvidersGet3Data),
    select: (data) => data.data,
    enabled: !!type,
  });
}

export function useCreateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ApiProvidersPostData["body"]) =>
      apiProvidersPost({ body: data }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useUpdateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      ...data
    }: ApiProvidersPutData["path"] & { data: ApiProvidersPutData["body"] }) =>
      apiProvidersPut({ path: { id }, body: data.data as ApiProvidersPutData["body"] }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: providerKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useDeleteProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiProvidersDelete({ path: { id } } as ApiProvidersDeleteData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useActivateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiActivatePost({ path: { id } } as ApiActivatePostData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useDeactivateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiDeactivatePost({ path: { id } } as ApiDeactivatePostData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}
