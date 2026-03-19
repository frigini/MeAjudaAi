import { Plus } from "lucide-react";

interface ProfileServicesProps {
  services: string[];
}

export function ProfileServices({ services }: ProfileServicesProps) {
  return (
    <div className="mt-8">
      <h2 className="mb-4 text-base font-bold text-foreground">Meus serviços</h2>
      
      <div className="mb-4 flex items-center gap-2">
        <input
          type="text"
          placeholder="Digite um novo serviço aqui"
          className="w-full max-w-sm rounded-md border border-border bg-surface px-3 text-sm h-8 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
        />
        <button 
          type="button"
          className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-primary-foreground hover:bg-primary-hover"
        >
          <Plus className="h-4 w-4" />
          <span className="sr-only">Adicionar serviço</span>
        </button>
      </div>

      <div className="flex flex-wrap gap-2">
        {services.map((service, index) => (
          <div
            key={index}
            className="flex items-center gap-1 rounded-md bg-primary px-3 py-1 text-xs font-medium text-primary-foreground"
          >
            {service}
            <button
              type="button"
              className="ml-1 opacity-70 hover:opacity-100"
            >
              &times;
              <span className="sr-only">Remover {service}</span>
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
