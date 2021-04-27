using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    public static class EnumHelper
    {
        static ConcurrentDictionary<Enum, string> _cache = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// 获取DescriptionAttribute
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum @enum)
        {
            var result = string.Empty;

            if (@enum == null) return result;

            if (!_cache.TryGetValue(@enum, out result))
            {
                var typeInfo = @enum.GetType();

                var enumValues = typeInfo.GetEnumValues();

                foreach (var value in enumValues)
                {
                    if (@enum.Equals(value))
                    {
                        MemberInfo memberInfo = typeInfo.GetMember(value.ToString()).First();

                        result = memberInfo.GetCustomAttribute<DescriptionAttribute>().Description;
                    }
                }

                _cache.TryAdd(@enum, result);
            }

            return result;
        }

        public static string ToNVString(this HttpStatusCode @enum)
        {
            return $"{(int)@enum}  {@enum.ToString()}";
        }


        public static byte[] ToArray(this ArraySegment<byte> source)
        {
            if (source.Array == null)
            {
                return null;
            }

            var buffer = new byte[source.Count];
            if (buffer.Length > 0)
            {
                Array.Copy(source.Array, source.Offset, buffer, 0, buffer.Length);
            }

            return buffer;
        }

        /// <summary>
        /// 根据字符串获取枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetEnum<T>(string str, out T result) where T : struct
        {
            return Enum.TryParse<T>(str, out result);
        }
    }
}
