import { withAuth } from "next-auth/middleware";

function isE2ETest(req: Request): boolean {
  const mockAuthHeader = req.headers.get("x-mock-auth");
  const cookieHeader = req.headers.get("cookie") || "";
  return mockAuthHeader === "true" || cookieHeader.includes("x-mock-auth=true");
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
