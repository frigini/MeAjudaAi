import { Search } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { UserMenu } from "./user-menu";

export interface HeaderProps {
    className?: string;
}

export function Header({ className }: HeaderProps) {
    return (
        <header className={cn("sticky top-0 z-50 border-b border-border bg-white shadow-sm", className)}>
            <div className="container mx-auto flex h-16 items-center justify-between px-4">
                {/* Logo */}
                <div className="flex items-center gap-2">
                    <span className="text-2xl font-bold text-primary">AjudaAi</span>
                </div>

                {/* Search Bar - Hidden on mobile */}
                <div className="hidden md:flex flex-1 max-w-2xl mx-8">
                    <div className="relative w-full">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-foreground-subtle" />
                        <input
                            type="search"
                            placeholder="Buscar serviço..."
                            className="w-full pl-10 pr-4 py-2 border border-input rounded-lg focus:outline-none focus:ring-2 focus:ring-ring"
                        />
                    </div>
                </div>

                {/* Actions */}
                <UserMenu />
            </div>
        </header>
    );
}
