"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Users,
  FileText,
  FolderTree,
  Wrench,
  MapPin,
  Settings,
  LogOut,
  Menu,
  X,
} from "lucide-react";
import { signOut, useSession } from "next-auth/react";
import { twMerge } from "tailwind-merge";
import { useTranslation } from "react-i18next";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { APP_ROUTES, ROLES } from "@/lib/types";

export function Sidebar() {
  const { t } = useTranslation("common");
  const [isOpen, setIsOpen] = useState(false);
  const pathname = usePathname();
  const { data: session } = useSession();

  const navItems = [
    { href: APP_ROUTES.DASHBOARD, label: t("dashboard"), icon: LayoutDashboard },
    { href: APP_ROUTES.PROVIDERS, label: t("providers"), icon: Users },
    { href: APP_ROUTES.DOCUMENTS, label: t("documents"), icon: FileText },
    { href: APP_ROUTES.CATEGORIES, label: t("categories"), icon: FolderTree },
    { href: APP_ROUTES.SERVICES, label: t("services"), icon: Wrench },
    { href: APP_ROUTES.CITIES, label: t("cities"), icon: MapPin },
    { href: APP_ROUTES.SETTINGS, label: t("settings"), icon: Settings },
  ];

  const name = session?.user?.name || "Admin";
  const nameTrimmed = name.trim();
  const firstInitial = nameTrimmed ? nameTrimmed.charAt(0).toUpperCase() : "A";
  const isAdmin = session?.user?.roles?.includes(ROLES.ADMIN);

  return (
    <>
      {/* Mobile Hamburger Button */}
      <button 
        className="fixed top-4 left-4 z-40 md:hidden flex items-center justify-center p-2 rounded-md bg-surface border border-border shadow-sm text-foreground"
        onClick={() => setIsOpen(true)}
        aria-label="Open sidebar"
        data-testid="mobile-menu-toggle"
      >
        <Menu className="h-5 w-5" />
      </button>

      {/* Backdrop for mobile */}
      {isOpen && (
        <div 
          className="fixed inset-0 z-40 bg-black/50 md:hidden" 
          onClick={() => setIsOpen(false)}
          aria-hidden="true"
          data-testid="mobile-menu-backdrop"
        />
      )}

      <aside 
        className={`fixed left-0 top-0 z-50 flex h-screen w-64 flex-col border-r border-border bg-surface transition-transform duration-200 md:translate-x-0 ${isOpen ? "translate-x-0" : "-translate-x-full"}`}
        data-testid="sidebar"
      >
        <div className="flex h-16 shrink-0 items-center justify-between border-b border-border px-6" data-testid="mobile-menu">
          <Link href={APP_ROUTES.DASHBOARD} className="flex items-center gap-2" onClick={() => setIsOpen(false)}>
            <span className="text-xl font-bold text-primary">MeAjudaAí</span>
            <span className="text-xs font-medium text-secondary">Admin</span>
          </Link>
          <button className="md:hidden" onClick={() => setIsOpen(false)} aria-label="Close sidebar">
            <X className="h-5 w-5 text-muted-foreground" />
          </button>
        </div>

        <nav className="flex-1 space-y-1 p-4 overflow-y-auto">
          {navItems.map((item) => {
            const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
            const Icon = item.icon;

            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setIsOpen(false)}
                className={twMerge(
                  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-foreground-subtle hover:bg-muted hover:text-foreground"
                )}
              >
                <Icon className="h-5 w-5" />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="border-t border-border p-4">
          <div className="mb-3 flex items-center justify-between px-3">
            <div className="flex items-center gap-3">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-secondary text-secondary-foreground text-sm font-medium">
                {firstInitial}
              </div>
              <div className="flex-1 overflow-hidden">
                <p className="truncate text-sm font-medium">{nameTrimmed}</p>
                <p className="truncate text-xs text-muted-foreground">
                  {isAdmin ? "Administrador" : "Usuário"}
                </p>
              </div>
            </div>
            <ThemeToggle />
          </div>
          <button
            onClick={() => signOut({ callbackUrl: "/login" })}
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
          >
            <LogOut className="h-5 w-5" />
            {t("logout")}
          </button>
        </div>
      </aside>
    </>
  );
}
