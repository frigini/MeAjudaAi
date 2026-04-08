import Link from "next/link";
import { Settings, Pencil, Star } from "lucide-react";
import { Button } from "../ui/button";

interface ProfileHeaderProps {
  name: string;
  email: string;
  isOnline: boolean;
  phones: string[];
  rating: number;
}

export function ProfileHeader({
  name,
  email,
  isOnline,
  phones,
  rating,
}: ProfileHeaderProps) {
  return (
    <div className="flex flex-col gap-6 md:flex-row md:items-start">
      {/* Avatar and Contact Info */}
      <div className="flex w-full md:w-64 flex-col items-center gap-3">
        <div className="relative flex h-32 w-32 shrink-0 items-center justify-center overflow-hidden rounded-full bg-muted">
          <span className="text-4xl text-muted-foreground">{name.charAt(0)}</span>
          {/* Avatar graphic placeholder */}
          <img src="https://i.pravatar.cc/150" alt={name} className="absolute inset-0 h-full w-full object-cover" />
        </div>
        <div className="flex text-primary">
          {Array.from({ length: 5 }).map((_, i) => (
            <Star
              key={i}
              data-testid="star-icon"
              className={`h-5 w-5 ${
                i < Math.floor(rating) ? "fill-current text-primary" : "text-muted-foreground"
              }`}
            />
          ))}
        </div>
        <div className="flex flex-col items-center gap-1 text-sm text-foreground-subtle">
          {phones.map((phone, i) => (
            <span key={i}>{phone}</span>
          ))}
        </div>
      </div>

      {/* Main Info */}
      <div className="flex flex-1 flex-col">
        <div className="flex w-full items-start justify-between">
          <div className="flex flex-col">
            <h1 className="text-2xl font-bold tracking-tight text-primary">
              Olá, <span className="text-foreground">{name}!</span>
            </h1>
            <p className="mt-1 text-sm font-medium">
              Seu perfil está{" "}
              <span className={isOnline ? "text-emerald-500" : "text-destructive"}>
                {isOnline ? "online!" : "desativado"}
              </span>
            </p>
            <p className="text-sm text-foreground-subtle">{email}</p>
          </div>

          <div className="flex items-center gap-2 text-primary">
            <Button variant="ghost" size="sm" asChild className="h-9 w-9 px-0">
              <Link href="/alterar-dados">
                <Pencil className="h-5 w-5" />
                <span className="sr-only">Editar perfil</span>
              </Link>
            </Button>
            <Button variant="ghost" size="sm" asChild className="h-9 w-9 px-0">
              <Link href="/configuracoes">
                <Settings className="h-5 w-5" />
                <span className="sr-only">Configurações</span>
              </Link>
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
