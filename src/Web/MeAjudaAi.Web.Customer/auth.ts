import NextAuth from "next-auth"
import Keycloak from "next-auth/providers/keycloak"
import { JWT } from "next-auth/jwt"

async function refreshAccessToken(token: JWT): Promise<JWT> {
    try {
        const response = await fetch(`${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/token`, {
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
                client_id: process.env.KEYCLOAK_CLIENT_ID ?? "",
                client_secret: process.env.KEYCLOAK_CLIENT_SECRET ?? "",
                grant_type: "refresh_token",
                refresh_token: token.refreshToken ?? "",
            }),
            method: "POST",
        })

        const refreshedTokens = await response.json()

        if (!response.ok) {
            throw refreshedTokens
        }

        return {
            ...token,
            accessToken: refreshedTokens.access_token,
            expiresAt: Date.now() + (refreshedTokens.expires_in * 1000),
            refreshToken: refreshedTokens.refresh_token ?? token.refreshToken, // Fall back to old refresh token
        }
    } catch (error) {
        console.error("RefreshAccessTokenError", error)

        return {
            ...token,
            error: "RefreshAccessTokenError",
        }
    }
}

export const { handlers, signIn, signOut, auth } = NextAuth({
    providers: [
        Keycloak({
            clientId: process.env.KEYCLOAK_CLIENT_ID ?? "",
            clientSecret: process.env.KEYCLOAK_CLIENT_SECRET ?? "",
            issuer: process.env.KEYCLOAK_ISSUER ?? "",
        }),
    ],
    pages: {
        signIn: "/auth/signin",
    },
    callbacks: {
        async jwt({ token, account, profile }) {
            // Initial sign in
            if (account && profile) {
                return {
                    ...token,
                    accessToken: account.access_token ?? "",
                    refreshToken: account.refresh_token ?? "",
                    expiresAt: account.expires_at ? account.expires_at * 1000 : Date.now() + 3600000,
                    id: profile.sub ?? "",
                }
            }

            // Return previous token if the access token has not expired yet
            if (Date.now() < (token.expiresAt ?? 0)) {
                return token
            }

            // Access token has expired, try to update it
            return refreshAccessToken(token)
        },
        async session({ session, token }) {
            if (token?.accessToken) {
                (session as any).accessToken = token.accessToken
            }
            if (token?.id && session.user) {
                session.user.id = token.id as string
            }
            if (token?.error) {
                (session as any).error = token.error as string
            }
            return session
        },
        async redirect({ url, baseUrl }) {
            // Rejeitar URLs protocolares relativas (ex: //attacker.com) para evitar Open Redirect
            if (url.startsWith("//")) {
                return baseUrl
            }
            // Permite URLs que começam com a baseUrl ou caminhos relativos
            if (url.startsWith(baseUrl)) return url
            else if (url.startsWith("/")) return new URL(url, baseUrl).toString()
            return baseUrl
        },
    },
    // Debug in development
    debug: process.env.NODE_ENV === "development",
});
