import { ProfileStatusCard } from "../components/dashboard/profile-status-card";
import { ServicesConfigurationCard } from "../components/dashboard/services-configuration-card";
import { VerificationCard } from "../components/dashboard/verification-card";

export default function Index() {
  return (
    <div className="container mx-auto max-w-5xl py-12 px-4 sm:px-6 lg:px-8">
      <header className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">
          Dashboard do Prestador
        </h1>
        <p className="mt-2 text-lg text-foreground-subtle">
          Bem-vindo de volta! Aqui você pode gerenciar seu perfil e informações.
        </p>
      </header>

      <main className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <ProfileStatusCard />
        <ServicesConfigurationCard />
        <VerificationCard />
      </main>
    </div>
  );
}
