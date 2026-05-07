import { defineConfig, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";
import { resolve } from "path";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  // 加载环境变量
  const env = loadEnv(mode, process.cwd(), '');
  
  // 根据环境模式设置 base 路径
  // 开发环境使用根路径，生产环境使用相对路径
  // 可以通过环境变量 VITE_BASE_URL 覆盖默认值
  const getBasePath = () => {
    // 如果设置了环境变量，优先使用环境变量
    if (env.VITE_BASE_URL) {
      return env.VITE_BASE_URL;
    }
    
    // 开发环境使用根路径
    if (mode === 'development') {
      return '/';
    }
    
    // 生产环境使用根路径（如果部署在子路径，通过环境变量 VITE_BASE_URL 设置）
    return '/';
  };

  const basePath = getBasePath();
  
  return {
    plugins: [
      vue(),
      // 自定义插件：确保 favicon 路径正确
      {
        name: 'transform-favicon',
        transformIndexHtml(html) {
          // 根据 base 配置动态设置 favicon 路径
          const faviconPath = basePath === '/' ? '/favicon.ico' : `${basePath}favicon.ico`;
          return html.replace(
            /<link\s+rel="icon"[^>]*>/i,
            `<link rel="icon" type="image/x-icon" href="${faviconPath}">`
          );
        },
      },
    ],
    base: basePath,
    resolve: {
      alias: {
        "@": resolve(__dirname, "src"),
      },
    },
    server: {
      port: 10001,
      host: true,
      open: true,
      // Vite 默认支持 SPA 路由，但需要确保正确处理
      // 这个配置确保所有未匹配的路由都返回 index.html
      strictPort: false,
    },
    // 确保构建时正确处理资源路径
    build: {
      assetsDir: 'assets',
      // 确保资源路径正确
      rollupOptions: {
        output: {
          // 确保 chunk 文件名正确
          chunkFileNames: 'assets/js/[name]-[hash].js',
          entryFileNames: 'assets/js/[name]-[hash].js',
          assetFileNames: 'assets/[ext]/[name]-[hash].[ext]',
        },
      },
    },
  };
});
