using Abp.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumLocalizationName(this Enum source)
        {
            string name = source.ToString();
            var type = source.GetType();
            var field = type.GetField(name);
            var nameKey = $"{type.Name}:{name}";
            var value2 = NullLocalizationManager.Instance.GetSource(ClientPlatformConsts.LocalizationSourceName).GetString(nameKey);
            var localiSource = Abp.Localization.LocalizationHelper.GetSource(ClientPlatformConsts.LocalizationSourceName);
            var value = localiSource.GetStringOrNull(nameKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = DisplayName(source);
            }
            return value;
        }

        public static string DisplayName(this Enum source)
        {
            string name = source.ToString();
            var field = source.GetType().GetField(name);
            if (field == null) return null;
            object[] objs = field.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (objs == null || objs.Length == 0) { return name; }
            var attr = (DisplayAttribute)objs[0];
            return attr.Name;
        }

        /// <summary>
        /// 获取枚举上的 AmbientValue 值
        /// </summary>
        public static string GetAmbientValue(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attribute = field.GetCustomAttribute<AmbientValueAttribute>();
            return attribute != null ? attribute.Value?.ToString() : value.ToString();
        }
    }
}
