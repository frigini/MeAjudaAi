import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary (Azul escuro - do Figma #355873)
        primary: {
          DEFAULT: "#355873",
          foreground: "#FFFFFF",
          hover: "#2a4660",
        },
        // Secondary (Laranja - do Figma #D06704)
        secondary: {
          DEFAULT: "#D06704",
          light: "#F2AE72",
          foreground: "#FFFFFF",
          hover: "#b85703",
        },
        // Neutral
        surface: {
          DEFAULT: "#FFFFFF",
          raised: "#F5F5F5",
        },
        foreground: {
          DEFAULT: "#2E2E2E",
          subtle: "#666666",
        },
        border: "#E0E0E0",
        input: "#E0E0E0",
        ring: "#D06704",
        // Destructive
        destructive: {
          DEFAULT: "#DC2626",
          foreground: "#FFFFFF",
        },
      },
    },
  },
  plugins: [],
};

export default config;
