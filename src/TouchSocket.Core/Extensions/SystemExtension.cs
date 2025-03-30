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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 为System提供扩展。
/// </summary>
public static class SystemExtension
{
    #region IDisposable

    /// <summary>
    /// 安全性释放（不用判断对象是否为空）。不会抛出任何异常。
    /// </summary>
    /// <param name="dis"></param>
    /// <returns></returns>
    public static void SafeDispose(this IDisposable dis)
    {
        if (dis == default)
        {
            return;
        }
        try
        {
            dis.Dispose();
        }
        catch
        {
        }
    }

    /// <summary>
    /// 安全性释放（不用判断对象是否为空）。不会抛出任何异常。
    /// <para>
    /// 内部会判断<see cref="IDisposableObject.DisposedValue"/>的值，如果为<see langword="true"/>，则不会再执行<see cref="IDisposable.Dispose"/>。
    /// </para>
    /// </summary>
    /// <param name="disposableObject"></param>
    /// <returns></returns>
    public static void SafeDispose(this IDisposableObject disposableObject)
    {
        if (disposableObject == default)
        {
            return;
        }
        if (disposableObject.DisposedValue)
        {
            return;
        }
        try
        {
            disposableObject.Dispose();
        }
        catch
        {
        }
    }

    #endregion IDisposable

    #region Enum


    /// <summary>
    /// 获取枚举成员上绑定的指定类型的自定义属性
    /// </summary>
    /// <param name="enumObj">枚举对象</param>
    /// <typeparam name="T">要获取的属性类型</typeparam>
    /// <returns>指定类型的自定义属性</returns>
    public static T GetAttribute<T>(this Enum enumObj) where T : Attribute
    {
        // 获取枚举对象的类型
        var type = enumObj.GetType();
        // 获取对应的枚举名
        var enumName = Enum.GetName(type, enumObj);
        // 获取枚举名对应的字段信息
        var field = type.GetField(enumName);
        // 获取字段上绑定的指定类型的自定义属性
        var attr = field.GetCustomAttribute(typeof(T), false);

        // 返回获取到的自定义属性
        return (T)attr;
    }

    #endregion Enum

    #region SetBit

