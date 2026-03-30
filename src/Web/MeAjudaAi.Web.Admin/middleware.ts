import { withAuth } from "next-auth/middleware";

function isE2ETest(request: Request): boolean {
  return request.headers.get("x-mock-auth") === "true";
}

export default withAuth({
  pages: {
    signIn: "/login",
  },
  callbacks: {
    authorized: ({ req, token }) => {
      if (isE2ETest(req)) {
        return true;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|login).*)"],
};
