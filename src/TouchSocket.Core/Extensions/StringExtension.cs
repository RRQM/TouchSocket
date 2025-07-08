//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// StringExtension
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// 默认的空格字符串。
    /// </summary>
    public const string DefaultSpaceString = " ";

    /// <summary>
    /// 默认的rn字符串的UTF-8表示。
    /// </summary>
    public static ReadOnlySpan<byte> Default_RN_Utf8Span => "\r\n"u8;

    /// <summary>
    /// 默认的rnrn字符串的UTF-8表示。
    /// </summary>
    public static ReadOnlySpan<byte> Default_RNRN_Utf8Span => "\r\n\r\n"u8;

    /// <summary>
    /// 默认的空格字符串的UTF-8表示。
    /// </summary>
    public static ReadOnlySpan<byte> DefaultSpaceUtf8Span => " "u8;

    /// <summary>
    /// 从Base64转到数组。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ByBase64ToBytes(this string value)
    {
        return Convert.FromBase64String(value);
    }

    /// <summary>
    /// 将16进制的字符转换为数组。
    /// </summary>
    /// <param name="hexString"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static byte[] ByHexStringToBytes(this string hexString, string split = default)
    {
        if (!string.IsNullOrEmpty(split))
        {
            hexString = hexString.Replace(split, string.Empty);
        }

        if ((hexString.Length % 2) != 0)
        {
            hexString += StringExtension.DefaultSpaceString;
        }
        var returnBytes = new byte[hexString.Length / 2];
        for (var i = 0; i < returnBytes.Length; i++)
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
        return string.IsNullOrEmpty(hexString) ? default : int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
    }

    /// <summary>
    /// 格式化字符串。
    /// </summary>
    /// <param name="str">要格式化的字符串。</param>
    /// <param name="ps">格式化参数。</param>
    /// <returns>格式化后的字符串。</returns>
    public static string Format(this string str, params object[] ps)
    {
        return string.Format(str, ps);
    }

    /// <summary>
    /// 检查字符串是否具有有效值。
    /// </summary>
    /// <param name="str">要检查的字符串。</param>
    /// <returns>如果字符串不是null且非空格或制表符等，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool HasValue([NotNullWhen(true)] this string str)
    {
        // 使用string.IsNullOrWhiteSpace方法检查字符串是否为<see langword="null"/>或包含仅空格或制表符等
        // 返回相反的结果以确定字符串是否具有有效值
        return !string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// 判断字符串compare 在 input字符串中出现的次数
    /// </summary>
    /// <param name="input">源字符串</param>
    /// <param name="compare">用于比较的字符串</param>
    /// <returns>字符串compare 在 input字符串中出现的次数</returns>
    public static int HitStringCount(this string input, string compare)
    {
        var index = input.IndexOf(compare);
        return index != -1 ? 1 + HitStringCount(input.Substring(index + compare.Length), compare) : 0;
    }

    /// <summary>
    /// 检查字符串是否为空或只包含空格。
    /// </summary>
    /// <param name="str">要检查的字符串。</param>
    /// <returns>如果字符串为空或只包含空格，则返回 true；否则返回 false。</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// 检查字符串是否为 null 或仅包含空白字符。
    /// </summary>
    /// <param name="str">要检查的字符串。</param>
    /// <returns>如果字符串为 null 或仅包含空白字符，则返回 true；否则返回 false。</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// 将输入字符串转换为有效的标识符。
    /// </summary>
    /// <param name="input">输入字符串。</param>
    /// <returns>转换后的标识符字符串。</returns>
    public static string MakeIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // 替换非法字符，允许 Unicode 字母、数字和下划线
        var result = Regex.Replace(input, @"[^\p{L}\p{N}_]", "_");

        // 如果结果以数字开头，则添加前缀 _
        if (char.IsDigit(result.First()))
        {
            result = "_" + result;
        }

        return result;
    }

    /// <summary>
    /// 将字符转换为对应的基础类型类型。
    /// </summary>
    /// <param name="value"></param>
    /// <param name="destinationType">目标类型必须为基础类型</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">类型转换失败</exception>
    public static object ParseToType(this string value, Type destinationType)
    {
        if (TryParseToType(value, destinationType, out var returnValue))
        {
            return returnValue;
        }
        ThrowHelper.ThrowNotSupportedException(TouchSocketCoreResource.StringParseToTypeFail.Format(value, destinationType));
        return default;
    }

    /// <summary>
    /// 将字符串解析为指定的类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="value">要解析的字符串。</param>
    /// <returns>解析后的目标类型对象。</returns>
    /// <exception cref="NotSupportedException">类型转换失败</exception>
    public static T ParseToType<T>(this string value)
    {
        if (TryParseToType<T>(value, out var returnValue))
        {
            return returnValue;
        }
        ThrowHelper.ThrowNotSupportedException(TouchSocketCoreResource.StringParseToTypeFail.Format(value, typeof(T)));
        return default;
    }

    /// <summary>
    /// 移除字符串末尾指定数量的字符。
    /// </summary>
    /// <param name="str">要处理的字符串。</param>
    /// <param name="count">要移除的字符数量。</param>
    /// <returns>移除末尾指定数量字符后的字符串。</returns>
    public static string RemoveLastChars(this string str, int count)
    {
        if (string.IsNullOrEmpty(str) || count <= 0)
        {
            return str;
        }

        // 计算需要保留的字符长度

        var lengthToKeep = str.Length - count;

        // 如果需要保留的长度小于0，则返回空字符串

        if (lengthToKeep < 0)
        {
            return string.Empty;
        }
        // 返回去除末尾指定数量字符后的子串
        return str.Substring(0, lengthToKeep);
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
    /// 只按第一个匹配项分割
    /// </summary>
    /// <param name="str"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string[] SplitFirst(this string str, char split)
    {
        var s = new List<string>();
        var index = str.IndexOf(split);
        if (index > 0)
        {
            s.Add(str.Substring(0, index).Trim());
            s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
        }

        return s.ToArray();
    }

    /// <summary>
    /// 只按最后一个匹配项分割
    /// </summary>
    /// <param name="str"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string[] SplitLast(this string str, char split)
    {
        var s = new List<string>();
        var index = str.LastIndexOf(split);
        if (index > 0)
        {
            s.Add(str.Substring(0, index).Trim());
            s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
        }

        return s.ToArray();
    }

    /// <summary>
    /// 转换为SHA1。
    /// </summary>
    /// <param name="value"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static byte[] ToSha1(this string value, Encoding encoding)
    {
        using (var sha1 = SHA1.Create())
        {
            return sha1.ComputeHash(encoding.GetBytes(value));
        }
    }

    /// <summary>
    /// 转换为UTF-8数据，效果等于<see cref="Encoding.UTF8"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ToUtf8Bytes(this string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    ///<summary>
    /// 将字符串格式化成指定的基本数据类型
    ///</summary>
    ///<param name="value">待解析的字符串</param>
    ///<param name="destinationType">目标数据类型</param>
    /// <param name="returnValue">解析后的值，输出参数</param>
    ///   <returns>如果解析成功返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public static bool TryParseToType(string value, Type destinationType, out object returnValue)
    {
        // 如果字符串为空或只含空格，将returnValue设为默认值并返回<see langword="true"/>
        if (string.IsNullOrEmpty(value))
        {
            returnValue = default;
            return true;
        }

        // 如果目标类型为枚举类型，使用Enum.Parse方法进行解析
        if (destinationType.IsEnum)
        {
            returnValue = Enum.Parse(destinationType, value);
            return true;
        }

        // 根据目标类型的TypeCode进行switch-case解析
        switch (Type.GetTypeCode(destinationType))
        {
            // 解析为布尔型
            case TypeCode.Boolean:
                {
                    returnValue = bool.Parse(value);
                    return true;
                }
            // 解析为字符型
            case TypeCode.Char:
                {
                    returnValue = char.Parse(value);
                    return true;
                }
            // 解析为带符号的字节型
            case TypeCode.SByte:
                {
                    returnValue = sbyte.Parse(value);
                    return true;
                }
            // 解析为无符号的字节型
            case TypeCode.Byte:
                {
                    returnValue = byte.Parse(value); ;
                    return true;
                }
            // 解析为短整型
            case TypeCode.Int16:
                {
                    returnValue = short.Parse(value);
                    return true;
                }
            // 解析为无符号短整型
            case TypeCode.UInt16:
                {
                    returnValue = ushort.Parse(value);
                    return true;
                }
            // 解析为整型
            case TypeCode.Int32:
                {
                    returnValue = int.Parse(value); ;
                    return true;
                }
            // 解析为无符号整型
            case TypeCode.UInt32:
                {
                    returnValue = uint.Parse(value);
                    return true;
                }
            // 解析为长整型
            case TypeCode.Int64:
                {
                    returnValue = long.Parse(value);
                    return true;
                }
            // 解析为无符号长整型
            case TypeCode.UInt64:
                {
                    returnValue = ulong.Parse(value); ;
                    return true;
                }
            // 解析为单精度浮点型
            case TypeCode.Single:
                {
                    returnValue = float.Parse(value);
                    return true;
                }
            // 解析为双精度浮点型
            case TypeCode.Double:
                {
                    returnValue = double.Parse(value);
                    return true;
                }
            // 解析为十进制小数类型
            case TypeCode.Decimal:
                {
                    returnValue = decimal.Parse(value);
                    return true;
                }
            // 解析为日期时间类型
            case TypeCode.DateTime:
                {
                    returnValue = DateTime.Parse(value);
                    return true;
                }
            // 解析为字符串类型
            case TypeCode.String:
                {
                    returnValue = value;
                    return true;
                }
            // 对空类型、对象类型或数据库空值类型，将returnValue设为默认值并返回<see langword="false"/>
            case TypeCode.Empty:
            case TypeCode.Object:
            case TypeCode.DBNull:
            default:
                returnValue = default;
                return false;
        }
    }


    ///<summary>
    /// 尝试将字符串解析为指定的类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="value">要解析的字符串。</param>
    /// <param name="returnValue">解析后的值，输出参数。</param>
    /// <returns>如果解析成功返回 true，否则返回 false。</returns>
    public static bool TryParseToType<T>(string value, out T returnValue)
    {
        // 处理空或全空格字符串
        if (string.IsNullOrWhiteSpace(value))
        {
            returnValue = default;
            return true;
        }

        var type = typeof(T);

        // 处理枚举类型
        if (type.IsEnum)
        {
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            var success = Enum.TryParse(type, value, out var enumResult);
            if (success)
            {
                returnValue = (T)enumResult;
                return true;
            }
#else
            try
            {
                var enumResult = Enum.Parse(type, value);
                returnValue = (T)enumResult;
                return true;
            }
            catch
            {
            }
#endif
            returnValue = default;
            return false;
        }

        // 根据 TypeCode 处理基础类型
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                if (bool.TryParse(value, out var boolResult))
                {
                    returnValue = Unsafe.As<bool, T>(ref boolResult);
                    return true;
                }
                break;

            case TypeCode.Char:
                if (char.TryParse(value, out var charResult))
                {
                    returnValue = Unsafe.As<char, T>(ref charResult);
                    return true;
                }
                break;

            case TypeCode.SByte:
                if (sbyte.TryParse(value, out var sbyteResult))
                {
                    returnValue = Unsafe.As<sbyte, T>(ref sbyteResult);
                    return true;
                }
                break;

            case TypeCode.Byte:
                if (byte.TryParse(value, out var byteResult))
                {
                    returnValue = Unsafe.As<byte, T>(ref byteResult);
                    return true;
                }
                break;

            case TypeCode.Int16:
                if (short.TryParse(value, out var shortResult))
                {
                    returnValue = Unsafe.As<short, T>(ref shortResult);
                    return true;
                }
                break;

            case TypeCode.UInt16:
                if (ushort.TryParse(value, out var ushortResult))
                {
                    returnValue = Unsafe.As<ushort, T>(ref ushortResult);
                    return true;
                }
                break;

            case TypeCode.Int32:
                if (int.TryParse(value, out var intResult))
                {
                    returnValue = Unsafe.As<int, T>(ref intResult);
                    return true;
                }
                break;

            case TypeCode.UInt32:
                if (uint.TryParse(value, out var uintResult))
                {
                    returnValue = Unsafe.As<uint, T>(ref uintResult);
                    return true;
                }
                break;

            case TypeCode.Int64:
                if (long.TryParse(value, out var longResult))
                {
                    returnValue = Unsafe.As<long, T>(ref longResult);
                    return true;
                }
                break;

            case TypeCode.UInt64:
                if (ulong.TryParse(value, out var ulongResult))
                {
                    returnValue = Unsafe.As<ulong, T>(ref ulongResult);
                    return true;
                }
                break;

            case TypeCode.Single:
                if (float.TryParse(value, out var floatResult))
                {
                    returnValue = Unsafe.As<float, T>(ref floatResult);
                    return true;
                }
                break;

            case TypeCode.Double:
                if (double.TryParse(value, out var doubleResult))
                {
                    returnValue = Unsafe.As<double, T>(ref doubleResult);
                    return true;
                }
                break;

            case TypeCode.Decimal:
                if (decimal.TryParse(value, out var decimalResult))
                {
                    returnValue = Unsafe.As<decimal, T>(ref decimalResult);
                    return true;
                }
                break;

            case TypeCode.DateTime:
                if (DateTime.TryParse(value, out var dateResult))
                {
                    returnValue = Unsafe.As<DateTime, T>(ref dateResult);
                    return true;
                }
                break;

            case TypeCode.String:
                returnValue = Unsafe.As<string, T>(ref value);
                return true;
        }

        returnValue = default;
        return false;
    }
}