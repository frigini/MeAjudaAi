import { Header } from "@/components/layout/header";
import { Footer } from "@/components/layout/footer";

export default function AuthLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <>
            <Header />
            <main className="min-h-[calc(100vh-140px)] flex items-start justify-center pt-8 md:pt-16 bg-surface-raised pb-12">
                {children}
            </main>
            <Footer />
        </>
    );
}
