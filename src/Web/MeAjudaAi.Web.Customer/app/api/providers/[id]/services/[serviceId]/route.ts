import { auth } from "@/auth";
import { NextRequest, NextResponse } from "next/server";

export async function POST(
    req: NextRequest,
    { params }: { params: Promise<{ id: string; serviceId: string }> }
) {
    const session = await auth();
    if (!session?.accessToken) {
        return new NextResponse("Unauthorized", { status: 401 });
    }

    const { id, serviceId } = await params;
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002';

    try {
        const res = await fetch(`${apiUrl}/api/v1/providers/${id}/services/${serviceId}`, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${session.accessToken}`
            }
        });

        if (!res.ok) {
            return new NextResponse(res.statusText, { status: res.status });
        }

        // Return empty or json depending on API. Assuming void/success.
        return new NextResponse(null, { status: 200 });
    } catch (error) {
        console.error(`Error proxying POST /providers/${id}/services/${serviceId}:`, error);
        return new NextResponse("Internal Server Error", { status: 500 });
    }
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
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002';

    try {
        const res = await fetch(`${apiUrl}/api/v1/providers/${id}/services/${serviceId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${session.accessToken}`
            }
        });

        if (!res.ok) {
            return new NextResponse(res.statusText, { status: res.status });
        }

        return new NextResponse(null, { status: 200 });
    } catch (error) {
        console.error(`Error proxying DELETE /providers/${id}/services/${serviceId}:`, error);
        return new NextResponse("Internal Server Error", { status: 500 });
    }
}
