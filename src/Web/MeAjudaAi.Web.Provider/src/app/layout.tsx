import { Inter } from "next/font/google";
import "./global.css";

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
      <body className={inter.className}>
        <div className="min-h-screen bg-surface-raised text-foreground antialiased selection:bg-primary selection:text-primary-foreground">
          {children}
        </div>
      </body>
    </html>
  );
}
