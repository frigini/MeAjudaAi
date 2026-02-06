import { GalleryVerticalEnd } from "lucide-react"
import { LoginForm } from "@/components/auth/login-form"
import Link from "next/link"

export const dynamic = "force-dynamic";

export default function LoginPage() {
    return (
        <div className="flex min-h-screen flex-col items-center justify-center gap-6 bg-muted p-6 md:p-10">
            <div className="flex w-full max-w-sm flex-col gap-6">
                <Link href="/" className="flex items-center gap-2 self-center font-medium">
                    <GalleryVerticalEnd className="size-6 text-primary" />
                    Me Ajuda Aí
                </Link>
                <LoginForm />
            </div>
        </div>
    )
}
