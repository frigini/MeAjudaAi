import { Search, CheckCircle2, Users, Star } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";

export default function HomePage() {
  return (
    <div className="flex flex-col">
      {/* Hero Section */}
      <section className="bg-primary text-white py-20">
        <div className="container mx-auto px-4 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-4">
            Conectando quem precisa com
            <br />
            <span className="text-secondary-light">quem sabe fazer.</span>
          </h1>
          <p className="text-xl mb-8 text-white/90 max-w-2xl mx-auto">
            Você já precisou de algum serviço e não sabia de nenhuma
            referência? Nós resolvemos esse problema para você!
          </p>

          {/* Search Bar */}
          <div className="max-w-2xl mx-auto">
            <div className="relative">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 size-5 text-foreground-subtle" />
              <input
                type="search"
                placeholder="Buscar serviço..."
                className="w-full pl-12 pr-4 py-4 rounded-lg text-foreground focus:outline-none focus:ring-2 focus:ring-secondary"
              />
              <Button
                variant="primary"
                size="lg"
                className="absolute right-2 top-1/2 -translate-y-1/2"
              >
                Buscar
              </Button>
            </div>
          </div>
        </div>
      </section>

      {/* Como Funciona */}
      <section className="py-16 bg-white">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-12 text-foreground">
            Como funciona?
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <Card>
              <CardHeader>
                <div className="size-12 rounded-full bg-secondary/10 flex items-center justify-center mb-4">
                  <Search className="size-6 text-secondary" />
                </div>
                <CardTitle>1. Busque o serviço</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-foreground-subtle">
                  Digite o serviço que você precisa e encontre profissionais
                  qualificados perto de você.
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <div className="size-12 rounded-full bg-secondary/10 flex items-center justify-center mb-4">
                  <Users className="size-6 text-secondary" />
                </div>
                <CardTitle>2. Compare prestadores</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-foreground-subtle">
                  Veja avaliações, preços e perfis de diferentes prestadores
                  para escolher o melhor.
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <div className="size-12 rounded-full bg-secondary/10 flex items-center justify-center mb-4">
                  <CheckCircle2 className="size-6 text-secondary" />
                </div>
                <CardTitle>3. Contrate com segurança</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-foreground-subtle">
                  Entre em contato diretamente e contrate com confiança
                  baseado em avaliações reais.
                </p>
              </CardContent>
            </Card>
          </div>
        </div>
      </section>

      {/* CTA Prestadores */}
      <section className="py-16 bg-surface-raised">
        <div className="container mx-auto px-4 text-center">
          <h2 className="text-3xl font-bold mb-4 text-foreground">
            Você é prestador de serviço?
          </h2>
          <p className="text-lg mb-8 text-foreground-subtle max-w-2xl mx-auto">
            Faça seu cadastro na nossa plataforma, cadastre seus serviços e
            apareça para seus clientes. Tenha boas recomendações e destaque-se
            frente aos seus concorrentes.
          </p>
          <Button size="lg" variant="primary">
            Cadastre-se grátis
          </Button>
        </div>
      </section>
    </div>
  );
}
