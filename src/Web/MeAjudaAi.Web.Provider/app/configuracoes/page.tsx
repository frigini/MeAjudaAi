"use client";

import { useState } from "react";
import Link from "next/link";
import { Button } from "../../components/ui/button";

export default function ConfiguracoesPage() {
  const [isVisible, setIsVisible] = useState(true);
  const [showDeactivateModal, setShowDeactivateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const handleToggleVisibility = () => {
    if (isVisible) {
      setShowDeactivateModal(true);
    } else {
      setIsVisible(true);
    }
  };

  const confirmDeactivation = () => {
    setIsVisible(false);
    setShowDeactivateModal(false);
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
        <div className="ml-auto text-sm font-medium hover:underline cursor-pointer">Sair</div>
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
              <Button variant="destructive" onClick={confirmDeactivation}>
                Desativar
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
                onClick={() => setShowDeleteModal(false)}
                className="text-foreground-subtle hover:text-foreground"
              >
                &times;
              </button>
            </div>
            <p className="mb-6 text-sm text-foreground-subtle">
              Apagando seu perfil, iremos excluir todos os seus dados do nosso sistema e não será mais possível recuperá-los.<br/><br/>
              Em conformidade com a LGPD, seus dados serão permanentemente excluídos.
            </p>
            <div className="flex justify-end gap-3">
              <Button variant="ghost" className="bg-muted hover:bg-muted/80" onClick={() => setShowDeleteModal(false)}>
                Cancelar
              </Button>
              <Button variant="destructive" onClick={() => {
                // Implement delete account logic here
                setShowDeleteModal(false);
              }}>
                Excluir
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
