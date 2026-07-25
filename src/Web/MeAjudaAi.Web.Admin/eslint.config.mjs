import nextConfig from "eslint-config-next";

const eslintConfig = [
  ...nextConfig,
  {
    ignores: [
      ".next/**",
      "out/**",
      "build/**",
      "coverage/**",
      "__tests__/**",
      "next-env.d.ts",
      "lib/api/generated/**",
    ],
  },
];

export default eslintConfig;
