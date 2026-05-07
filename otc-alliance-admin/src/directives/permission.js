import { useUserStore } from "@/store/modules/user";

export const permission = {
  mounted(el, binding) {
    const userStore = useUserStore();
    const { value } = binding;

    // 如果没有传入权限值，则不进行权限控制
    if (!value) {
      return;
    }

    // 判断是否有权限
    const hasPermission = userStore.btnPermissions.includes(value);

    // 使用 v-show 控制显示隐藏
    el.style.display = hasPermission ? "" : "none";
  },
  updated(el, binding) {
    const userStore = useUserStore();
    const { value } = binding;

    if (!value) {
      return;
    }

    const hasPermission = userStore.btnPermissions.includes(value);
    el.style.display = hasPermission ? "" : "none";
  },
};
