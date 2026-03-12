import { DefaultSession } from "next-auth";

declare module "next-auth" {
    /**
     * Returned by `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
     */
    interface Session {
        /** The user's access token from Keycloak. */
        accessToken?: string;
        /** Any refresh error that might have occurred. */
        error?: string;
        user: {
            /** The user's internal database UUID. */
            id: string;
        } & DefaultSession["user"];
    }

    /**
     * Extends the built-in User type with fields returned by Keycloak via ROPC flow.
     * Avoids manual type assertions in the `jwt` callback when `account.provider === "credentials"`.
     */
    interface User {
        /** Keycloak access token (ROPC flow). */
        accessToken?: string;
        /** Keycloak refresh token (ROPC flow). */
        refreshToken?: string;
        /** Access token expiry as UNIX timestamp (ms). */
        expiresAt?: number;
    }
}

declare module "next-auth/jwt" {
    /** Returned by the `jwt` callback and `auth`, when using JWT sessions */
    interface JWT {
        accessToken?: string;
        refreshToken?: string;
        expiresAt?: number;
        id: string;
        error?: string;
    }
}
