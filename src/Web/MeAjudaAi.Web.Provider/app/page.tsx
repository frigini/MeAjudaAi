import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";

export default function Index() {
  return (
    <div className="container mx-auto max-w-5xl py-12 px-4 sm:px-6 lg:px-8">
      <header className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">
          Dashboard do Prestador
        </h1>
        <p className="mt-2 text-lg text-foreground-subtle">
          Bem-vindo de volta! Aqui você pode gerenciar seu perfil e informações.
        </p>
      </header>

      <main className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* Profile Status Card */}
        <Card>
          <CardHeader>
            <CardTitle>Status do Perfil</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <div className="flex items-center gap-3">
              <div className="h-2 w-2 rounded-full bg-emerald-500" />
              <p className="text-sm font-medium">Ativo e visível</p>
            </div>
            <p className="text-sm text-muted-foreground">
              Seu perfil está acessível nas buscas da sua região.
            </p>
            <Button variant="secondary" className="mt-2 w-full">
              Pausar Visibilidade
            </Button>
          </CardContent>
        </Card>

        {/* Services Configuration Card */}
        <Card>
          <CardHeader>
            <CardTitle>Serviços</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <p className="text-sm text-muted-foreground">
              Configure seus serviços oferecidos, fotos, e região de atuação.
            </p>
            <Button variant="primary" className="mt-auto w-full">
              Gerenciar Serviços
            </Button>
          </CardContent>
        </Card>

        {/* Verification / Upload Card */}
        <Card className="md:col-span-2 lg:col-span-1">
          <CardHeader>
            <CardTitle>Documentos</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <p className="text-sm text-foreground-subtle">
              Sua conta requer envio de documentos para validação final de segurança.
            </p>
            <div className="flex flex-col gap-2 rounded-lg border border-border bg-surface p-4">
              <h4 className="text-sm font-medium">Documento de Identidade</h4>
              <p className="text-xs text-muted-foreground">
                Envie a frente e o verso do seu RG ou CNH vigente.
              </p>
            </div>
            <Button variant="primary" className="mt-2 w-full">
              Fazer Upload
            </Button>
          </CardContent>
        </Card>
      </main>
    </div>
  );
}
