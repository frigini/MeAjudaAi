import { Inter } from "next/font/google";
import "./global.css";
import { Header } from "../components/layout/header";
import { Footer } from "../components/layout/footer";
import { AppProviders } from "../components/providers/app-providers";

const inter = Inter({ subsets: ["latin"] });

export const metadata = {
  title: "MeAjudaAí - Para Prestadores",
  description: "Gerencie seu perfil, serviços e agendamentos no MeAjudaAí.",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="pt-BR">
      <body className={`${inter.className} min-h-screen flex flex-col bg-surface-raised text-foreground antialiased selection:bg-primary selection:text-primary-foreground`}>
        <AppProviders>
          <Header />
          <main className="flex-grow">
            {children}
          </main>
          <Footer />
        </AppProviders>
      </body>
    </html>
  );
}
