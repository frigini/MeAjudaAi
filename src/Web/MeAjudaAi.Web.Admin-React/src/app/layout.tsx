import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./global.css";
import { AppProviders } from "@/components/providers/app-providers";
import { getServerSession } from "next-auth/next";
import { authOptions } from "@/lib/auth/auth";


const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "MeAjudaAí - Admin Portal",
  description: "Portal de administração do MeAjudaAí",
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const session = await getServerSession(authOptions);

  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body className={`${inter.className} min-h-screen bg-background text-foreground antialiased`}>
        <AppProviders session={session}>{children}</AppProviders>
      </body>
    </html>
  );
}
