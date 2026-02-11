import { auth } from "@/auth";
import { NextRequest, NextResponse } from "next/server";

export async function POST(
    req: NextRequest,
    { params }: { params: Promise<{ id: string }> } // In Next.js 15 params are async
) {
    const session = await auth();
    if (!session?.accessToken) {
        return new NextResponse("Unauthorized", { status: 401 });
    }

    const { id } = await params;
    // We need serviceId from body or query? 
    // The previous implementation was POST .../services/{serviceId}
    // But here we are in /api/providers/[id]/services/route.ts? No.
    // I should match the usage: /api/v1/providers/${provider.id}/services/${newServiceId}
    // So the route should be app/api/providers/[id]/services/[serviceId]/route.ts
    // Wait, POST is usually to add a service to a collection. The previous code was POST .../services/{serviceId}.
    // If I put this in [serviceId]/route.ts I can handle POST and DELETE there.
    return new NextResponse("Method not allowed", { status: 405 });
}
