/**
 * 时区处理工具类
 */

// 需要处理的时间字段关键字
const TIME_FIELD_KEYWORDS = [
  "Time",
  "DateStart",
  "DateEnd",
  "TimeStart",
  "TimeEnd",
  "startDate",
  "endDate",
];

/**
 * 检查字段名是否包含时间关键字
 * @param {string} fieldName - 字段名
 * @returns {boolean}
 */
export const isTimeField = (fieldName) => {
  return TIME_FIELD_KEYWORDS.some((keyword) => fieldName.includes(keyword));
};

/**
 * 格式化日期时间字符串
 * @param {Date} date - 日期对象
 * @param {boolean} dateOnly - 是否只返回日期部分
 * @returns {string}
 */
export const formatDateTime = (date, dateOnly = false) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  if (dateOnly) {
    return `${year}-${month}-${day}`;
  }

  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

/**
 * 检查时间值是否只包含日期
 * @param {string|number} timeValue - 时间值
 * @returns {boolean}
 */
const isDateOnly = (timeValue) => {
  if (typeof timeValue === "number") {
    // 如果是时间戳，转换为日期字符串
    const date = new Date(timeValue);
    return (
      date.getHours() === 0 &&
      date.getMinutes() === 0 &&
      date.getSeconds() === 0
    );
  }
  // 如果是字符串，检查是否包含时间部分
  return typeof timeValue === "string" && !timeValue.includes(":");
};

/**
 * 将日期字符串转换为指定时区的日期对象
 * @param {string} dateStr - 日期字符串 (YYYY-MM-DD)
 * @param {number} timezoneOffset - 时区偏移量（小时）
 * @returns {Date}
 */
const createDateWithTimezone = (dateStr, timezoneOffset) => {
  const [year, month, day] = dateStr.split("-").map(Number);
  // 创建本地日期对象
  const date = new Date(year, month - 1, day);
  // 获取本地时区偏移（分钟）
  const localOffset = date.getTimezoneOffset();
  // 计算目标时区偏移（分钟）
  const targetOffset = timezoneOffset * 60;
  // 调整时间
  const utcTime = date.getTime() + (localOffset + targetOffset) * 60 * 1000;
  return new Date(utcTime);
};

/**
 * 将UTC时间转换为本地时间
 * @param {string|number} utcTime - UTC时间
 * @param {number} timezoneOffset - 时区偏移量（小时）
 * @returns {string|number}
 */
const convertUTCToLocal = (utcTime, timezoneOffset) => {
  if (!utcTime) return utcTime;

  // 如果是纯日期格式 (YYYY-MM-DD)
  if (typeof utcTime === "string" && /^\d{4}-\d{2}-\d{2}$/.test(utcTime)) {
    const date = createDateWithTimezone(utcTime, timezoneOffset);
    return formatDateTime(date, true);
  }

  const date = new Date(utcTime);
  if (isNaN(date.getTime())) return utcTime;

  // 计算时区偏移的毫秒数
  const offsetMs = timezoneOffset * 60 * 60 * 1000;
  const localDate = new Date(date.getTime() + offsetMs);

  return formatDateTime(localDate, isDateOnly(utcTime));
};

/**
 * 将本地时间转换为UTC时间
 * @param {string|number} localTime - 本地时间
 * @param {number} timezoneOffset - 时区偏移量（小时）
 * @returns {string|number}
 */
const convertLocalToUTC = (localTime, timezoneOffset) => {
  if (!localTime) return localTime;

  // 如果是纯日期格式 (YYYY-MM-DD)
  if (typeof localTime === "string" && /^\d{4}-\d{2}-\d{2}$/.test(localTime)) {
    // 创建日期对象并设置为当天的开始时间
    const [year, month, day] = localTime.split("-").map(Number);
    const date = new Date(year, month - 1, day);

    // 计算UTC时间 - 注意：timezoneOffset是负数表示东区，需要加上这个偏移
    const utcDate = new Date(date.getTime() + timezoneOffset * 60 * 60 * 1000);

    // 格式化为 YYYY-MM-DD HH:mm:ss
    const utcYear = utcDate.getUTCFullYear();
    const utcMonth = String(utcDate.getUTCMonth() + 1).padStart(2, "0");
    const utcDay = String(utcDate.getUTCDate()).padStart(2, "0");
    const utcHours = String(utcDate.getUTCHours()).padStart(2, "0");
    const utcMinutes = String(utcDate.getUTCMinutes()).padStart(2, "0");
    const utcSeconds = String(utcDate.getUTCSeconds()).padStart(2, "0");

    const result = `${utcYear}-${utcMonth}-${utcDay} ${utcHours}:${utcMinutes}:${utcSeconds}`;
    return result;
  }

  const date = new Date(localTime);
  if (isNaN(date.getTime())) return localTime;

  // 计算时区偏移的毫秒数 - 注意：timezoneOffset是负数表示东区，需要加上这个偏移
  const offsetMs = timezoneOffset * 60 * 60 * 1000;
  const utcDate = new Date(date.getTime() + offsetMs);

  return formatDateTime(utcDate, isDateOnly(localTime));
};

/**
 * 递归处理对象中的时间字段
 * @param {Object} obj - 要处理的对象
 * @param {number} timezoneOffset - 时区偏移量（小时）
 * @param {boolean} toUTC - true: 转换为UTC时间, false: 转换为本地时间
 * @returns {Object}
 */
const processTimeFields = (obj, timezoneOffset, toUTC = false) => {
  if (!obj || typeof obj !== "object") return obj;

  if (Array.isArray(obj)) {
    return obj.map((item) => processTimeFields(item, timezoneOffset, toUTC));
  }

  const result = { ...obj };
  for (const key in result) {
    if (Object.prototype.hasOwnProperty.call(result, key)) {
      if (isTimeField(key)) {
        result[key] = toUTC
          ? convertLocalToUTC(result[key], timezoneOffset)
          : convertUTCToLocal(result[key], timezoneOffset);
      } else if (typeof result[key] === "object" && result[key] !== null) {
        result[key] = processTimeFields(result[key], timezoneOffset, toUTC);
      }
    }
  }
  return result;
};

/**
 * 处理请求数据，将本地时间转换为UTC时间
 * @param {Object} data - 请求数据
 * @param {number} timezoneOffset - 时区偏移量（小时）
 * @returns {Object}
 */
export const processRequestData = (data, timezoneOffset) => {
  return processTimeFields(data, timezoneOffset, true);
};

/**
 * 处理响应数据，将UTC时间转换为本地时间
 * @param {Object} data - 响应数据
 * @param {number} timezoneOffset - 时区偏移量（小时）
 * @returns {Object}
 */
export const processResponseData = (data, timezoneOffset) => {
  return processTimeFields(data, timezoneOffset, false);
};
