import { Card, CardContent, CardHeader } from "../components/ui/card";

export default function Loading() {
  return (
    <div className="container mx-auto max-w-5xl py-12 px-4 sm:px-6 lg:px-8">
      <header className="mb-8">
        <div className="h-9 w-64 animate-pulse rounded-md bg-muted" />
        <div className="mt-4 h-6 w-96 animate-pulse rounded-md bg-muted" />
      </header>

      <main className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Card key={i} className={i === 2 ? "md:col-span-2 lg:col-span-1" : ""}>
            <CardHeader>
              <div className="h-6 w-32 animate-pulse rounded-md bg-muted" />
            </CardHeader>
            <CardContent className="flex flex-col gap-4">
              <div className="h-4 w-full animate-pulse rounded-md bg-muted" />
              <div className="h-4 w-3/4 animate-pulse rounded-md bg-muted" />
              <div className="mt-4 h-9 w-full animate-pulse rounded-md bg-muted" />
            </CardContent>
          </Card>
        ))}
      </main>
    </div>
  );
}
