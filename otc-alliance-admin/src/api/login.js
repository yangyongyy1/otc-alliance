import request from "@/utils/request";

export function login(data) {
  return request({
    url: "/api/auth/login",
    method: "post",
    data,
  });
}

export function getCaptcha() {
  return request({
    url: "/api/auth/captcha",
    method: "get",
  });
}

export function logout() {
  return request({
    url: "/api/auth/logout",
    method: "post",
  });
}
