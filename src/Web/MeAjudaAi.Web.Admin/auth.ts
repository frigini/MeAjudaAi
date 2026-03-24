import NextAuth, { type NextAuthOptions, type DefaultSession } from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

declare module "next-auth" {
  interface Session extends DefaultSession {
    user: {
      id?: string;
      roles?: string[];
    } & DefaultSession["user"];
  }
}

const keycloakClientId = process.env.KEYCLOAK_ADMIN_CLIENT_ID;
const keycloakClientSecret = process.env.KEYCLOAK_ADMIN_CLIENT_SECRET;
const keycloakIssuer = process.env.KEYCLOAK_ISSUER;

if (!keycloakClientId || !keycloakClientSecret || !keycloakIssuer) {
  throw new Error("Missing Keycloak environment variables: KEYCLOAK_ADMIN_CLIENT_ID, KEYCLOAK_ADMIN_CLIENT_SECRET, or KEYCLOAK_ISSUER");
}

export const authOptions: NextAuthOptions = {
  providers: [
    Keycloak({
      clientId: keycloakClientId,
      clientSecret: keycloakClientSecret,
      issuer: keycloakIssuer,
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
    async jwt({ token, user, account, profile }) {
      if (account && profile) {
        const keycloakProfile = profile as {
          sub?: string;
          realm_access?: { roles?: string[] };
        };
        token.id = keycloakProfile.sub ?? profile.sub;
        token.roles = keycloakProfile.realm_access?.roles ?? [];
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.exp = account.expires_at;
      }
      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.id = token.id as string | undefined;
        session.user.roles = token.roles as string[] | undefined;
      }
      return session;
    },
  },
};

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
