export const GetAllForSelectItemByType = {
  url: "/api/services/app/User/GetAllForSelectItem",
  method: "get",
  params: {},
};

// 根据类型获取商户列表
export function getBusinessByType(businessType) {
  return {
    url: "/api/services/app/Business/GetBusinessByType",
    method: "get",
    params: {
      businessType
    },
  };
}

// 获取代理列表
export function getAgentSelectItems() {
  return {
    url: "/api/services/app/Agent/GetAgentSelectItems",
    method: "get",
    params: {},
  };
}