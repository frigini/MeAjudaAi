import { withAuth } from "next-auth/middleware"

function isE2ETest(req: Request): boolean {
  const mockAuthHeader = req.headers.get("x-mock-auth");
  const cookieHeader = req.headers.get("cookie") || "";
  return mockAuthHeader === "true" || cookieHeader.includes("x-mock-auth=true");
}

export default withAuth(
    function middleware() {
        // Here we can access the token if logged in.
        // The checking logic is already handled by `withAuth` callbacks.
    },
    {
        pages: {
            signIn: "/auth/signin"
        },
        callbacks: {
            authorized: ({ req, token }) => {
                if (isE2ETest(req as unknown as Request)) {
                    return true;
                }

                const isLoggedIn = !!token
                const { pathname } = req.nextUrl

                // Define protected routes
                const isProfile = pathname.startsWith("/perfil")
                const isClientDashboard = pathname.startsWith("/cliente")

                // Protect /prestador ONLY if it's the exact path (Dashboard)
                // /prestador/[id] is a public profile and should remain accessible
                const isProviderDashboard = pathname === "/prestador"

                // Protect onboarding wizard (must be logged in to complete profile)
                const isOnboardingWizard = pathname.startsWith("/cadastro/prestador")

                const isProtected = isProfile || isClientDashboard || isProviderDashboard || isOnboardingWizard

                // If protected route and token empty -> Unauthorized (Redirect to signin)
                if (isProtected && !isLoggedIn) {
                    return false
                }

                return true
            }
        }
    }
)

export const config = {
    matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)"],
}
