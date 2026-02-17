import type { Metadata } from "next";
import "./globals.css";
import { AppProviders } from "@/components/providers/app-providers";
import { Toaster } from "sonner";

export const metadata: Metadata = {
  title: "Me Ajuda Aí - Conectando quem precisa com quem sabe fazer",
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
        <AppProviders>
          {children}
          <Toaster />
        </AppProviders>
      </body>
    </html>
  );
}
