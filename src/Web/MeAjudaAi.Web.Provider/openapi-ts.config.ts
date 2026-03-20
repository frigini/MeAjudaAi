import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    input: '../../api/api-spec.json',
    output: './lib/api/generated',
    plugins: [
        '@tanstack/react-query',
        'zod',
    ],
});
