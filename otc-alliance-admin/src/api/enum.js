import request from "@/utils/request";

/**
 * 获取枚举值
 * @param {string} enumName - 枚举名称
 * @returns {Promise} 返回枚举数据
 */
export const getEnum = (enumName) => {
  return request({
    url: "/api/services/app/Basic/GetEnum",
    method: "get",
    params: {
      enumName,
    },
  });
};

/**
 * 获取多个枚举值
 * @param {string[]} enumNames - 枚举名称数组
 * @returns {Promise} 返回多个枚举数据
 */
export const getEnums = (enumNames) => {
  return Promise.all(enumNames.map((name) => getEnum(name)));
};
