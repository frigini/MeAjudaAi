"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "../../components/ui/button";
import { useMutation, useQuery } from "@tanstack/react-query";
import { apiMeGet } from "@/lib/api/generated";
import { signOut } from "next-auth/react";
import { toast } from "sonner";

export default function ConfiguracoesPage() {
  const router = useRouter();
  const [isVisible, setIsVisible] = useState(true);
  const [showDeactivateModal, setShowDeactivateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteConfirmation, setDeleteConfirmation] = useState("");

  const { data: providerData } = useQuery({
    queryKey: ["providerMe"],
    queryFn: () => apiMeGet(),
  });

  const deactivateMutation = useMutation({
    mutationFn: async () => {
      const response = await fetch(`/api/v1/providers/${providerData?.data?.data?.id}/deactivate`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      });
      if (!response.ok) throw new Error("Falha ao desativar");
      return response.json();
    },
    onSuccess: () => {
      toast.success("Perfil desativado com sucesso!");
      setIsVisible(false);
      setShowDeactivateModal(false);
      router.refresh();
    },
    onError: () => {
      toast.error("Erro ao desativar o perfil. Tente novamente.");
    },
  });

  const handleToggleVisibility = () => {
    if (isVisible) {
      setShowDeactivateModal(true);
    } else {
      setIsVisible(true);
    }
  };

  const confirmDeactivation = () => {
    deactivateMutation.mutate();
  };

  const confirmDelete = async () => {
    if (deleteConfirmation !== "EXCLUIR") {
      toast.error("Digite EXACTAMENTE EXCLUIR para confirmar");
      return;
    }

    try {
      const userId = providerData?.data?.data?.userId;
      if (!userId) {
        toast.error("ID do usuário não encontrado");
        return;
      }

      const response = await fetch(`/api/v1/users/${userId}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error("Falha ao excluir conta");
      }

      toast.success("Conta excluída com sucesso!");
      setShowDeleteModal(false);
      
      setTimeout(() => {
        signOut({ callbackUrl: "/" });
      }, 1500);
    } catch (error) {
      console.error("Erro ao excluir conta:", error);
      toast.error("Erro ao excluir a conta. Tente novamente mais tarde.");
    }
  };

  return (
    <div className="container mx-auto max-w-2xl py-8 px-4 sm:px-6 lg:px-8">
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild className="h-8 w-8 px-0">
          <Link href="/">
            <span className="text-xl">&lsaquo;</span>
            <span className="sr-only">Voltar</span>
          </Link>
        </Button>
        <span className="font-bold">AjudaAí</span>
        <div 
          className="ml-auto text-sm font-medium hover:underline cursor-pointer"
          onClick={() => signOut({ callbackUrl: "/" })}
        >
          Sair
        </div>
      </div>

      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10 flex flex-col min-h-[60vh]">
        <h1 className="mb-12 text-2xl font-bold tracking-tight text-foreground">
          Configurações
        </h1>

        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-foreground">Deixar meu perfil visível</span>
          <button
            type="button"
            role="switch"
            aria-checked={isVisible}
            onClick={handleToggleVisibility}
            className="relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center justify-center rounded-full bg-border transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 aria-checked:bg-emerald-500 appearance-none after:absolute after:left-0.5 after:top-0.5 after:h-5 after:w-5 after:rounded-full after:bg-white after:transition-all after:content-[''] aria-checked:after:translate-x-5"
          >
            <span className="sr-only">Alternar visibilidade do perfil</span>
          </button>
        </div>

        <div className="mt-8 border-t border-border pt-8">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-foreground-subtle mb-4">
            Assinatura e Pagamentos
          </h2>
          <div className="flex flex-col gap-4">
            <p className="text-sm text-foreground-subtle">
              Gerencie seus dados de pagamento, faturas e plano de assinatura diretamente pelo portal seguro do Stripe.
            </p>
            <Button
              variant="secondary"
              className="w-full sm:w-auto"
              onClick={async () => {
                try {
                  const response = await fetch(`/api/v1/payments/subscriptions/billing-portal`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ returnUrl: 'account' })
                  });
                  if (!response.ok) throw new Error("Falha ao carregar portal");
                  const data = await response.json();
                  if (data.portalUrl) {
                    window.location.href = data.portalUrl;
                  }
                } catch (error) {
                  toast.error("Não foi possível carregar o portal de pagamentos.");
                  console.error(error);
                }
              }}
            >
              Gerenciar Assinatura
            </Button>
          </div>
        </div>

        <div className="mt-auto flex justify-center pt-12">
          <Button 
            variant="destructive" 
            size="sm"
            onClick={() => setShowDeleteModal(true)}
          >
            Apagar minha conta
          </Button>
        </div>
      </main>

      {/* Modal Desativar Perfil */}
      {showDeactivateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-lg animate-in fade-in zoom-in duration-200">
            <div className="mb-4 flex items-start justify-between">
              <h2 className="text-lg font-bold text-foreground">Esconder perfil?</h2>
              <button 
                onClick={() => setShowDeactivateModal(false)}
                className="text-foreground-subtle hover:text-foreground"
              >
                &times;
              </button>
            </div>
            <p className="mb-6 text-sm text-foreground-subtle">
              Desativando seu perfil ele não será mais exibido em buscas no nosso site!<br/><br/>
              Essa opção é ideal para quando você quiser editar seus serviços, descrição ou para quando quiser tirar umas férias.
            </p>
            <div className="flex justify-end gap-3">
              <Button variant="ghost" className="bg-muted hover:bg-muted/80" onClick={() => setShowDeactivateModal(false)}>
                Cancelar
              </Button>
              <Button variant="destructive" onClick={confirmDeactivation} disabled={deactivateMutation.isPending}>
                {deactivateMutation.isPending ? "Desativando..." : "Desativar"}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Modal Apagar Conta */}
      {showDeleteModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-lg animate-in fade-in zoom-in duration-200">
            <div className="mb-4 flex items-start justify-between">
              <h2 className="text-lg font-bold text-foreground">Apagar perfil?</h2>
              <button 
                onClick={() => {
                  setShowDeleteModal(false);
                  setDeleteConfirmation("");
                }}
                className="text-foreground-subtle hover:text-foreground"
              >
                &times;
              </button>
            </div>
            <p className="mb-4 text-sm text-foreground-subtle">
              Apagando seu perfil, iremos excluir todos os seus dados do nosso sistema e não será mais possível recuperá-los.<br/><br/>
              Em conformidade com a LGPD, seus dados serão permanentemente excluídos.
            </p>
            
            <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/10 p-3">
              <p className="text-xs text-destructive font-medium mb-2">Esta ação NÃO pode ser desfeita:</p>
              <ul className="text-xs text-destructive/80 list-disc list-inside space-y-1">
                <li>Seu perfil de prestador será removido</li>
                <li>Sua conta de usuário será excluída</li>
                <li>Todos os seus dados serão apagados</li>
                <li>Seus documentos serão removidos</li>
              </ul>
            </div>

            <div className="mb-6">
              <label className="block text-sm font-medium text-foreground mb-2">
                Digite <span className="font-bold text-destructive">EXCLUIR</span> para confirmar:
              </label>
              <input
                type="text"
                value={deleteConfirmation}
                onChange={(e) => setDeleteConfirmation(e.target.value)}
                className="w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus:border-destructive focus:outline-none focus:ring-1 focus:ring-destructive"
                placeholder="EXCLUIR"
              />
            </div>

            <div className="flex justify-end gap-3">
              <Button 
                variant="ghost" 
                className="bg-muted hover:bg-muted/80"
                onClick={() => {
                  setShowDeleteModal(false);
                  setDeleteConfirmation("");
                }}
              >
                Cancelar
              </Button>
              <Button 
                variant="destructive" 
                onClick={confirmDelete}
                disabled={deleteConfirmation !== "EXCLUIR"}
              >
                Excluir Minha Conta
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
