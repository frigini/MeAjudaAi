import { Metadata } from "next";
import { ScheduleManager } from "@/components/dashboard/schedule-manager";

export const metadata: Metadata = {
    title: "Minha Agenda | MeAjudaAí",
    description: "Gerencie seus horários de atendimento.",
};

export default function AgendaPage() {
    return (
        <div className="container mx-auto py-10 px-4">
            <ScheduleManager />
        </div>
    );
}
