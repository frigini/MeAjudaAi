import { auth } from "@/auth";
import { NextRequest, NextResponse } from "next/server";

export async function PUT(req: NextRequest) {
    const session = await auth();
    if (!session?.accessToken) {
        return new NextResponse("Unauthorized", { status: 401 });
    }

    try {
        const body = await req.json();
        const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002';

        const res = await fetch(`${apiUrl}/api/v1/providers/me`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${session.accessToken}`
            },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            return new NextResponse(res.statusText, { status: res.status });
        }

        // Check for empty response (e.g., 204 No Content)
        const contentType = res.headers.get("content-type");
        const contentLength = res.headers.get("content-length");

        if (res.status === 204 || contentLength === "0" || !contentType?.includes("application/json")) {
            return new NextResponse(null, { status: res.status });
        }

        const data = await res.json();
        return NextResponse.json(data);
    } catch (error) {
        console.error("Error proxying PUT /providers/me:", error);
        return new NextResponse("Internal Server Error", { status: 500 });
    }
}
