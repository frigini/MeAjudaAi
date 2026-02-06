import type { Metadata } from "next";
import "./globals.css";
import { Header } from "@/components/layout/header";
import { Footer } from "@/components/layout/footer";
import { QueryProvider } from "@/components/providers/query-provider";

export const metadata: Metadata = {
  title: "AjudaAi - Conectando quem precisa com quem sabe fazer",
  description:
    "Encontre prestadores de serviços confiáveis perto de você. Avaliações reais, profissionais verificados.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR">
      <body className="antialiased">
        <QueryProvider>
          <Header />
          <main className="min-h-screen">{children}</main>
          <Footer />
        </QueryProvider>
      </body>
    </html>
  );
}
