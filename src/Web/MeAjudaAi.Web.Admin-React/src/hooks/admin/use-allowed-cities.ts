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
    queryFn: () => apiAllowedCitiesGet({ query: { onlyActive } }),
    select: (data) => data.data,
  });
}

export function useAllowedCityById(id: string) {
  return useQuery({
    queryKey: allowedCitiesKeys.detail(id),
    queryFn: () => apiAllowedCitiesGet2({ path: { id } } as ApiAllowedCitiesGet2Data),
    select: (data) => data.data,
    enabled: !!id,
  });
}

export function useCreateAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ApiAllowedCitiesPostData["body"]) =>
      apiAllowedCitiesPost({ body: data }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}

export function useUpdateAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      ...data
    }: ApiAllowedCitiesPutData["path"] & { data: ApiAllowedCitiesPutData["body"] }) =>
      apiAllowedCitiesPut({
        path: { id },
        body: data.data as ApiAllowedCitiesPutData["body"],
      }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}

export function usePatchAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      ...data
    }: ApiAllowedCitiesPatchData["path"] & { data: ApiAllowedCitiesPatchData["body"] }) =>
      apiAllowedCitiesPatch({
        path: { id },
        body: data.data as ApiAllowedCitiesPatchData["body"],
      }),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}

export function useDeleteAllowedCity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiAllowedCitiesDelete({ path: { id } } as ApiAllowedCitiesDeleteData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: allowedCitiesKeys.lists() });
    },
  });
}
