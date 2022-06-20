//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Log;
using RRQMCore.XREF.Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RRQMCore.Extensions
{
    /// <summary>
    /// 辅助扩展类
    /// </summary>
    public static class RRQMCoreExtensions
    {
        #region Json转换

        /// <summary>
        /// 序列化成Json数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToJsonBytes(this object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None));
        }

        /// <summary>
        /// 转换为json字符串。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        /// <summary>
        ///  反序列化成Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this byte[] buffer, int offset, int len)
        {
            return Encoding.UTF8.GetString(buffer, offset, len).ToJsonObject<T>();
        }

        /// <summary>
        ///  反序列化成Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer).ToJsonObject<T>();
        }

        /// <summary>
        ///  反序列化成Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// 反序列化成Json数据
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToJsonObject(this string jsonString, Type type)
        {
            return JsonConvert.DeserializeObject(jsonString, type);
        }

        #endregion Json转换

        #region 字符串扩展

        /// <summary>
        /// 判断字符串compare 在 input字符串中出现的次数
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="compare">用于比较的字符串</param>
        /// <returns>字符串compare 在 input字符串中出现的次数</returns>
        public static int HitStringCount(this string input, string compare)
        {
            int index = input.IndexOf(compare);
            if (index != -1)
            {
                return 1 + HitStringCount(input.Substring(index + compare.Length), compare);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 将字符转换为对应类型。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static object ParseToType(this string value, Type destinationType)
        {
            object returnValue;
            if ((value == null) || destinationType.IsInstanceOfType(value))
            {
                return value;
            }
            string str = value;
            if ((str != null) && (str.Length == 0))
            {
                return null;
            }
            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!flag && !converter.CanConvertTo(destinationType))
            {
                throw new InvalidOperationException("无法转换成类型：" + value.ToString() + "==>" + destinationType);
            }
            try
            {
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(" 类型转换出错：" + value.ToString() + "==>" + destinationType, e);
            }
            return returnValue;
        }

        /// <summary>
        /// 只按第一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitFirst(this string str, char split)
        {
            List<string> s = new List<string>();
            int index = str.IndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }

        /// <summary>
        /// 按字符串分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string[] Split(this string str, string pattern)
        {
            return Regex.Split(str, pattern);
        }

        /// <summary>
        /// 只按最后一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitLast(this string str, char split)
        {
            List<string> s = new List<string>();
            int index = str.LastIndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }

        /// <summary>
        /// 按格式填充
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string Format(this string str, params object[] ps)
        {
            if (ps==null||ps.Length==0)
            {
                return str;
            }
            return string.Format(str, ps);
        }

        /// <summary>
        /// 转换为SHA1。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ToSha1(this string value, Encoding encoding)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            return sha1.ComputeHash(encoding.GetBytes(value));
        }

        /// <summary>
        /// 转换为UTF-8数据，效果等于<see cref="Encoding.UTF8"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToUTF8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// 将16进制的字符转换为数组。
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static byte[] ByHexStringToBytes(this string hexString, string splite = default)
        {
            if (!string.IsNullOrEmpty(splite))
            {
                hexString = hexString.Replace(splite, string.Empty);
            }

            if ((hexString.Length % 2) != 0)
            {
                hexString += " ";
            }
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        /// <summary>
        /// 将16进制的字符转换为int32。
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static int ByHexStringToInt32(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return default;
            }
            return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// 从Base64转到数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ByBase64ToBytes(this string value)
        {
            return Convert.FromBase64String(value);
        }
        #endregion

        #region 字节数组扩展
        /// <summary>
        /// 转Base64。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }


        /// <summary>
        /// 索引包含数组。
        /// <para>
        /// 例如：在{0,1,2,3,1,2,3}中搜索{1,2}，则会返回list:[2,5]，均为最后索引的位置。
        /// </para>
        /// </summary>
        /// <param name="srcByteArray"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="subByteArray"></param>
        /// <returns></returns>
        public static List<int> IndexOfInclude(this byte[] srcByteArray, int offset, int length, byte[] subByteArray)
        {
            int subByteArrayLen = subByteArray.Length;
            List<int> indexes = new List<int>();
            if (length < subByteArrayLen)
            {
                return indexes;
            }
            int hitLength = 0;
            for (int i = offset; i < length; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                }

                if (hitLength == subByteArray.Length)
                {
                    hitLength = 0;
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        /// <summary>
        /// 索引第一个包含数组的索引位置，例如：在{0,1,2,3,1,2,3}中索引{2,3}，则返回3。
        /// <para>如果目标数组为null或长度为0，则直接返回offset的值</para>
        /// </summary>
        /// <param name="srcByteArray"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="subByteArray"></param>
        /// <returns></returns>
        public static int IndexOfFirst(this byte[] srcByteArray, int offset, int length, byte[] subByteArray)
        {
            if (length < subByteArray.Length)
            {
                return -1;
            }
            if (subByteArray == null || subByteArray.Length == 0)
            {
                return offset;
            }
            int hitLength = 0;
            for (int i = offset; i < length + offset; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                }

                if (hitLength == subByteArray.Length)
                {
                    return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// 字节数组转16进制字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static string ByBytesToHexString(this byte[] buffer, int offset, int length, string splite = default)
        {
            if (string.IsNullOrEmpty(splite))
            {
                return BitConverter.ToString(buffer, offset, length).Replace("-", string.Empty);
            }
            else
            {
                return BitConverter.ToString(buffer, offset, length).Replace("-", splite);
            }
        }
        #endregion

        #region Type扩展
        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetRefOutType(this Type type)
        {
            if (type.IsByRef)
            {
                return type.GetElementType();
            }
            else
            {
                return type;
            }
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object GetDefault(this Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        /// <summary>
        /// 判断为结构体
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStruct(this Type targetType)
        {
            if (!targetType.IsPrimitive && !targetType.IsClass && !targetType.IsEnum && targetType.IsValueType)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type theType)
        {
            return (theType.IsGenericType && theType.
              GetGenericTypeDefinition().Equals
              (RRQMCoreUtility.nullableType));
        }
        #endregion

        #region 类型转换
        ///<summary> 
        ///  将字符串格式化成指定的数据类型
        ///</summary> 
        ///<param name="str"></param> 
        ///<param name="type"></param>
        /// <param name="value"></param>
        /// <param name="splits"></param> 
        ///   <returns></returns> 
        public static bool TryParseToType(string str, Type type, out object value, char[] splits = default)
        {
            if (string.IsNullOrEmpty(str))
            {
                value = default;
                return true;
            }

            if (type == null)
            {
                value = str;
                return true;
            }
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                string[] strs;
                if (splits == null)
                {
                    strs = str.Split(new char[] { ' ', ';', '-', '/' });
                }
                else
                {
                    strs = str.Split(splits);
                }

                Array array = Array.CreateInstance(elementType, strs.Length);
                for (int i = 0, c = strs.Length; i < c; ++i)
                {
                    object o;
                    if (ConvertSimpleType(strs[i], elementType, out o))
                    {
                        array.SetValue(o, i);
                    }
                    else
                    {
                        value = default;
                        return false;
                    }
                }
                value = array;
                return true;
            }
            return ConvertSimpleType(str, type, out value);
        }



        private static bool ConvertSimpleType(string value, Type destinationType, out object returnValue)
        {
            if ((value == null) || destinationType.IsInstanceOfType(value))
            {
                returnValue = value;
                return true;
            }

            if (string.IsNullOrEmpty(value))
            {
                returnValue = default;
                return true;
            }
            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!flag && !converter.CanConvertTo(destinationType))
            {
                returnValue = default;
                return false;
            }
            try
            {
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
            }
            catch
            {
                returnValue = default;
                return false;
            }
            return true;
        }
        #endregion

        #region 字典扩展

        /// <summary>
        /// 移除满足条件的项目。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="pairs"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static int Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> pairs, Func<KeyValuePair<TKey, TValue>, bool> func)
        {
            List<TKey> list = new List<TKey>();
            foreach (var item in pairs)
            {
                if (func?.Invoke(item) == true)
                {
                    list.Add(item.Key);
                }
            }

            int count = 0;
            foreach (var item in list)
            {
                if (pairs.TryRemove(item, out _))
                {
                    count++;
                }
            }
            return count;
        }

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER

        /// <summary>
        /// 尝试添加
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="tkey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryAdd<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey, TValue value)
        {
            if (dictionary.ContainsKey(tkey))
            {
                return false;
            }
            dictionary.Add(tkey, value);
            return true;
        }
#endif

        /// <summary>
        /// 尝试添加
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="tkey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void AddOrUpdate<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey, TValue value)
        {
            if (dictionary.ContainsKey(tkey))
            {
                dictionary[tkey] = value;
            }
            else
            {
                dictionary.Add(tkey, value);
            }
        }
        #endregion
    }
}