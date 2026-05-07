import { defineStore } from "pinia";
import { login, getUserInfo } from "@/api/user";

export const useUserStore = defineStore("user", {
  state: () => ({
    token: localStorage.getItem("token") || "",
    userInfo: JSON.parse(localStorage.getItem("userInfo") || "null"),
    btnPermissions: [],
  }),

  getters: {
    isLoggedIn: (state) => !!state.token,
  },

  actions: {
    // 登录
    async login(loginData) {
      try {
        const res = await login(loginData);
        if (res && res.result) {
          this.token = res.result.token;
          localStorage.setItem("token", res.result.token);
          return true;
        }
        return false;
      } catch (error) {
        return false;
      }
    },

    // 获取用户信息
    async getUserInfo() {
      try {
        const res = await getUserInfo();
        if (res && res.result) {
          this.userInfo = res.result;
          localStorage.setItem("userInfo", JSON.stringify(res.result));
          return res.result;
        }
        return null;
      } catch (error) {
        return null;
      }
    },

    // 登出
    logout() {
      this.token = "";
      this.userInfo = null;
      localStorage.removeItem("token");
      localStorage.removeItem("userInfo");
    },
  },
});
