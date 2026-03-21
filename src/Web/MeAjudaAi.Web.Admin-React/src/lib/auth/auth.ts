import NextAuth, { type NextAuthOptions, type DefaultSession } from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

declare module "next-auth" {
  interface Session extends DefaultSession {
    user: {
      roles?: string[];
    } & DefaultSession["user"];
  }
}

export const authOptions: NextAuthOptions = {
  providers: [
    Keycloak({
      clientId: process.env.KEYCLOAK_ADMIN_CLIENT_ID ?? "meajudaai-admin",
      clientSecret: process.env.KEYCLOAK_ADMIN_CLIENT_SECRET ?? "",
      issuer: process.env.KEYCLOAK_ISSUER,
    }),
  ],
  pages: {
    signIn: "/login",
    error: "/login",
  },
  session: {
    strategy: "jwt",
    maxAge: 30 * 60,
  },
  callbacks: {
    async jwt({ token, account, profile }) {
      if (account && profile) {
        const keycloakProfile = profile as { realm_access?: { roles?: string[] } };
        token.roles = keycloakProfile.realm_access?.roles ?? [];
      }
      return token;
    },
    async session({ session, token }) {
      session.user.roles = token.roles as string[] | undefined;
      return session;
    },
  },
};

export { auth as middleware } from "@/lib/auth/auth";

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