    /// <summary>
    /// 对于给定的无符号长整型数值，设置指定索引位置的位值为指定的布尔值。
    /// </summary>
    /// <param name="b">原始数值。</param>
    /// <param name="index">位索引，范围为0到63。</param>
    /// <param name="bitvalue">要设置的位值（true为1，false为0）。</param>
    /// <returns>修改后的数值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在有效范围内时抛出异常。</exception>
    public static ulong SetBit(this ulong b, int index, bool bitvalue)
    {
        // 检查索引范围是否有效
        if (index < 0 || index > 63)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 63);
        }

        // 构造一个基数，用于在指定索引位置设置位值
        ulong baseNumber = 1;
        // 根据bitvalue的值，设置位
        return bitvalue ? b | baseNumber << index : b & ~(baseNumber << index);
    }

    /// <summary>
    /// 对于给定的无符号整型数值，设置指定索引位置的位值为指定的布尔值。
    /// </summary>
    /// <param name="b">原始数值。</param>
    /// <param name="index">位索引，范围为0到31。</param>
    /// <param name="bitvalue">要设置的位值（true为1，false为0）。</param>
    /// <returns>修改后的数值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在有效范围内时抛出异常。</exception>
    public static uint SetBit(this uint b, int index, bool bitvalue)
    {
        // 检查索引范围是否有效
        if (index < 0 || index > 31)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 31);
        }

        // 构造一个基数，用于在指定索引位置设置位值
        uint baseNumber = 1;
        // 根据bitvalue的值，设置位
        return bitvalue ? b | baseNumber << index : b & ~(baseNumber << index);
    }

    /// <summary>
    /// 对于给定的无符号短整型数值，设置指定索引位置的位值为指定的布尔值。
    /// </summary>
    /// <param name="b">原始数值。</param>
    /// <param name="index">位索引，范围为0到15。</param>
    /// <param name="bitvalue">要设置的位值（true为1，false为0）。</param>
    /// <returns>修改后的数值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在有效范围内时抛出异常。</exception>
    public static ushort SetBit(this ushort b, int index, bool bitvalue)
    {
        // 检查索引范围是否有效
        if (index < 0 || index > 15)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 15);
        }

        // 构造一个基数，用于在指定索引位置设置位值
        ushort baseNumber = 1;
        // 根据bitvalue的值，设置位
        return bitvalue ? (ushort)(b | baseNumber << index) : (ushort)(b & ~(baseNumber << index));
    }

    /// <summary>
    /// 对于给定的无符号字节型数值，设置指定索引位置的位值为指定的布尔值。
    /// </summary>
    /// <param name="b">原始数值。</param>
    /// <param name="index">位索引，范围为0到7。</param>
    /// <param name="bitvalue">要设置的位值（true为1，false为0）。</param>
    /// <returns>修改后的数值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在有效范围内时抛出异常。</exception>
    public static byte SetBit(this byte b, int index, bool bitvalue)
    {
        // 检查索引范围是否有效
        if (index < 0 || index > 7)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
        }

        // 构造一个基数，用于在指定索引位置设置位值
        byte baseNumber = 1;
        // 根据bitvalue的值，设置位
        return bitvalue ? (byte)(b | baseNumber << index) : (byte)(b & ~(baseNumber << index));
    }

    #endregion

    #region GetBit
    /// <summary>
    /// 获取无符号长整型数值中的指定位置的位是否为1。
    /// </summary>
    /// <param name="b">要检查的无符号长整型数值。</param>
    /// <param name="index">要检查的位的位置，从0到63。</param>
    /// <returns>如果指定位置的位为1，则返回true；否则返回false。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在0到63之间时，抛出此异常。</exception>
    public static bool GetBit(this ulong b, int index)
    {
        if (index > 63 || index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 63);
        }
        return (b & (ulong)1 << index) != 0;
    }

    /// <summary>
    /// 获取无符号整型数值中的指定位置的位是否为1。
    /// </summary>
    /// <param name="b">要检查的无符号整型数值。</param>
    /// <param name="index">要检查的位的位置，从0到31。</param>
    /// <returns>如果指定位置的位为1，则返回true；否则返回false。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在0到31之间时，抛出此异常。</exception>
    public static bool GetBit(this uint b, int index)
    {
        if (index > 31 || index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 31);
        }
        return (b & (uint)1 << index) != 0;
    }

    /// <summary>
    /// 获取无符号短整型数值中的指定位置的位是否为1。
    /// </summary>
    /// <param name="b">要检查的无符号短整型数值。</param>
    /// <param name="index">要检查的位的位置，从0到15。</param>
    /// <returns>如果指定位置的位为1，则返回true；否则返回false。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在0到15之间时，抛出此异常。</exception>
    public static bool GetBit(this ushort b, int index)
    {
        if (index > 15 || index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 15);
        }
        return (b & 1 << index) != 0;
    }

    /// <summary>
    /// 获取字节型数值中的指定位置的位是否为1。
    /// </summary>
    /// <param name="b">要检查的字节型数值。</param>
    /// <param name="index">要检查的位的位置，从0到7。</param>
    /// <returns>如果指定位置的位为1，则返回true；否则返回false。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当索引值不在0到7之间时，抛出此异常。</exception>
    public static bool GetBit(this byte b, int index)
    {
        if (index > 7 || index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
        }
        return (b & 1 << index) != 0;
    }
    #endregion

    #region Byte[]

    /// <summary>
    /// 字节数组转16进制字符
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string ByBytesToHexString(this byte[] buffer, int offset, int length, string split = default)
    {
        return string.IsNullOrEmpty(split)
            ? BitConverter.ToString(buffer, offset, length).Replace("-", string.Empty)
            : BitConverter.ToString(buffer, offset, length).Replace("-", split);
    }


    /// <summary>
    /// 将字节缓冲区转换为十六进制字符串。
    /// </summary>
    /// <param name="buffer">要转换的字节缓冲区。</param>
    /// <param name="split">可选参数，用于指定分隔符，默认为空。</param>
    /// <returns>转换后的十六进制字符串。</returns>
    public static string ByBytesToHexString(this byte[] buffer, string split = default)
    {
        return ByBytesToHexString(buffer, 0, buffer.Length, split);
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
        return IndexOfFirst(new ReadOnlySpan<byte>(subByteArray), offset, length, subByteArray);
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
    public static int IndexOfFirst(this ReadOnlySpan<byte> srcByteArray, int offset, int length, byte[] subByteArray)
    {
        if (length < subByteArray.Length)
        {
            return -1;
        }
        if (subByteArray == null || subByteArray.Length == 0)
        {
            return offset;
        }
        var hitLength = 0;
        for (var i = offset; i < length + offset; i++)
        {
            if (srcByteArray[i] == subByteArray[hitLength])
            {
                hitLength++;
            }
            else
            {
                hitLength = 0;
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
            }

            if (hitLength == subByteArray.Length)
            {
                return i;
            }
        }

        return -1;
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
        return IndexOfInclude(new ReadOnlySpan<byte>(srcByteArray), offset, length, subByteArray);
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
    public static List<int> IndexOfInclude(this ReadOnlySpan<byte> srcByteArray, int offset, int length, Span<byte> subByteArray)
    {
        var subByteArrayLen = subByteArray.Length;
        var indexes = new List<int>();
        if (length < subByteArrayLen)
        {
            return indexes;
        }
        var hitLength = 0;
        for (var i = offset; i < length; i++)
        {
            if (srcByteArray[i] == subByteArray[hitLength])
            {
                hitLength++;
            }
            else
            {
                hitLength = 0;
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
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
    /// 转Base64。
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToBase64(this byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    /// <summary>
    /// 将字节数组转换为UTF-8编码的字符串。
    /// </summary>
    /// <param name="data">要转换的字节数组。</param>
    /// <returns>转换后的UTF-8编码字符串。</returns>
    public static string ToUtf8String(this byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    /// <summary>
    /// 将字节数组的一部分转换为UTF-8编码的字符串。
    /// </summary>
    /// <param name="data">要转换的字节数组。</param>
    /// <param name="offset">数组中开始转换的索引位置。</param>
    /// <param name="length">要转换的字节数。</param>
    /// <returns>转换后的UTF-8编码字符串。</returns>
    public static string ToUtf8String(this byte[] data, int offset, int length)
    {
        return Encoding.UTF8.GetString(data, offset, length);
    }
    #endregion Byte[]

    #region Type

    /// <summary>
    /// 获取默认值
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetDefault(this Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Type GetRefOutType(this Type type)
    {
        return type.IsByRef ? type.GetElementType() : type;
    }

    /// <summary>
    /// 检查类型是否是匿名类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsAnonymous(this Type type)
    {
        // 检查是否贴有 [CompilerGenerated] 特性
        if (!type.IsDefined(typeof(CompilerGeneratedAttribute), false))
        {
            return false;
        }

        // 类型限定名是否以 <> 开头且以 AnonymousType 结尾
        return type.FullName.HasValue()
            && type.FullName.StartsWith("<>")
            && type.FullName.Contains("AnonymousType");
    }

    /// <summary>
    /// 检查类型是否是小数类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsDecimal(this Type type)
    {
        // 如果是浮点类型则直接返回
        return type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
|| Type.GetTypeCode(type) switch
{
    TypeCode.Double or TypeCode.Decimal => true,
    _ => false,
};
    }

    /// <summary>
    /// 检查类型是否是字典类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsDictionary(this Type type)
    {
        return TouchSocketCoreUtility.DicType.IsAssignableFrom(type);
    }

    /// <summary>
    /// 检查类型是否可实例化
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsInstantiable(this Type type)
    {
        return type is { IsClass: true, IsAbstract: false }
            && !type.IsStatic();
    }

    /// <summary>
    /// 检查类型是否是整数类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsInteger(this Type type)
    {
        // 如果是枚举或浮点类型则直接返回
        if (type.IsEnum || type.IsDecimal())
        {
            return false;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 是否是<see cref="List{T}"/>类型。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsList(this Type type)
    {
        return typeof(IList).IsAssignableFrom(type);
    }

    /// <summary>
    /// 判断该类型是否为可空类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNullableType(this Type type)
    {
        return (type.IsGenericType && type.
          GetGenericTypeDefinition().Equals
          (TouchSocketCoreUtility.NullableType));
    }

    /// <summary>
    /// 检查类型是否是数值类型
    /// </summary>
    /// <param name="type"><see cref="Type"/></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsNumeric(this Type type)
    {
        return type.IsInteger() || type.IsDecimal();
    }

    /// <summary>
    /// 判断是否为静态类。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsStatic(this Type type)
    {
        return type.IsAbstract && type.IsSealed;
    }

    /// <summary>
    /// 判断为结构体
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsStruct(this Type type)
    {
        return !type.IsPrimitive && !type.IsClass && !type.IsEnum && type.IsValueType;
    }

    /// <summary>
    /// 判断该类型是否为值元组类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsValueTuple(this Type type)
    {
        return type.IsValueType &&
             type.IsGenericType &&
             type.FullName.StartsWith("System.ValueTuple");
    }

    /// <summary>
    /// 判断类型是否为基础类型，此处认为除<see cref="Type.IsPrimitive"/>为<see langword="true"/>的类型以外，还包含下列类型：
    /// <list type="bullet">
    /// <item><see cref="string"/></item>
    /// </list>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsPrimitive(this Type type)
    {
        return type.IsPrimitive || type == typeof(string);
    }

    #endregion Type

    #region Memory

    /// <summary>
    /// 从指定的 <see cref="Memory{T}"/> 对象中获取内部数组。
    /// </summary>
    /// <param name="memory">要获取内部数组的内存对象。</param>
    /// <returns>一个表示内存内部数组的 <see cref="ArraySegment{T}"/> 对象。</returns>
    /// <remarks>
    /// 此方法通过将 <see cref="Memory{T}"/> 对象转换为 <see cref="ReadOnlyMemory{T}"/> 对象，
    /// 然后调用 <see cref="GetArray(ReadOnlyMemory{byte})"/> 方法来获取内部数组。
    /// </remarks>
    public static ArraySegment<byte> GetArray(this Memory<byte> memory)
    {
        return ((ReadOnlyMemory<byte>)memory).GetArray();
    }

    /// <summary>
    /// 从指定的 <see cref="ReadOnlyMemory{T}"/> 对象中获取内部数组。
    /// </summary>
    /// <param name="memory">要获取内部数组的只读内存对象。</param>
    /// <returns>一个表示内存内部数组的 <see cref="ArraySegment{T}"/> 对象。</returns>
    /// <remarks>
    /// 此方法尝试通过 <see cref="MemoryMarshal.TryGetArray"/> 方法获取内存的内部数组。
    /// 如果成功，直接返回结果；如果失败（即内存不是由数组支持的），则将内存复制到数组并返回该数组的段。
    /// </remarks>
    public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
    {
        return MemoryMarshal.TryGetArray(memory, out var result) ? result : new ArraySegment<byte>(memory.ToArray());
    }

    #endregion Memory

    #region EndPoint

    /// <summary>
    /// 从<see cref="EndPoint"/>中获得IP地址。
    /// </summary>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    public static string GetIP(this EndPoint endPoint)
    {
        var r = endPoint.ToString().LastIndexOf(":");
        return endPoint.ToString().Substring(0, r);
    }

    /// <summary>
    /// 从<see cref="EndPoint"/>中获得Port。
    /// </summary>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    public static int GetPort(this EndPoint endPoint)
    {
        var r = endPoint.ToString().LastIndexOf(":");
        return Convert.ToInt32(endPoint.ToString().Substring(r + 1, endPoint.ToString().Length - (r + 1)));
    }

    #endregion EndPoint

    #region Span<byte>
    /// <summary>
    /// 将字节的连续内存表示形式转换为字符串。
    /// </summary>
    /// <param name="span">要转换为字符串的字节范围。</param>
    /// <param name="encoding">用于解码字节的编码。</param>
    /// <returns>转换后的字符串。</returns>
    public static unsafe string ToString(this Span<byte> span, Encoding encoding)
    {
        // 根据目标框架选择不同的实现方式
#if NET6_0_OR_GREATER
        // 对于.NET 6.0及以上版本，直接使用内置方法
        return encoding.GetString(span);
#elif NET462_OR_GREATER
        // 对于.NET 4.6.2到.NET 5.0的版本，使用指针访问提高效率
        fixed (byte* p = &span[0])
        {
            return encoding.GetString(p, span.Length);
        }
#else
        // 对于更早的版本，将Span转换为数组再处理
        return encoding.GetString(span.ToArray());
#endif
    }

    /// <summary>
    /// 将只读的字节连续内存表示形式转换为字符串。
    /// </summary>
    /// <param name="span">要转换为字符串的只读字节范围。</param>
    /// <param name="encoding">用于解码字节的编码。</param>
    /// <returns>转换后的字符串。</returns>
    public static unsafe string ToString(this ReadOnlySpan<byte> span, Encoding encoding)
    {
        if (span.IsEmpty)
        {
            return string.Empty;
        }

        // 根据目标框架选择不同的实现方式
#if NET6_0_OR_GREATER|| NETSTANDARD2_1_OR_GREATER
        // 对于.NET 6.0及以上版本，直接使用内置方法
        return encoding.GetString(span);
#elif NET462_OR_GREATER||NETSTANDARD2_0_OR_GREATER
        // 对于.NET 4.6.2到.NET 5.0的版本，使用指针访问提高效率

        fixed (byte* p = &span[0])
        {
            return encoding.GetString(p, span.Length);
        }
#else
        // 对于更早的版本，将ReadOnlySpan转换为数组再处理
        return encoding.GetString(span.ToArray());
#endif
    }
    #endregion

    #region DateTime
    private static readonly DateTime s_utc_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly DateTimeOffset s_utc1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);


    /// <summary>
    /// 将DateTime对象转换为GMT格式的字符串。
    /// </summary>
    /// <param name="dt">要转换的DateTime对象。</param>
    /// <returns>转换后的GMT格式字符串。</returns>
    public static string ToGMTString(this DateTime dt)
    {
        // 使用"r"格式字符串和InvariantCulture确保GMT格式的正确性
        return dt.ToString("r", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 将DateTime对象转换为自1970年1月1日以来的毫秒数的32位无符号整数表示。
    /// </summary>
    /// <param name="time">要转换的DateTime对象。</param>
    /// <returns>自1970年1月1日以来的毫秒数的32位无符号整数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUnsignedMillis(this in DateTime time)
    {
        // 计算自1970年1月1日以来的毫秒数，并转换为32位无符号整数
        return (uint)(Convert.ToInt64(time.Subtract(s_utc_time).TotalMilliseconds) & 0xffffffff);
    }

    /// <summary>
    /// 将DateTimeOffset对象转换为自1970年1月1日以来的毫秒数的32位无符号整数表示。
    /// </summary>
    /// <param name="time">要转换的DateTimeOffset对象。</param>
    /// <returns>自1970年1月1日以来的毫秒数的32位无符号整数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUnsignedMillis(this in DateTimeOffset time)
    {
        // 计算自1970年1月1日以来的毫秒数，并转换为32位无符号整数
        return (uint)(Convert.ToInt64(time.Subtract(s_utc1970).TotalMilliseconds) & 0xffffffff);
    }
    #endregion

    #region Stream
#if (!NET6_0_OR_GREATER) && (!NETSTANDARD2_1_OR_GREATER)
    /// <summary>
    /// 异步读取数据到指定的内存区域。
    /// </summary>
    /// <param name="stream">要读取数据的流。</param>
    /// <param name="memory">要读取数据到的内存区域。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>读取到的数据长度。</returns>
    public static Task<int> ReadAsync(this Stream stream, Memory<byte> memory, CancellationToken cancellationToken)
    {
        // 获取内存区域对应的数组
        var bytes = memory.GetArray();
        // 调用异步方法读取数据到指定的数组区域
        return stream.ReadAsync(bytes.Array, bytes.Offset, bytes.Count);
    }

    /// <summary>
    /// 从流中读取数据到指定的字节跨度。
    /// </summary>
    /// <param name="stream">要读取数据的流。</param>
    /// <param name="span">要读取数据到的字节跨度。</param>
    /// <returns>读取到的数据长度。</returns>
    public static int Read(this Stream stream, Span<byte> span)
    {
        // 获取字节跨度的长度
        var len = span.Length;
        // 从字节池中租用一个缓冲区
        var buffer = BytePool.Default.Rent(len);
        try
        {
            // 从流中读取数据到缓冲区
            var r = stream.Read(buffer, 0, len);
            // 将缓冲区的数据复制到字节跨度
            Unsafe.CopyBlock(ref span[0], ref buffer[0], (uint)r);
            // 返回读取到的数据长度
            return r;
        }
        finally
        {
            // 将缓冲区归还到字节池
            BytePool.Default.Return(buffer);
        }
    }

    /// <summary>
    /// 异步地将只读内存块中的字节内容写入流中，并支持取消操作。
    /// </summary>
    /// <param name="stream">此方法扩展的流对象，表示要写入的流。</param>
    /// <param name="memory">只读内存块，其中包含要写入的字节数据。</param>
    /// <param name="token">用于取消异步写入操作的取消令牌。</param>
    /// <remarks>
    /// 此方法利用内存块的 GetArray 方法获取数组段信息，然后使用现有的 WriteAsync 方法异步地将内容写入流中，提高了写入操作的效率和灵活性。
    /// </remarks>
    public static async ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        var segment = memory.GetArray();
        await stream.WriteAsync(segment.Array, segment.Offset, segment.Count, token);
    }

    /// <summary>
    /// 使用缓冲区高效地将只读字节跨度写入流。
    /// </summary>
    /// <param name="stream">要写入的流。</param>
    /// <param name="span">要写入流的只读字节跨度。</param>
    /// <remarks>
    /// 该方法通过使用字节池来优化内存分配和释放，减少内存分配的开销。
    /// </remarks>
    public static void Write(this Stream stream, ReadOnlySpan<byte> span)
    {
        // 获取字节跨度的长度
        var len = span.Length;
        // 从字节池中租用一个缓冲区
        var buffer = BytePool.Default.Rent(len);
        try
        {
            // 将字节跨度的内容复制到租用的缓冲区中
            span.CopyTo(buffer);

            // 将缓冲区的内容写入流
            stream.Write(buffer, 0, len);
        }
        finally
        {
            // 将使用完的缓冲区归还到字节池，以便其他操作重用
            BytePool.Default.Return(buffer);
        }
    }
#endif

    /// <summary>
    /// 读取流中的所有字节并返回字节数组。
    /// </summary>
    /// <param name="stream">要读取的流。</param>
    /// <returns>包含流中所有字节的字节数组。</returns>
    /// <exception cref="ArgumentNullException">当输入的流为 null 时抛出。</exception>
    /// <exception cref="IOException">当读取的字节数与流的长度不匹配时抛出。</exception>
    public static byte[] ReadAllToByteArray(this Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream), "输入的 Stream 不能为 null。");
        }

        // 如果流支持长度属性，并且流的位置在起始位置，可以直接创建对应长度的数组
        if (stream.CanSeek && stream.Length > 0 && stream.Position == 0)
        {
            var buffer = new byte[stream.Length];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
            {
                throw new IOException("读取的字节数与流的长度不匹配。");
            }
            return buffer;
        }

        // 如果流不支持长度属性或位置不在起始位置，使用 MemoryStream 来读取数据
        using (var memoryStream = new MemoryStream())
        {
            var buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }
            return memoryStream.ToArray();
        }
    }
    #endregion

    #region IEnumerable
    /// <summary>
    /// 获取安全的枚举器。
    /// </summary>
    public static IEnumerable<T> GetSafeEnumerator<T>(this IEnumerable<T> enumerator)
    {
        if (enumerator is null)
        {
            return [];
        }

        return enumerator;
    }
    #endregion
}