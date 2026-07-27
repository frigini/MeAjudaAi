import { withAuth } from "next-auth/middleware";

function isE2ETest(req: Request): boolean {
  if (process.env.E2E_MOCK_AUTH_ENABLED !== "true") return false;
  const mockAuthHeader = req.headers.get("x-mock-auth");
  if (mockAuthHeader === "true") return true;
  const cookieHeader = req.headers.get("cookie") || "";
  return /(?:^|;\s*)x-mock-auth=true(?:;|$)/.test(cookieHeader);
}

export default withAuth({
  pages: {
    signIn: "/login",
  },
  callbacks: {
    authorized: ({ req, token }) => {
      if (isE2ETest(req as unknown as Request)) {
        return true;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: [
    "/((?!api|_next/static|_next/image|favicon.ico|login|auth|onboarding|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)",
  ],
};
