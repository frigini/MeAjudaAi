import { auth } from "@/auth"

export default auth((req) => {
    const isLoggedIn = !!req.auth
    const { pathname, search } = req.nextUrl

    // Define protected routes
    const isProfile = pathname.startsWith("/perfil")
    const isClientDashboard = pathname.startsWith("/cliente")

    // Protect /prestador ONLY if it's the exact path (Dashboard)
    // /prestador/[id] is a public profile and should remain accessible
    const isProviderDashboard = pathname === "/prestador"

    // Protect onboarding wizard (must be logged in to complete profile)
    const isOnboardingWizard = pathname.startsWith("/cadastro/prestador")

    const isProtected = isProfile || isClientDashboard || isProviderDashboard || isOnboardingWizard

    if (isProtected && !isLoggedIn) {
        const callbackUrl = encodeURIComponent(pathname + search);
        return Response.redirect(new URL(`/api/auth/signin?callbackUrl=${callbackUrl}`, req.nextUrl))
    }
})

export const config = {
    matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
}
