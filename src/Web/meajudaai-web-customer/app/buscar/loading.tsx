import { Card } from '@/components/ui/card';

export default function Loading() {
    return (
        <div className="container mx-auto px-4 py-8">
            {/* Search Header Skeleton */}
            <div className="mb-8">
                <div className="h-8 w-64 animate-pulse rounded bg-surface-raised" />
                <div className="mt-2 h-4 w-96 animate-pulse rounded bg-surface-raised" />
            </div>

            {/* Provider Grid Skeleton */}
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {[...Array(6)].map((_, i) => (
                    <Card key={i} className="p-6">
                        {/* Avatar */}
                        <div className="mb-4 flex items-center gap-4">
                            <div className="h-16 w-16 animate-pulse rounded-full bg-surface-raised" />
                            <div className="flex-1">
                                <div className="mb-2 h-5 w-32 animate-pulse rounded bg-surface-raised" />
                                <div className="h-4 w-24 animate-pulse rounded bg-surface-raised" />
                            </div>
                        </div>

                        {/* Services */}
                        <div className="mb-4 flex flex-wrap gap-2">
                            <div className="h-6 w-20 animate-pulse rounded-full bg-surface-raised" />
                            <div className="h-6 w-24 animate-pulse rounded-full bg-surface-raised" />
                            <div className="h-6 w-16 animate-pulse rounded-full bg-surface-raised" />
                        </div>

                        {/* Location */}
                        <div className="h-4 w-40 animate-pulse rounded bg-surface-raised" />
                    </Card>
                ))}
            </div>
        </div>
    );
}
