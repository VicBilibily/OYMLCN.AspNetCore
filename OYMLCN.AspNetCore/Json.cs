#if NETCOREAPP3_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace OYMLCN.Extensions
{
    /// <summary>
    /// JsonExtension
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// 默认序列化设置
        /// </summary>
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions()
        {
            //WriteIndented = true, // 是否格式化
            IgnoreNullValues = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // 解决中文格式化的问题
        };

        /// <summary>
        /// 将对象转为JSON字符串
        /// </summary>
        /// <param name="data">任意对象</param>
        /// <param name="options">序列化配置</param>
        /// <returns>JSON字符串</returns>
        public static string ToTextJsonString<T>(this T data, JsonSerializerOptions options = null) where T : class
            => JsonSerializer.Serialize(data, options ?? DefaultOptions);

        /// <summary>
        /// JSON字符串转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeserializeTextJsonToObject<T>(this string str)
            => JsonSerializer.Deserialize<T>(str);
        /// <summary>
        /// 转换JSON字符串为可供查询的JsonDocument
        /// </summary>
        /// <returns></returns>
        public static JsonDocument ParseToJsonDocument(this string str, JsonDocumentOptions options = default)
            => JsonDocument.Parse(str.IsNullOrWhiteSpace() ? "{}" : str, options);

    }
}
#endif