import NextAuth from "next-auth"
import { authOptions, validateCriticalEnvOnStartup } from "@/auth"

const nextAuthHandler = NextAuth(authOptions)

export const GET = (req: Request, res: any) => {
    validateCriticalEnvOnStartup();
    return nextAuthHandler(req, res);
}

export const POST = (req: Request, res: any) => {
    validateCriticalEnvOnStartup();
    return nextAuthHandler(req, res);
}
