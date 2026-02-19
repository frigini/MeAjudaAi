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

    const isProtected = isProfile || isClientDashboard || isProviderDashboard

    if (isProtected && !isLoggedIn) {
        const callbackUrl = encodeURIComponent(pathname + search);
        return Response.redirect(new URL(`/api/auth/signin?callbackUrl=${callbackUrl}`, req.nextUrl))
    }
})

export const config = {
    matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
}
