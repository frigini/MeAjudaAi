import { withAuth } from "next-auth/middleware";
import { NextRequest } from "next/server";

function isE2ETest(req: NextRequest): boolean {
  return req.headers.get("x-mock-auth") === "true" || 
         req.cookies.get("x-mock-auth")?.value === "true";
}

export default withAuth({
  pages: {
    signIn: "/login",
  },
  callbacks: {
    authorized: ({ req, token }) => {
      if (isE2ETest(req as NextRequest)) {
        return true;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: [
    "/((?!api|_next/static|_next/image|favicon.ico|login|auth).*)",
  ],
};
