import { defineStore } from "pinia";

export const useTabsStore = defineStore("tabs", {
  state: () => ({
    // 访问过的页面标签列表
    visitedViews: [],
    // 缓存的页面列表（用于 keep-alive）
    cachedViews: [],
  }),

  getters: {
    // 获取所有访问过的页面
    getVisitedViews: (state) => state.visitedViews,
    // 获取所有缓存的页面
    getCachedViews: (state) => state.cachedViews,
  },

  actions: {
    // 添加标签页
    addView(view) {
      this.addVisitedView(view);
      this.addCachedView(view);
    },

    // 添加访问过的页面
    addVisitedView(view) {
      // 如果已存在，不重复添加
      if (this.visitedViews.some((v) => v.path === view.path)) return;
      
      // 添加新的标签页
      this.visitedViews.push({
        name: view.name,
        path: view.path,
        title: view.meta?.title || "未命名",
        meta: { ...view.meta },
      });
    },

    // 添加缓存页面
    addCachedView(view) {
      // 如果页面不需要缓存，直接返回
      if (view.meta?.noCache) return;
      // 如果已存在，不重复添加
      if (this.cachedViews.includes(view.name)) return;
      // 添加到缓存列表
      if (view.name) {
        this.cachedViews.push(view.name);
      }
    },

    // 删除标签页
    delView(view) {
      return new Promise((resolve) => {
        this.delVisitedView(view);
        this.delCachedView(view);
        resolve({
          visitedViews: [...this.visitedViews],
          cachedViews: [...this.cachedViews],
        });
      });
    },

    // 删除访问过的页面
    delVisitedView(view) {
      return new Promise((resolve) => {
        for (const [i, v] of this.visitedViews.entries()) {
          if (v.path === view.path) {
            this.visitedViews.splice(i, 1);
            break;
          }
        }
        resolve([...this.visitedViews]);
      });
    },

    // 删除缓存页面
    delCachedView(view) {
      return new Promise((resolve) => {
        const index = this.cachedViews.indexOf(view.name);
        if (index > -1) {
          this.cachedViews.splice(index, 1);
        }
        resolve([...this.cachedViews]);
      });
    },

    // 删除其他标签页
    delOthersViews(view) {
      return new Promise((resolve) => {
        this.delOthersVisitedViews(view);
        this.delOthersCachedViews(view);
        resolve({
          visitedViews: [...this.visitedViews],
          cachedViews: [...this.cachedViews],
        });
      });
    },

    // 删除其他访问过的页面
    delOthersVisitedViews(view) {
      return new Promise((resolve) => {
        this.visitedViews = this.visitedViews.filter((v) => {
          return v.meta?.affix || v.path === view.path;
        });
        resolve([...this.visitedViews]);
      });
    },

    // 删除其他缓存页面
    delOthersCachedViews(view) {
      return new Promise((resolve) => {
        const index = this.cachedViews.indexOf(view.name);
        if (index > -1) {
          this.cachedViews = this.cachedViews.slice(index, index + 1);
        } else {
          this.cachedViews = [];
        }
        resolve([...this.cachedViews]);
      });
    },

    // 删除所有标签页
    delAllViews() {
      return new Promise((resolve) => {
        this.delAllVisitedViews();
        this.delAllCachedViews();
        resolve({
          visitedViews: [...this.visitedViews],
          cachedViews: [...this.cachedViews],
        });
      });
    },

    // 删除所有访问过的页面（保留固定的）
    delAllVisitedViews() {
      return new Promise((resolve) => {
        // 保留固定的标签（affix 为 true 的）
        const affixTags = this.visitedViews.filter((tag) => tag.meta?.affix);
        this.visitedViews = affixTags;
        resolve([...this.visitedViews]);
      });
    },

    // 删除所有缓存页面
    delAllCachedViews() {
      return new Promise((resolve) => {
        this.cachedViews = [];
        resolve([...this.cachedViews]);
      });
    },

    // 更新访问过的页面
    updateVisitedView(view) {
      for (let v of this.visitedViews) {
        if (v.path === view.path) {
          v = Object.assign(v, view);
          break;
        }
      }
    },

    // 删除右侧标签页
    delRightViews(view) {
      return new Promise((resolve) => {
        const index = this.visitedViews.findIndex((v) => v.path === view.path);
        if (index === -1) {
          return;
        }
        this.visitedViews = this.visitedViews.filter((item, idx) => {
          if (idx <= index || item.meta?.affix) {
            return true;
          }
          const i = this.cachedViews.indexOf(item.name);
          if (i > -1) {
            this.cachedViews.splice(i, 1);
          }
          return false;
        });
        resolve({
          visitedViews: [...this.visitedViews],
        });
      });
    },

    // 删除左侧标签页
    delLeftViews(view) {
      return new Promise((resolve) => {
        const index = this.visitedViews.findIndex((v) => v.path === view.path);
        if (index === -1) {
          return;
        }
        this.visitedViews = this.visitedViews.filter((item, idx) => {
          if (idx >= index || item.meta?.affix) {
            return true;
          }
          const i = this.cachedViews.indexOf(item.name);
          if (i > -1) {
            this.cachedViews.splice(i, 1);
          }
          return false;
        });
        resolve({
          visitedViews: [...this.visitedViews],
        });
      });
    },
  },
});

