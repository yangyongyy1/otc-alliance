const viewModules = import.meta.glob("@/views/**/*.vue");

export function loadView(path) {
  const fullPath = `/src/views/${path}.vue`;
  const loader = viewModules[fullPath];

  if (!loader) {
    return () => import("@/views/error/404.vue");
  }

  return loader;
}
