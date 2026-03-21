"use client";

import { useState } from "react";
import { Settings, User, Bell, Shield, Palette, Save, Loader2 } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";

type SettingsTab = "profile" | "notifications" | "security" | "appearance";

export default function SettingsClient() {
  const [activeTab, setActiveTab] = useState<SettingsTab>("profile");
  const [isSaving, setIsSaving] = useState(false);

  const handleSave = async () => {
    setIsSaving(true);
    await new Promise((resolve) => setTimeout(resolve, 1000));
    toast.success("Configurações salvas com sucesso");
    setIsSaving(false);
  };

  const tabs = [
    { id: "profile" as const, label: "Perfil", icon: User },
    { id: "notifications" as const, label: "Notificações", icon: Bell },
    { id: "security" as const, label: "Segurança", icon: Shield },
    { id: "appearance" as const, label: "Aparência", icon: Palette },
  ];

  return (
    <div className="p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-foreground">Configurações</h1>
        <p className="text-muted-foreground">Gerencie suas preferências do sistema</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-4">
        <Card className="lg:col-span-1">
          <CardContent className="p-4">
            <nav className="space-y-1">
              {tabs.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                      activeTab === tab.id
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    }`}
                  >
                    <Icon className="h-4 w-4" />
                    {tab.label}
                  </button>
                );
              })}
            </nav>
          </CardContent>
        </Card>

        <div className="lg:col-span-3 space-y-6">
          {activeTab === "profile" && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2"><User className="h-5 w-5" />Perfil do Administrador</CardTitle>
                <CardDescription>Atualize suas informações pessoais</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Nome</label>
                    <Input defaultValue="Administrador" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Email</label>
                    <Input defaultValue="admin@meajudaai.com" type="email" />
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Função</label>
                  <div>
                    <Badge variant="success">Administrador</Badge>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {activeTab === "notifications" && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2"><Bell className="h-5 w-5" />Notificações</CardTitle>
                <CardDescription>Configure como você recebe notificações</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Novos cadastros de prestadores</p>
                    <p className="text-sm text-muted-foreground">Receba notificações quando um novo prestador se cadastrar</p>
                  </div>
                  <input type="checkbox" defaultChecked className="h-5 w-5" />
                </div>
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Solicitações de verificação pendentes</p>
                    <p className="text-sm text-muted-foreground">Notifique quando houver solicitações pendentes de verificação</p>
                  </div>
                  <input type="checkbox" defaultChecked className="h-5 w-5" />
                </div>
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Relatórios diários</p>
                    <p className="text-sm text-muted-foreground">Receba um resumo diário de atividade</p>
                  </div>
                  <input type="checkbox" className="h-5 w-5" />
                </div>
              </CardContent>
            </Card>
          )}

          {activeTab === "security" && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2"><Shield className="h-5 w-5" />Segurança</CardTitle>
                <CardDescription>Gerencie suas configurações de segurança</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-4">
                  <h3 className="font-medium">Alterar senha</h3>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="space-y-2">
                      <label className="text-sm font-medium">Senha atual</label>
                      <Input type="password" placeholder="••••••••" />
                    </div>
                    <div className="space-y-2">
                      <label className="text-sm font-medium">Nova senha</label>
                      <Input type="password" placeholder="••••••••" />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Confirmar nova senha</label>
                    <Input type="password" placeholder="••••••••" />
                  </div>
                </div>

                <div className="border-t border-border pt-6">
                  <h3 className="font-medium mb-4">Autenticação em dois fatores</h3>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Status</p>
                      <p className="text-sm text-muted-foreground">Proteja sua conta com autenticação adicional</p>
                    </div>
                    <Badge variant="secondary">Desativado</Badge>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {activeTab === "appearance" && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2"><Palette className="h-5 w-5" />Aparência</CardTitle>
                <CardDescription>Personalize a aparência do sistema</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div>
                  <h3 className="font-medium mb-4">Tema</h3>
                  <div className="grid grid-cols-3 gap-4">
                    <button className="flex flex-col items-center gap-2 rounded-lg border-2 border-primary p-4">
                      <div className="h-12 w-full rounded bg-white border border-gray-200" />
                      <span className="text-sm font-medium">Claro</span>
                    </button>
                    <button className="flex flex-col items-center gap-2 rounded-lg border border-border p-4 hover:border-primary transition-colors">
                      <div className="h-12 w-full rounded bg-gray-900" />
                      <span className="text-sm font-medium">Escuro</span>
                    </button>
                    <button className="flex flex-col items-center gap-2 rounded-lg border border-border p-4 hover:border-primary transition-colors">
                      <div className="h-12 w-full rounded bg-gradient-to-r from-white to-gray-900" />
                      <span className="text-sm font-medium">Sistema</span>
                    </button>
                  </div>
                </div>

                <div>
                  <h3 className="font-medium mb-4">Idioma</h3>
                  <select className="w-full rounded-lg border border-border bg-background px-3 py-2 text-sm">
                    <option value="pt-BR">Português (Brasil)</option>
                    <option value="en" disabled>English (Em breve)</option>
                  </select>
                </div>
              </CardContent>
            </Card>
          )}

          <div className="flex justify-end">
            <Button onClick={handleSave} disabled={isSaving}>
              {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              <Save className="mr-2 h-4 w-4" />
              Salvar Alterações
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
