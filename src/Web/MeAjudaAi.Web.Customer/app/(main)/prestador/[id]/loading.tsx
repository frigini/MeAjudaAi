import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
    return (
        <div className="container mx-auto px-4 py-8" role="status" aria-live="polite">
            <span className="sr-only">Carregando perfil do prestador...</span>
            <div className="max-w-4xl mx-auto">
                <div className="grid grid-cols-1 md:grid-cols-12 gap-8 mb-12">
                    {/* Sidebar Area */}
                    <div className="md:col-span-3 flex flex-col items-center space-y-4">
                        {/* Avatar Skeleton */}
                        <Skeleton className="h-32 w-32 rounded-full border-4 border-white shadow-md" />
                        
                        {/* Rating Skeleton */}
                        <div className="flex items-center gap-2">
                            <Skeleton className="h-5 w-24" />
                            <Skeleton className="h-4 w-16" />
                        </div>

                        {/* Contact Area Skeleton */}
                        <div className="w-full space-y-2 mt-4">
                            <Skeleton className="h-10 w-full rounded-lg" />
                            <Skeleton className="h-10 w-full rounded-lg" />
                        </div>
                    </div>

                    {/* Main Content Area */}
                    <div className="md:col-span-9 space-y-6">
                        {/* Title and Badge */}
                        <div className="flex items-center gap-3">
                            <Skeleton className="h-10 w-64" />
                            <Skeleton className="h-10 w-32 rounded-full" />
                        </div>

                        {/* Email/Sub-info */}
                        <Skeleton className="h-4 w-48" />
                        
                        {/* Location */}
                        <div className="flex items-center gap-2">
                            <Skeleton className="h-4 w-4 rounded-full" />
                            <Skeleton className="h-4 w-40" />
                        </div>

                        {/* Description */}
                        <div className="space-y-3">
                            <Skeleton className="h-4 w-full" />
                            <Skeleton className="h-4 w-full" />
                            <Skeleton className="h-4 w-3/4" />
                        </div>

                        {/* Services */}
                        <div className="pt-4">
                            <Skeleton className="h-6 w-32 mb-3" />
                            <div className="flex flex-wrap gap-2">
                                <Skeleton className="h-8 w-20 rounded-full" />
                                <Skeleton className="h-8 w-24 rounded-full" />
                                <Skeleton className="h-8 w-16 rounded-full" />
                            </div>
                        </div>
                    </div>
                </div>

                {/* Reviews Area Skeleton */}
                <div className="pt-8 border-t border-gray-200">
                    <Skeleton className="h-8 w-40 mb-6" />
                    <div className="space-y-6">
                        <Skeleton className="h-32 w-full rounded-lg" />
                        <div className="space-y-4">
                            <Skeleton className="h-24 w-full rounded-lg" />
                            <Skeleton className="h-24 w-full rounded-lg" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
