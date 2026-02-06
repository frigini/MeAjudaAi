import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    input: 'http://localhost:7002/api-docs/v1/swagger.json',
    output: './lib/api/generated',
    plugins: [
        '@tanstack/react-query',
        'zod',
    ],
});
