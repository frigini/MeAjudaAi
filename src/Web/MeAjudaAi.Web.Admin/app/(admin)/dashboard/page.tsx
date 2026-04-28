"use client";

/* eslint-disable @typescript-eslint/no-explicit-any */

import { useMemo } from "react";
import { Users, Clock, CheckCircle, AlertCircle, TrendingUp, Loader2, RefreshCw } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip, LineChart, Line, XAxis, YAxis, CartesianGrid } from "recharts";
import { useDashboardStats } from "@/hooks/admin";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { useTranslation } from "react-i18next";

const TypedTooltip = Tooltip as any;
const TypedLegend = Legend as any;

const verificationColors = {
  approved: "#22c55e",
  pending: "#f59e0b",
  under_review: "#3b82f6",
  rejected: "#ef4444",
  suspended: "#6b7280",
};

const typeColors = {
  individual: "#8b5cf6",
  company: "#06b6d4",
  freelancer: "#D96704", // Marca secundária — laranja
  cooperative: "#ec4899",
};

export default function DashboardPage() {
  const { t, i18n } = useTranslation("common");
  const { data: stats, isLoading, error, refetch } = useDashboardStats();

  const verificationData = useMemo(() => [
    { name: t("approved"), value: stats?.approved ?? 0, color: verificationColors.approved },
    { name: t("under_review"), value: stats?.underReview ?? 0, color: verificationColors.under_review },
    { name: t("pending"), value: stats?.pending ?? 0, color: verificationColors.pending },
    { name: t("rejected"), value: stats?.rejected ?? 0, color: verificationColors.rejected },
    { name: t("suspended"), value: stats?.suspended ?? 0, color: verificationColors.suspended },
  ].filter((d) => d.value > 0), [stats, t]);

  const typeData = useMemo(() => [
    { name: t("individual"), value: stats?.individual ?? 0, color: typeColors.individual },
    { name: t("company"), value: stats?.company ?? 0, color: typeColors.company },
    { name: t("freelancer"), value: stats?.freelancer ?? 0, color: typeColors.freelancer },
    { name: t("cooperative"), value: stats?.cooperative ?? 0, color: typeColors.cooperative },
  ].filter((d) => d.value > 0), [stats, t]);

  if (isLoading && !stats) {
    return (
      <div data-testid="dashboard-loading" className="flex items-center justify-center h-[calc(100vh-200px)]">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error) {
    return (
      <div data-testid="dashboard-error" className="flex flex-col items-center justify-center p-8 space-y-4">
        <p className="text-destructive">{t("error_loading_dashboard")}</p>
        <Button data-testid="retry-button" onClick={() => refetch()} variant="secondary">
          {t("retry")}
        </Button>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="mb-8 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">{t("dashboard")}</h1>
          <p className="text-muted-foreground">{t("dashboard_subtitle")}</p>
        </div>
        
        {stats?.updatedAt && (
          <div className="flex items-center space-x-4">
            <span data-testid="last-updated" className="text-sm text-muted-foreground">
              {t("updated_at")} {stats.updatedAt.toLocaleTimeString(i18n.language)}
            </span>
            <Button data-testid="refresh-dashboard" variant="secondary" size="icon" onClick={() => refetch()}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        )}
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-8" data-testid="kpi-grid">
        <Link href="/admin/providers" className="block transition-transform hover:scale-[1.02]">
          <Card data-testid="kpi-total-providers" className="h-full border-l-4 border-l-primary">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {t("total_providers")}
              </CardTitle>
              <Users className="h-4 w-4 text-primary" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold" data-testid="kpi-value">{stats?.total ?? 0}</div>
              <p className="text-xs text-muted-foreground flex items-center gap-1" data-testid="kpi-label">
                <TrendingUp className="h-3 w-3 text-green-500" />
                {t("total_providers_label")}
              </p>
            </CardContent>
          </Card>
        </Link>

        <Card data-testid="kpi-pending-verification" className="border-l-4 border-l-secondary">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("waiting_verification")}
            </CardTitle>
            <Clock className="h-4 w-4 text-secondary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{(stats?.pending ?? 0) + (stats?.underReview ?? 0)}</div>
            <p className="text-xs text-muted-foreground" data-testid="kpi-label">{t("waiting_verification_label")}</p>
          </CardContent>
        </Card>

        <Card data-testid="kpi-approved" className="border-l-4 border-l-green-500">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("approved")}
            </CardTitle>
            <CheckCircle className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{stats?.approved ?? 0}</div>
            <p className="text-xs text-muted-foreground" data-testid="kpi-label">{t("approved_label")}</p>
          </CardContent>
        </Card>

        <Card data-testid="kpi-rejected" className="border-l-4 border-l-red-500">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t("rejected")}
            </CardTitle>
            <AlertCircle className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{stats?.rejected ?? 0}</div>
            <p className="text-xs text-muted-foreground" data-testid="kpi-label">{t("rejected_label")}</p>
          </CardContent>
        </Card>
      </div>

      <div className="mb-8">
        <Card className="bg-cream/20">
          <CardHeader>
            <CardTitle data-testid="chart-title-providers-over-time">{t("providers_over_time")}</CardTitle>
          </CardHeader>
          <CardContent>
            <div data-testid="providers-line-chart" className="h-[300px] w-full">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={stats?.timeSeries ?? []}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
                  <XAxis dataKey="date" tickLine={false} axisLine={false} tick={{fill: '#6b7280', fontSize: 12}} dy={10} />
                  <YAxis tickLine={false} axisLine={false} tick={{fill: '#6b7280', fontSize: 12}} dx={-10} />
                  <TypedTooltip 
                    contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                    wrapperProps={{ "data-testid": "chart-tooltip" } as any} 
                  />
                  <Line type="monotone" dataKey="value" name={t("total_providers")} stroke="#395873" strokeWidth={3} dot={{ r: 4, strokeWidth: 2 }} activeDot={{ r: 6 }} />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle data-testid="chart-title-verification-status">{t("verification_status")}</CardTitle>
          </CardHeader>
          <CardContent>
            <div data-testid="verification-pie-chart" className="h-[250px] relative">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={verificationData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={5}
                    dataKey="value"
                    labelLine={false}
                  >
                    {verificationData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <TypedTooltip wrapperProps={{ "data-testid": "chart-tooltip" } as any} />
                  <TypedLegend 
                    wrapperProps={{ "data-testid": "chart-legend" } as any}
                    formatter={(value: any) => <span data-testid="legend-item" className="text-sm font-medium">{value}</span>}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle data-testid="chart-title-providers-by-type">{t("providers_by_type")}</CardTitle>
          </CardHeader>
          <CardContent>
             <div className="h-[250px] relative">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={typeData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={5}
                    dataKey="value"
                    labelLine={false}
                  >
                    {typeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend formatter={(value) => <span className="text-sm font-medium">{value}</span>} />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
