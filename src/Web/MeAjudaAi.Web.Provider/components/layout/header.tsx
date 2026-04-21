"use client";

import Image from "next/image";
import Link from "next/link";
import { User, Calendar as CalendarIcon } from "lucide-react";
import { useSession, signIn, signOut } from "next-auth/react";

export interface HeaderProps {
    className?: string;
}

export function Header({ className }: HeaderProps) {
    const { data: session, status } = useSession();

    return (
        <header className={`border-b border-[#E0702B] bg-white shadow-sm ${className || ''}`}>
            <div className="container mx-auto flex h-24 items-center justify-between px-4">
                {/* Logo */}
                <Link href="/" className="flex items-center gap-4 cursor-pointer shrink-0" aria-label="Ir para a página inicial">
                    <Image
                        src="/logo-icon-azul.png"
                        alt="Logo Icon"
                        width={120}
                        height={120}
                        className="h-20 w-auto object-contain"
                        style={{ width: "auto" }}
                        priority
                        quality={100}
                    />
                    <Image
                        src="/logo-text-azul.png"
                        alt="MeAjudaAí"
                        width={140}
                        height={40}
                        className="h-10 w-auto object-contain"
                        style={{ width: "auto" }}
                        priority
                        quality={100}
                    />
                </Link>

                {/* Actions (Simplified for Provider Web App) */}
                <div className="flex items-center shrink-0">
                    <div className="flex items-center gap-3 shrink-0 whitespace-nowrap">
                        {status === "loading" ? (
                            <div className="h-5 w-5 animate-pulse bg-surface rounded-full" />
                        ) : session ? (
                            <>
                                <Link href="/agenda" className="text-sm font-medium hover:underline flex flex-row items-center gap-2 mr-4 text-[#002D62]">
                                    <CalendarIcon className="h-5 w-5" /> Agenda
                                </Link>
                                <Link href="/configuracoes" className="text-sm font-medium hover:underline flex flex-row items-center gap-2">
                                    <User className="h-5 w-5" /> Configurações
                                </Link>
                                <button onClick={() => signOut({ callbackUrl: '/' })} className="text-sm font-medium text-destructive hover:underline ml-4">
                                    Sair
                                </button>
                            </>
                        ) : (
                            <button onClick={() => signIn("keycloak")} className="text-sm font-medium text-primary hover:underline flex flex-row items-center gap-2">
                                <User className="h-5 w-5" /> Entrar
                            </button>
                        )}
                    </div>
                </div>
            </div>
        </header>
    );
}
