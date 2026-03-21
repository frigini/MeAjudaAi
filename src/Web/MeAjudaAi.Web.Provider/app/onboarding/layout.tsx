export default function OnboardingLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col items-center bg-surface-raised px-4 py-12 sm:px-6 lg:px-8">
      <div className="mb-8 w-full max-w-2xl text-center">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">
          Complete seu Perfil
        </h1>
        <p className="mt-2 text-muted-foreground">
          Faltam apenas alguns passos para seu perfil ficar visível nas buscas.
        </p>
      </div>

      <div className="w-full max-w-2xl">
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <div className="flex border-b border-border bg-secondary/50 px-6 py-4 text-sm font-medium">
            <span className="text-primary">Etapa em andamento</span>
          </div>
          <div className="p-6 sm:p-8">
            {children}
          </div>
        </div>
      </div>
    </div>
  );
}
