"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Rating } from "@/components/ui/rating";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";

const reviewSchema = z.object({
    rating: z.number().min(1, "Selecione uma avaliação").max(5),
    comment: z.string().min(10, "Comentário deve ter pelo menos 10 caracteres"),
});

type ReviewFormValues = z.infer<typeof reviewSchema>;

interface ReviewFormProps {
    providerId: string;
    onSuccess?: () => void;
}

export function ReviewForm({ providerId, onSuccess }: ReviewFormProps) {
    const [isSubmitting, setIsSubmitting] = useState(false);

    const form = useForm<ReviewFormValues>({
        resolver: zodResolver(reviewSchema),
        defaultValues: {
            rating: 0,
            comment: "",
        },
    });

    async function onSubmit(data: ReviewFormValues) {
        setIsSubmitting(true);
        try {
            // Mock API call
            await new Promise((resolve) => setTimeout(resolve, 1000));

            console.log("Submitting review for provider", providerId, data);

            toast.success("Avaliação enviada!", {
                description: "Obrigado pelo seu feedback.",
            });

            form.reset();
            onSuccess?.();
        } catch (error) {
            toast.error("Erro ao enviar avaliação", {
                description: "Tente novamente mais tarde.",
            });
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4 mb-8 p-6 bg-muted/30 rounded-lg">
                <h3 className="text-lg font-semibold mb-2">Avaliar este prestador</h3>

                <FormField
                    control={form.control}
                    name="rating"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Sua nota</FormLabel>
                            <FormControl>
                                <Rating
                                    value={field.value}
                                    onChange={field.onChange}
                                    size="lg"
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="comment"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Seu comentário</FormLabel>
                            <FormControl>
                                <Textarea
                                    placeholder="Conte como foi sua experiência com este profissional..."
                                    className="resize-none"
                                    rows={4}
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="flex justify-end">
                    <Button type="submit" disabled={isSubmitting}>
                        {isSubmitting ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Enviando...
                            </>
                        ) : (
                            "Enviar Avaliação"
                        )}
                    </Button>
                </div>
            </form>
        </Form>
    );
}
