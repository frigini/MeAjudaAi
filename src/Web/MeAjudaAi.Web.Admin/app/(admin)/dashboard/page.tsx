"use client";

import { Users, Clock, CheckCircle, AlertCircle, TrendingUp, Loader2 } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from "recharts";
import { useDashboardStats } from "@/hooks/admin";

const verificationColors = {
  approved: "#22c55e",
  pending: "#f59e0b",
  underReview: "#3b82f6",
  rejected: "#ef4444",
  suspended: "#6b7280",
};

const typeColors = {
  individual: "#8b5cf6",
  company: "#06b6d4",
  freelancer: "#f97316",
  cooperative: "#ec4899",
};

export default function DashboardPage() {
  const { data: stats, isLoading, error } = useDashboardStats();

  const verificationData = [
    { name: "Aprovados", value: stats?.approved ?? 0, color: verificationColors.approved },
    { name: "Em Análise", value: stats?.underReview ?? 0, color: verificationColors.underReview },
    { name: "Pendentes", value: stats?.pending ?? 0, color: verificationColors.pending },
    { name: "Rejeitados", value: stats?.rejected ?? 0, color: verificationColors.rejected },
    { name: "Suspensos", value: stats?.suspended ?? 0, color: verificationColors.suspended },
  ].filter((d) => d.value > 0);

  const typeData = [
    { name: "Pessoa Física", value: stats?.individual ?? 0, color: typeColors.individual },
    { name: "Empresa", value: stats?.company ?? 0, color: typeColors.company },
    { name: "Freelancer", value: stats?.freelancer ?? 0, color: typeColors.freelancer },
    { name: "Cooperativa", value: stats?.cooperative ?? 0, color: typeColors.cooperative },
  ].filter((d) => d.value > 0);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-200px)]">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8 text-center text-destructive">
        Erro ao carregar dados do dashboard. Tente novamente.
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>
        <p className="text-muted-foreground">Visão geral dos prestadores e métricas</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-8" data-testid="kpi-grid">
        <Card data-testid="kpi-total-providers">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total de Prestadores
            </CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{stats?.total ?? 0}</div>
            <p className="text-xs text-muted-foreground flex items-center gap-1" data-testid="kpi-label">
              <TrendingUp className="h-3 w-3 text-green-500" />
              Total de Prestadores
            </p>
          </CardContent>
        </Card>

        <Card data-testid="kpi-pending-verification">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Aguardando Verificação
            </CardTitle>
            <Clock className="h-4 w-4 text-yellow-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{(stats?.pending ?? 0) + (stats?.underReview ?? 0)}</div>
            <p className="text-xs text-muted-foreground" data-testid="kpi-label">Aguardando Verificação</p>
          </CardContent>
        </Card>

        <Card data-testid="kpi-approved">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Aprovados
            </CardTitle>
            <CheckCircle className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{stats?.approved ?? 0}</div>
            <p className="text-xs text-muted-foreground" data-testid="kpi-label">Aprovados</p>
          </CardContent>
        </Card>

        <Card data-testid="kpi-rejected">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Rejeitados
            </CardTitle>
            <AlertCircle className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold" data-testid="kpi-value">{stats?.rejected ?? 0}</div>
            <p className="text-xs text-muted-foreground" data-testid="kpi-label">Rejeitados</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Status de Verificação</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-[250px]">
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
                    label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}
                  >
                    {verificationData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Prestadores por Tipo</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-[250px]">
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
                    label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}
                  >
                    {typeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
