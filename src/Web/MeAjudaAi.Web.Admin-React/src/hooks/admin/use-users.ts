"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  apiUsersGet,
  apiUsersGet2,
  apiUsersPost,
  apiUsersDelete,
} from "@/lib/api/generated";
import type {
  ApiUsersGetData,
  ApiUsersGet2Data,
  ApiUsersPostData,
  ApiUsersDeleteData,
} from "@/lib/api/generated";

export const userKeys = {
  all: ["users"] as const,
  lists: () => [...userKeys.all, "list"] as const,
  list: (filters?: ApiUsersGetData["query"]) =>
    [...userKeys.lists(), filters] as const,
  details: () => [...userKeys.all, "detail"] as const,
  detail: (id: string) => [...userKeys.details(), id] as const,
};

export function useUsers(filters?: ApiUsersGetData["query"]) {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => apiUsersGet({ query: filters }),
    select: (data) => data.data,
  });
}

export function useUserById(id: string) {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: () => apiUsersGet2({ path: { id } } as ApiUsersGet2Data),
    select: (data) => data.data,
    enabled: !!id,
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ApiUsersPostData["body"]) =>
      apiUsersPost({ body: data }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() });
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      apiUsersDelete({ path: { id } } as ApiUsersDeleteData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() });
    },
  });
}
