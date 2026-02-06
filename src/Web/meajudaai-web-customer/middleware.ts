import { auth } from "@/auth"

export default auth((req) => {
    const isLoggedIn = !!req.auth
    const isOnProfile = req.nextUrl.pathname.startsWith("/perfil")

    if (isOnProfile && !isLoggedIn) {
        return Response.redirect(new URL("/api/auth/signin", req.nextUrl))
    }
})

export const config = {
    matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
}
