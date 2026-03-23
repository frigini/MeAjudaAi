"use client";

import { useQuery } from "@tanstack/react-query";
import { apiProvidersGet2 } from "@/lib/api/generated";
import type { ApiProvidersGet2Data } from "@/lib/api/generated";
import { EVerificationStatus, EProviderType } from "@/lib/types";

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
      let currentPage = 1;
      let totalPages = 1;
      let allProviders: any[] = [];

      do {
        const response: any = await apiProvidersGet2({
          query: { pageNumber: currentPage, pageSize: 100 }
        } as any);

        const data = response.data?.value ?? response.data ?? response ?? {};
        const items = data.items ?? data.data ?? [];
        
        allProviders = allProviders.concat(items);
        totalPages = data.totalPages ?? 1;
        currentPage++;
      } while (currentPage <= totalPages);

      const stats: DashboardStats = {
        total: allProviders.length,
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

      allProviders.forEach((p) => {
        switch (p.verificationStatus) {
          case EVerificationStatus.Pending: stats.pending++; break;
          case EVerificationStatus.InProgress: stats.underReview++; break;
          case EVerificationStatus.Verified: stats.approved++; break;
          case EVerificationStatus.Rejected: stats.rejected++; break;
          case EVerificationStatus.Suspended: stats.suspended++; break;
        }

        switch (p.type) {
          case EProviderType.Individual: stats.individual++; break;
          case EProviderType.Company: stats.company++; break;
          case EProviderType.Cooperative: stats.cooperative++; break;
          case EProviderType.Freelancer: stats.freelancer++; break;
        }
      });

      return stats;
    },
    staleTime: 1000 * 60 * 5,
  });
}
