import Link from "next/link";
import { Pencil } from "lucide-react";
import { Button } from "../ui/button";

interface ProfileDescriptionProps {
  description: string;
}

export function ProfileDescription({ description }: ProfileDescriptionProps) {
  return (
    <div className="mt-8">
      <div className="mb-2 flex items-center gap-2">
        <h2 className="text-base font-bold text-foreground">Minha descrição</h2>
        <Button variant="ghost" size="sm" asChild className="h-6 w-6 px-0 text-primary hover:text-primary-hover">
          <Link href="/alterar-dados#description">
            <Pencil className="h-4 w-4" />
            <span className="sr-only">Editar descrição</span>
          </Link>
        </Button>
      </div>
      <p className="text-sm leading-relaxed text-foreground-subtle">
        {description}
      </p>
    </div>
  );
}
