import NextAuth from "next-auth"
import Keycloak from "next-auth/providers/keycloak"
import { JWT } from "next-auth/jwt"

async function refreshAccessToken(token: JWT): Promise<JWT> {
    try {
        const response = await fetch(`${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/token`, {
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
                client_id: process.env.KEYCLOAK_CLIENT_ID ?? "",
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
    callbacks: {
        async jwt({ token, account, profile }) {
            // Initial sign in
            if (account && profile) {
                return {
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
                session.accessToken = token.accessToken
            }
            if (token?.id) {
                session.user.id = token.id
            }
            if (token?.error) {
                session.error = token.error as string
            }
            return session
        },
    },
    // Debug in development
    debug: process.env.NODE_ENV === "development",
    pages: {
        signIn: "/auth/signin",
    },
})
