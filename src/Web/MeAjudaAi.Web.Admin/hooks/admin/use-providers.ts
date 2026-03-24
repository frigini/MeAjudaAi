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
  list: (filters?: ApiProvidersGet2Data["query"]) =>
    [...providerKeys.lists(), filters] as const,
  details: () => [...providerKeys.all, "detail"] as const,
  detail: (id: string) => [...providerKeys.details(), id] as const,
  byStatus: (status: string) =>
    [...providerKeys.all, "byStatus", status] as const,
  byType: (type: string) => [...providerKeys.all, "byType", type] as const,
};

export function useProviders(filters?: any) {
  return useQuery({
    queryKey: providerKeys.list(filters),
    queryFn: () => apiProvidersGet2({ query: filters } as any) as any,
    select: (data: any) => data.data ?? data,
  });
}

export function useProviderById(id: string) {
  return useQuery({
    queryKey: providerKeys.detail(id),
    queryFn: () => apiProvidersGet3({ path: { id } } as any) as any,
    select: (data: any) => data.data ?? data,
    enabled: !!id,
  });
}

export function useProvidersByStatus(status: string) {
  return useQuery({
    queryKey: providerKeys.byStatus(status),
    queryFn: () =>
      apiProvidersGet2({
        query: { verificationStatus: status },
      } as any) as any,
    select: (data: any) => data.data ?? data,
    enabled: !!status,
  });
}

export function useProvidersByType(type: string) {
  return useQuery({
    queryKey: providerKeys.byType(type),
    queryFn: () =>
      apiProvidersGet2({
        query: { type },
      } as any) as any,
    select: (data: any) => data.data ?? data,
    enabled: !!type,
  });
}

export function useCreateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiProvidersPost({ body: data } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useUpdateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiProvidersPut({ path: { id: data.id }, body: data.data } as any),
    onSuccess: (_, variables: any) => {
      queryClient.invalidateQueries({ queryKey: providerKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useDeleteProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiProvidersDelete({ path: { id } } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useActivateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiActivatePost({ path: { id } } as any),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: providerKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}

export function useDeactivateProvider() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiDeactivatePost({ path: { id } } as any),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: providerKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: providerKeys.lists() });
    },
  });
}
