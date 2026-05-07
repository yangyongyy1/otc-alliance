import request from "@/utils/request";

// 获取所有权限
export function getPermissions() {
  return request.get("/api/services/app/Role/GetAllPermissionsWithLevel");
}

// 登录
export function login(data) {
  return request({
    url: "/api/account/login",
    method: "post",
    data,
  });
}

// 获取用户信息
export function getUserInfo(id) {
  return request({
    url: "api/services/app/User/Get",
    method: "get",
    params: {
      id,
    },
  });
}

export function setUserTimezone(data) {
  return request({
    url: "/api/services/app/User/SetUserTimeZoneValue",
    method: "post",
    data,
  });
}

// 登出
export function logout() {
  return request({
    url: "/api/account/logout",
    method: "post",
  });
}

// 更新用户信息
export function updateUserInfo(data) {
  return request({
    url: "/api/user/info",
    method: "put",
    data,
  });
}

// 修改密码（调用后端 UserAppService.ChangePassword）
export function changePassword(data) {
  return request({
    url: "/api/services/app/User/ChangePassword",
    method: "post",
    data,
  });
}
