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

function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    if (process.env.NODE_ENV !== "development") {
      console.warn(`[auth] Warning: Environment variable ${name} is missing.`);
    }
    return "";
  }
  return value;
}

const keycloakClientId = process.env.KEYCLOAK_ADMIN_CLIENT_ID || process.env.KEYCLOAK_CLIENT_ID;
const keycloakClientSecret = process.env.KEYCLOAK_ADMIN_CLIENT_SECRET || process.env.KEYCLOAK_CLIENT_SECRET;
const keycloakIssuer = process.env.KEYCLOAK_ISSUER;

if (!keycloakClientId || !keycloakClientSecret || !keycloakIssuer) {
  if (process.env.CI === "true" || process.env.NEXT_PUBLIC_CI === "true") {
    console.warn("[auth] Warning: Missing Keycloak environment variables - using placeholder values for CI build.");
  } else if (process.env.NODE_ENV === "production") {
    console.warn("[auth] Warning: Missing Keycloak environment variables - build may fail at runtime.");
  } else {
    console.warn("[auth] Warning: Missing Keycloak environment variables - using placeholder values for development.");
  }
}

export const authOptions: NextAuthOptions = {
  providers: [
    Keycloak({
      clientId: keycloakClientId || "placeholder",
      clientSecret: keycloakClientSecret || "placeholder",
      issuer: keycloakIssuer || "http://localhost:8080/realms/meajudaai",
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
