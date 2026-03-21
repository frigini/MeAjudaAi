import NextAuth, { type NextAuthOptions } from "next-auth";
import Keycloak from "next-auth/providers/keycloak";
import Credentials from "next-auth/providers/credentials";
import { JWT } from "next-auth/jwt";

export const authOptions: NextAuthOptions = {
  providers: [
    Keycloak({
      clientId: process.env.KEYCLOAK_ADMIN_CLIENT_ID ?? "meajudaai-admin",
      clientSecret: process.env.KEYCLOAK_ADMIN_CLIENT_SECRET ?? "",
      issuer: process.env.KEYCLOAK_ISSUER,
    }),
    Credentials({
      name: "Development",
      credentials: {
        username: { label: "Username", type: "text", placeholder: "admin" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        if (
          process.env.NODE_ENV === "development" &&
          credentials?.username === "admin" &&
          credentials?.password === "admin"
        ) {
          return {
            id: "dev-admin-id",
            name: "Dev Admin",
            email: "admin@meajudaai.local",
            roles: ["admin"],
          };
        }
        return null;
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      if (account && account.provider === "keycloak") {
        const keycloakToken = token as JWT & {
          refresh_token?: string;
          expires_at?: number;
        };
        token.accessToken = account.access_token;
        token.refreshToken = keycloakToken.refresh_token;
        token.accessTokenExpires = keycloakToken.expires_at
          ? keycloakToken.expires_at * 1000
          : Date.now() + 3600 * 1000;

        const realmAccess = (profile as { realm_access?: { roles?: string[] } })
          ?.realm_access;
        token.roles = realmAccess?.roles ?? [];
      }
      return token;
    },
    async session({ session, token }) {
      return {
        ...session,
        accessToken: token.accessToken as string,
        refreshToken: token.refreshToken as string,
        accessTokenExpires: token.accessTokenExpires as number,
        user: {
          ...session.user,
          id: token.sub,
          roles: token.roles as string[],
        },
      };
    },
  },
  pages: {
    signIn: "/login",
    error: "/login",
  },
  session: {
    strategy: "jwt",
    maxAge: 30 * 60,
  },
};

export default NextAuth(authOptions);
