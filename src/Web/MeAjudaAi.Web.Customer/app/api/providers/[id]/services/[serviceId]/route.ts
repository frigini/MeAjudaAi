import { auth } from "@/auth";
import { NextRequest, NextResponse } from "next/server";

/**
 * Shared helper to proxy authenticated requests to the backend API
 */
async function proxyProviderServiceRequest(
    method: "POST" | "DELETE",
    providerId: string,
    serviceId: string,
    accessToken: string
): Promise<NextResponse> {
    const apiUrl = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002';

    try {
        const res = await fetch(`${apiUrl}/api/v1/providers/${providerId}/services/${serviceId}`, {
            method,
            headers: {
                "Authorization": `Bearer ${accessToken}`
            }
        });

        if (!res.ok) {
            return new NextResponse(res.statusText, { status: res.status });
        }

        // Forward successful status (201, 204, etc) and body if present
        const body = res.status === 204 ? null : await res.text();
        return new NextResponse(body, {
            status: res.status,
            headers: { 'Content-Type': res.headers.get('Content-Type') || 'application/json' }
        });
    } catch (error) {
        console.error(`Error proxying ${method} /providers/${providerId}/services/${serviceId}:`, error);
        return new NextResponse("Internal Server Error", { status: 500 });
    }
}

export async function POST(
    req: NextRequest,
    { params }: { params: Promise<{ id: string; serviceId: string }> }
) {
    const session = await auth();
    if (!session?.accessToken) {
        return new NextResponse("Unauthorized", { status: 401 });
    }

    const { id, serviceId } = await params;
    return proxyProviderServiceRequest("POST", id, serviceId, session.accessToken);
}

export async function DELETE(
    req: NextRequest,
    { params }: { params: Promise<{ id: string; serviceId: string }> }
) {
    const session = await auth();
    if (!session?.accessToken) {
        return new NextResponse("Unauthorized", { status: 401 });
    }

    const { id, serviceId } = await params;
    return proxyProviderServiceRequest("DELETE", id, serviceId, session.accessToken);
}
