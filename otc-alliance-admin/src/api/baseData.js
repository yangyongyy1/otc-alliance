// 国家地区信息相关接口

// 获取国家地区分页列表
export const getCountryInfoPaged = {
  url: "/api/services/app/CountryInfo/GetCountryInfoList",
  method: "get",
  params: {},
};

// 获取国家地区详情
export const getCountryInfo = {
  url: "/api/services/app/CountryInfo/Get",
  method: "get",
  params: {},
};

// 创建国家地区
export const createCountryInfo = {
  url: "/api/services/app/CountryInfo/Create",
  method: "post",
  data: {},
};

// 更新国家地区
export const updateCountryInfo = {
  url: "/api/services/app/CountryInfo/Update",
  method: "put",
  data: {},
};

// 删除国家地区
export const deleteCountryInfo = {
  url: "/api/services/app/CountryInfo/Delete",
  method: "delete",
  params: {},
};

// 同步国家数据
export const syncCountries = {
  url: "/api/services/app/CountryInfo/SyncCountries",
  method: "post",
  params: {},
};

// 数据字典相关接口

// 获取数据字典分页列表
export const getDataDictionaryList = {
  url: "/api/services/app/DataDictionary/GetDataDictionaryList",
  method: "get",
  params: {},
};

// 获取数据字典详情
export const getDataDictionary = {
  url: "/api/services/app/DataDictionary/Get",
  method: "get",
  params: {},
};

// 创建数据字典
export const createDataDictionary = {
  url: "/api/services/app/DataDictionary/Create",
  method: "post",
  data: {},
};

// 更新数据字典
export const updateDataDictionary = {
  url: "/api/services/app/DataDictionary/Update",
  method: "put",
  data: {},
};

// 删除数据字典
export const deleteDataDictionary = {
  url: "/api/services/app/DataDictionary/Delete",
  method: "delete",
  params: {},
};

