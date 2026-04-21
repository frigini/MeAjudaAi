"use client";

import { Skeleton } from '@/components/ui/skeleton';
import { Card } from '@/components/ui/card';
import { useTranslation } from 'react-i18next';

export default function Loading() {
    const { t } = useTranslation();
    return (
        <div className="container mx-auto px-4 py-8" role="status" aria-label={t('search.loadingResults')}>
            {/* Search Header Skeleton */}
            <div className="mb-8">
                <Skeleton className="h-8 w-64" />
                <Skeleton className="mt-2 h-4 w-full max-w-[384px] sm:w-96" />
            </div>

            {/* Provider Grid Skeleton */}
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {[...Array(6)].map((_, i) => (
                    <Card key={i} className="p-6 h-full flex flex-col">
                        <div className="flex items-start gap-4">
                            {/* Avatar */}
                            <Skeleton className="h-16 w-16 rounded-full flex-shrink-0" />
                            
                            <div className="flex-1 min-w-0">
                                {/* Name and Badge */}
                                <div className="flex items-center gap-2">
                                    <Skeleton className="h-6 w-32" />
                                    <Skeleton className="h-5 w-5 rounded-full" />
                                </div>

                                {/* Rating */}
                                <div className="flex items-center gap-2 mt-2">
                                    <Skeleton className="h-4 w-20" />
                                    <Skeleton className="h-3 w-16" />
                                </div>

                                {/* Services */}
                                <div className="flex flex-wrap gap-2 mt-3">
                                    <Skeleton className="h-6 w-20 rounded-full" />
                                    <Skeleton className="h-6 w-24 rounded-full" />
                                    <Skeleton className="h-6 w-16 rounded-full" />
                                </div>

                                {/* Location */}
                                <div className="mt-4 flex items-center gap-2">
                                    <Skeleton className="h-4 w-4 rounded-full" />
                                    <Skeleton className="h-4 w-40" />
                                </div>
                            </div>
                        </div>
                    </Card>
                ))}
            </div>
        </div>
    );
}
