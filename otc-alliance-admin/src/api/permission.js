import request from "@/utils/request";

export const getRoleList = {
  url: "/api/services/app/Role/GetAll",
  method: "get",
  params: {},
};

export const deleteRole = {
  url: "/api/services/app/Role/Delete",
  method: "delete",
  data: {},
};

export const getRoleDetail = {
  url: "/api/services/app/Role/Get",
  method: "get",
  params: {},
};

export const RoleGetAllPermissionsWithLevel = {
  url: "/api/services/app/Role/GetAllPermissionsWithLevel",
  method: "get",
  params: {},
};

export const RoleUpdate = {
  url: "/api/services/app/Role/Update",
  method: "put",
  data: {},
};

export const RoleAdd = {
  url: "/api/services/app/Role/Create",
  method: "post",
  data: {},
};

export const GetRoleSelectItem = {
  url: "/api/services/app/User/GetRoles",
  method: "get",
  params: {},
};
