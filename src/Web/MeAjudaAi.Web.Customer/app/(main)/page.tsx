import { Search, CheckCircle2, Users, ArrowRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { AdBanner } from "@/components/ui/ad-banner";
import { CitySearch } from "@/components/search/city-search";
import Image from "next/image";

export default function HomePage() {
  return (
    <div className="flex flex-col min-h-screen">
      <AdBanner />

      {/* Hero Section - White Background */}
      <section className="bg-white py-20 pb-10">
        <div className="container mx-auto px-4">
          <div className="flex flex-col items-start text-left max-w-4xl mb-12">
            <h1 className="flex flex-col gap-2 font-bold">
              <span className="text-2xl text-black">Conectando quem precisa com</span>
              <span className="text-5xl md:text-6xl text-secondary">quem sabe fazer.</span>
            </h1>
          </div>

          {/* City Search - Center aligned relative to the page */}
          <div className="w-full max-w-2xl mx-auto">
            <CitySearch />
          </div>
        </div>
      </section>

      {/* Blue Section - Conheça */}
      <section className="bg-primary text-white py-20 overflow-hidden relative">
        {/* Background decoration would go here */}
        <div className="container mx-auto px-4 flex flex-col md:flex-row items-center gap-12">
          <div className="flex-1 space-y-6">
            <h2 className="text-4xl font-bold text-white">
              Conheça o MeAjudaAí
            </h2>
            <p className="text-xl text-blue-50">
              Você já precisou de algum serviço e não sabia de nenhuma referência
              ou alguém que conhecia alguém que faça esse serviço que você está
              precisando?
            </p>
            <p className="text-xl text-blue-50">
              Nós nascemos para solucionar esse problema, uma plataforma que
              conecta quem está oferecendo serviço com quem está prestando
              serviço. Oferecemos métodos de avaliação dos serviços prestados,
              você consegue saber se o prestador possui boas indicações com
              base nos serviços já prestados por ele pela nossa plataforma.
            </p>
          </div>

          <div className="flex-1 relative h-[500px] w-full hidden md:block">
            <Image
              src="/illustration-woman.png"
              alt="Conheça o MeAjudaAí"
              fill
              className="object-contain object-center z-10"
              priority
            />
          </div>
        </div>
      </section>



      {/* CTA Prestadores */}
      <section className="py-20 bg-white">
        <div className="container mx-auto px-4">
          <div className="flex flex-col md:flex-row items-center gap-12">
            {/* Illustration Placeholder - Right (Desktop) / Top (Mobile) order swap? No, left usually. */}
            {/* Making it Right aligned text based on original code, but mockup 4 shows text on RIGHT, image LEFT? 
                    Actually mockup 4 bottom part: "Você é prestador de serviço?" Text on RIGHT, Man on LEFT.
                 */}
            <div className="flex-1 relative h-[500px] w-full hidden md:block order-1 md:order-2">
              <Image
                src="/illustration-man.png"
                alt="Seja um prestador"
                fill
                className="object-contain object-center mix-blend-multiply"
              />
            </div>

            <div className="flex-1 space-y-6 order-1 md:order-2">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-secondary/10 rounded-full">
                  <CheckCircle2 className="size-6 text-secondary" />
                </div>
                <h2 className="text-3xl font-bold text-foreground">
                  Você é prestador de serviço?
                </h2>
              </div>

              <p className="text-xl text-foreground-subtle">
                Faça seu cadastro na nossa plataforma, cadastre seus serviços,
                meios de contato e apareça para seus clientes, tenha boas
                recomendações e destaque-se frente aos seus concorrentes.
              </p>

              <p className="text-xl text-foreground-subtle">
                Não importa qual tipo de serviço você presta, sempre tem alguém
                precisando de uma ajuda! Conseguimos fazer com que o seu cliente
                te encontre, você estará na vitrine virtual mais cobiçada do Brasil.
              </p>

              <Button size="lg" className="bg-secondary hover:bg-secondary-hover text-white mt-4" asChild>
                <a href="/auth/signin">Cadastre-se grátis</a>
              </Button>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
