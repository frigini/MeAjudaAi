"use client";

import { useQuery } from "@tanstack/react-query";
import { apiProvidersGet } from "@/lib/api/generated";
import type { ApiProvidersGetData } from "@/lib/api/generated";

export interface DashboardStats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  suspended: number;
  underReview: number;
  individual: number;
  company: number;
  freelancer: number;
  cooperative: number;
}

export function useDashboardStats() {
  return useQuery({
    queryKey: ["dashboard", "stats"],
    queryFn: async (): Promise<DashboardStats> => {
      const response = await apiProvidersGet({} as ApiProvidersGetData);
      const providers = response.data?.data ?? [];

      const stats: DashboardStats = {
        total: providers.length,
        pending: 0,
        approved: 0,
        rejected: 0,
        suspended: 0,
        underReview: 0,
        individual: 0,
        company: 0,
        freelancer: 0,
        cooperative: 0,
      };

      providers.forEach((p) => {
        switch (p.verificationStatus) {
          case 0: stats.pending++; break;
          case 1: stats.underReview++; break;
          case 2: stats.approved++; break;
          case 3: stats.rejected++; break;
          case 4: stats.suspended++; break;
        }

        switch (p.type) {
          case 1: stats.individual++; break;
          case 2: stats.company++; break;
          case 3: stats.cooperative++; break;
          case 4: stats.freelancer++; break;
        }
      });

      return stats;
    },
    staleTime: 1000 * 60 * 5,
  });
}
