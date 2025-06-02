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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TouchSocket.Core;

/// <summary>
/// 常量
/// </summary>
public class TouchSocketCoreUtility
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public static readonly Type StringType = typeof(string);
    public static readonly Type ByteType = typeof(byte);
    public static readonly Type SbyteType = typeof(sbyte);
    public static readonly Type ShortType = typeof(short);
    public static readonly Type ObjType = typeof(object);
    public static readonly Type UshortType = typeof(ushort);
    public static readonly Type IntType = typeof(int);
    public static readonly Type UintType = typeof(uint);
    public static readonly Type BoolType = typeof(bool);
    public static readonly Type CharType = typeof(char);
    public static readonly Type LongType = typeof(long);
    public static readonly Type UlongType = typeof(ulong);
    public static readonly Type FloatType = typeof(float);
    public static readonly Type DoubleType = typeof(double);
    public static readonly Type DecimalType = typeof(decimal);
    public static readonly Type DateTimeType = typeof(DateTime);
    public static readonly Type BytesType = typeof(byte[]);
    public static readonly Type DicType = typeof(IDictionary);
    public static readonly Type ArrayType = typeof(Array);
    public static readonly Type NullableType = typeof(Nullable<>);
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

    /// <summary>
    /// 空字符串常亮
    /// </summary>
    public const string Empty = "";

    /// <summary>
    /// 0长度字节数组
    /// </summary>
#if NET45
    public static readonly byte[] ZeroBytes = new byte[0];
#else
    public static readonly byte[] ZeroBytes = Array.Empty<byte>();
#endif
    private static int s_seed;

    /// <summary>
    /// 生成指定范围内的随机Int64整数
    /// </summary>
    /// <returns>返回一个在0到10000000之间的随机Int64整数</returns>
    public static long GenerateRandomInt64()
    {
        // 使用种子生成器创建一个新的随机数生成器实例
        // 种子递增以保持随机性
        var random = new Random(s_seed++);

        // 生成一个0到1之间的随机双精度浮点数
        var randomDouble = random.NextDouble();

        // 将随机双精度浮点数乘以10000000，然后取其整数部分
        // 以获得一个在0到10000000之间的随机Int64整数
        return (long)Math.Floor(randomDouble * 10000000D);
    }
    /// <summary>
    /// 判断输入的字符串是否是一个超链接
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsUrl(string input)
    {
        var pattern = @"^[a-zA-Z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$?";
        var regex = new Regex(pattern);
        return regex.IsMatch(input);
    }

    /// <summary>
    /// 判断输入的字符串是否是表示一个IP地址
    /// </summary>
    /// <param name="input">被比较的字符串</param>
    /// <returns>是IP地址则为<see langword="true"/></returns>
    public static bool IsIPv4(string input)
    {
        try
        {
            var iPs = input.Split('.');
            var regex = new Regex(@"^\d+$");
            for (var i = 0; i < iPs.Length; i++)
            {
                if (!regex.IsMatch(iPs[i]))
                {
                    return false;
                }
                if (Convert.ToUInt16(iPs[i]) > 255)
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 判断输入的字符串是否是合法的IPV6 地址
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsIpv6(string input)
    {
        var temp = input;
        var strs = temp.Split(':');
        if (strs.Length > 8)
        {
            return false;
        }
        var count = input.HitStringCount("::");
        string pattern;
        if (count > 1)
        {
            return false;
        }
        else if (count == 0)
        {
            pattern = @"^([\da-f]{1,4}:){7}[\da-f]{1,4}$";

            var regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
        else
        {
            pattern = @"^([\da-f]{1,4}:){0,5}::([\da-f]{1,4}:){0,5}[\da-f]{1,4}$";
            var regex1 = new Regex(pattern);
            return regex1.IsMatch(input);
        }
    }

    /// <summary>
    /// 命中BufferLength
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HitBufferLength(long value)
    {
        if (value < 1024 * 100)
        {
            return 1024;
        }
        else if (value < 1024 * 512)
        {
            return 1024 * 10;
        }
        else if (value < 1024 * 1024)
        {
            return 1024 * 64;
        }
        else
        {
            return value < 1024 * 1024 * 50
                ? 1024 * 512
                : value < 1024 * 1024 * 100
                                ? 1024 * 1024
                                : value < 1024 * 1024 * 1024 ? 1024 * 1024 * 2 : value < 1024 * 1024 * 1024 * 10L ? 1024 * 1024 * 5 : 1024 * 1024 * 10;
        }
    }
}