"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiAllowedCitiesGet,
  apiAllowedCitiesGet2,
  apiAllowedCitiesPost,
  apiAllowedCitiesPut,
  apiAllowedCitiesPatch,
  apiAllowedCitiesDelete,
} from "@/lib/api/generated";
import type {
  ApiAllowedCitiesGetData,
  ApiAllowedCitiesGet2Data,
  ApiAllowedCitiesPostData,
  ApiAllowedCitiesPutData,
  ApiAllowedCitiesPatchData,
  ApiAllowedCitiesDeleteData,
} from "@/lib/api/generated";

export const allowedCitiesKeys = {
  all: ["allowedCities"] as const,
  lists: () => [...allowedCitiesKeys.all, "list"] as const,
  list: (filters?: ApiAllowedCitiesGetData["query"]) =>
    [...allowedCitiesKeys.lists(), filters] as const,
  details: () => [...allowedCitiesKeys.all, "detail"] as const,
  detail: (id: string) => [...allowedCitiesKeys.details(), id] as const,
};

export function useAllowedCities(onlyActive?: boolean) {
  return useQuery({
    queryKey: allowedCitiesKeys.list({ onlyActive }),
    queryFn: () => apiAllowedCitiesGet({ query: { onlyActive } } as any),
    select: (data: any) => data.data ?? data,
  });
}

export function useAllowedCityById(id: string) {
  return useQuery({
    queryKey: allowedCitiesKeys.detail(id),
    queryFn: () => apiAllowedCitiesGet2({ path: { id } } as any),
    select: (data: any) => data.data ?? data,
    enabled: !!id,
  });
}

export function useCreateAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiAllowedCitiesPost(data.body ? data : { body: data } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}

export function useUpdateAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiAllowedCitiesPut(data.path ? data : {
        path: { id: data.id },
        body: data.body ?? data,
      }),
    onSuccess: (_, variables: any) => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.detail(variables?.path?.id ?? variables.id) });
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}

export function usePatchAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiAllowedCitiesPatch(data.path ? data : {
        path: { id: data.id },
        body: data.body ?? data,
      }),
    onSuccess: (_, variables: any) => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.detail(variables?.path?.id ?? variables.id) });
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}

export function useDeleteAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) =>
      apiAllowedCitiesDelete(data.path ? data : { path: { id: data } } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}
