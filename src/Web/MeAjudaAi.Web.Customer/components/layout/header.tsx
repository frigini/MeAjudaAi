"use client";

import { Search } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { UserMenu } from "./user-menu";
import { useRouter, usePathname } from "next/navigation";
import { useState } from "react";
import { Input } from "@/components/ui/input";
import Image from "next/image";
import Link from "next/link";

export interface HeaderProps {
    className?: string;
}

export function Header({ className }: HeaderProps) {
    const router = useRouter();
    const pathname = usePathname();
    const [searchQuery, setSearchQuery] = useState("");

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        const trimmedQuery = searchQuery.trim();
        if (trimmedQuery) {
            router.push(`/buscar?q=${encodeURIComponent(trimmedQuery)}`);
        }
    };

    return (
        <header className={cn("sticky top-0 z-50 border-b border-border bg-white shadow-sm", className)}>
            <div className="container mx-auto flex h-24 items-center justify-between px-4">
                {/* Logo */}
                <Link href="/" className="flex items-center gap-4 cursor-pointer" aria-label="Ir para a página inicial">
                    <Image
                        src="/logo-icon-azul.png"
                        alt="Logo Icon"
                        width={120}
                        height={120}
                        className="h-[120px] w-auto object-contain"
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

                {/* Search Bar - Hidden on mobile and on search page */}
                {!pathname?.startsWith("/buscar") && (
                    <div className="hidden md:flex flex-1 max-w-2xl mx-8">
                        <form onSubmit={handleSearch} className="relative w-full">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-foreground-subtle" />
                            <Input
                                type="search"
                                placeholder="Buscar serviço..."
                                aria-label="Buscar serviço"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                className="w-full pl-10 pr-4 py-2 border-2 border-secondary rounded-lg focus:outline-none focus:ring-0 text-foreground bg-background"
                            />
                        </form>
                    </div>
                )}

                {/* Actions */}
                <UserMenu />
            </div>
        </header>
    );
}
