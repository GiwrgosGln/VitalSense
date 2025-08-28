import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import viteTsconfigPaths from "vite-tsconfig-paths";
import { visualizer } from "rollup-plugin-visualizer";

export default defineConfig({
  plugins: [react(), tailwindcss(), viteTsconfigPaths()],
  build: {
    rollupOptions: {
      plugins: [visualizer({ open: true, filename: "stats.html" })],
    },
  },
});
