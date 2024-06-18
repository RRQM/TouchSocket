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
using TouchSocket.Resources;

namespace TouchSocket.Core
{
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
        /// 获取自定义attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumObj) where T : Attribute
        {
            var type = enumObj.GetType();
            var enumName = Enum.GetName(type, enumObj);  //获取对应的枚举名
            var field = type.GetField(enumName);
            var attr = field.GetCustomAttribute(typeof(T), false);

            return (T)attr;
        }

        #endregion Enum

        #region SetBit

        public static ulong SetBit(this ulong b, int index, bool bitvalue)
        {
            if (index < 0 || index > 63)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 63);
            }

            ulong baseNumber = 1;
            if (bitvalue)
            {
                return b | baseNumber << index;
            }
            else
            {
                return b & ~(baseNumber << index);
            }
        }

        public static uint SetBit(this uint b, int index, bool bitvalue)
        {
            if (index < 0 || index > 31)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 31);
            }

            uint baseNumber = 1;
            if (bitvalue)
            {
                return b | baseNumber << index;
            }
            else
            {
                return b & ~(baseNumber << index);
            }
        }

        public static ushort SetBit(this ushort b, int index, bool bitvalue)
        {
            if (index < 0 || index > 15)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 15);
            }

            ushort baseNumber = 1;
            if (bitvalue)
            {
                return (ushort)(b | baseNumber << index);
            }
            else
            {
                return (ushort)(b & ~(baseNumber << index));
            }
        }

        public static byte SetBit(this byte b, int index, bool bitvalue)
        {
            if (index < 0 || index > 7)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
            }

            byte baseNumber = 1;
            if (bitvalue)
            {
                return (byte)(b | baseNumber << index);
            }
            else
            {
                return (byte)(b & ~(baseNumber << index));
            }
        }

        #endregion

        #region GetBit
        public static bool GetBit(this ulong b, int index)
        {
            if (index > 63 || index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 63);
            }
            return (b & (ulong)1 << index) != 0;
        }

        public static bool GetBit(this uint b, int index)
        {
            if (index > 31 || index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 31);
            }
            return (b & (uint)1 << index) != 0;
        }

        public static bool GetBit(this ushort b, int index)
        {
            if (index > 15 || index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 15);
            }
            return (b & (ushort)1 << index) != 0;
        }

        public static bool GetBit(this byte b, int index)
        {
            if (index > 7 || index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
            }
            return (b & (byte)1 << index) != 0;
        }
        #endregion

        #region Byte

        ///// <summary>
        ///// 获取字节中的指定Bit的值
        ///// </summary>
        ///// <param name="this">字节</param>
        ///// <param name="index">Bit的索引值(0-7)</param>
        ///// <returns></returns>
        //public static int GetBit(this byte @this, int index)
        //{
        //    byte x;
        //    switch (index)
        //    {
        //        case 0: { x = 0x01; } break;
        //        case 1: { x = 0x02; } break;
        //        case 2: { x = 0x04; } break;
        //        case 3: { x = 0x08; } break;
        //        case 4: { x = 0x10; } break;
        //        case 5: { x = 0x20; } break;
        //        case 6: { x = 0x40; } break;
        //        case 7: { x = 0x80; } break;
        //        default:
        //            {
        //                ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
        //                return default;
        //            }
        //    }
        //    return (@this & x) == x ? 1 : 0;
        //}

        ///// <summary>
        ///// 设置字节中的指定Bit的值
        ///// </summary>
        ///// <param name="this">字节</param>
        ///// <param name="index">Bit的索引值(0-7)</param>
        ///// <param name="bitvalue">Bit值(0,1)</param>
        ///// <returns></returns>
        //public static byte SetBit(this byte @this, int index, int bitvalue)
        //{
        //    var _byte = @this;
        //    if (bitvalue == 0)
        //    {
        //        switch (index)
        //        {
        //            case 0: { return _byte &= 0xFE; }
        //            case 1: { return _byte &= 0xFD; }
        //            case 2: { return _byte &= 0xFB; }
        //            case 3: { return _byte &= 0xF7; }
        //            case 4: { return _byte &= 0xEF; }
        //            case 5: { return _byte &= 0xDF; }
        //            case 6: { return _byte &= 0xBF; }
        //            case 7: { return _byte &= 0x7F; }
        //            default:
        //                {
        //                    ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
        //                    return default;
        //                }
        //        }
        //    }
        //    else
        //    {
        //        switch (index)
        //        {
        //            case 0: { return _byte |= 0x01; }
        //            case 1: { return _byte |= 0x02; }
        //            case 2: { return _byte |= 0x04; }
        //            case 3: { return _byte |= 0x08; }
        //            case 4: { return _byte |= 0x10; }
        //            case 5: { return _byte |= 0x20; }
        //            case 6: { return _byte |= 0x40; }
        //            case 7: { return _byte |= 0x80; }
        //            default:
        //                {
        //                    ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(index), index, 0, 7);
        //                    return default;
        //                }
        //        }
        //    }
        //}

        #endregion Byte

        #region Byte[]

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
            return string.IsNullOrEmpty(splite)
                ? BitConverter.ToString(buffer, offset, length).Replace("-", string.Empty)
                : BitConverter.ToString(buffer, offset, length).Replace("-", splite);
        }

        /// <summary>
        /// 字节数组转16进制字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static string ByBytesToHexString(this byte[] buffer, string splite = default)
        {
            return ByBytesToHexString(buffer, 0, buffer.Length, splite);
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
        public static List<int> IndexOfInclude(this ReadOnlySpan<byte> srcByteArray, int offset, int length, byte[] subByteArray)
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

        public static string ToUtf8String(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static string ToUtf8String(this byte[] data, int offset, int length)
        {
            return Encoding.UTF8.GetString(data, offset, length);
        }

        #endregion Byte[]

        #region Type

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
            if (type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(float))
            {
                return true;
            }

            return Type.GetTypeCode(type) switch
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
            return TouchSocketCoreUtility.dicType.IsAssignableFrom(type);
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
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type theType)
        {
            return (theType.IsGenericType && theType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType));
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
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStatic(this Type targetType)
        {
            return targetType.IsAbstract && targetType.IsSealed;
        }

        /// <summary>
        /// 判断为结构体
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStruct(this Type targetType)
        {
            return !targetType.IsPrimitive && !targetType.IsClass && !targetType.IsEnum && targetType.IsValueType;
        }

        /// <summary>
        /// 判断该类型是否为值元组类型
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsValueTuple(this Type theType)
        {
            return theType.IsValueType &&
                 theType.IsGenericType &&
                 theType.FullName.StartsWith("System.ValueTuple");
        }

        #endregion Type

        #region Memory

        public static ArraySegment<byte> GetArray(this Memory<byte> memory)
        {
            return ((ReadOnlyMemory<byte>)memory).GetArray();
        }

        public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out var result))
            {
                return result;
            }
            return new ArraySegment<byte>(memory.ToArray());
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
        public unsafe static string ToString(this Span<byte> span, Encoding encoding)
        {
#if NET6_0_OR_GREATER
            return encoding.GetString(span);
#elif NET462_OR_GREATER
            fixed (byte* p = &span[0])
            {
                return encoding.GetString(p, span.Length);
            }
#else
            return encoding.GetString(span.ToArray());
#endif
        }

        public unsafe static string ToString(this ReadOnlySpan<byte> span, Encoding encoding)
        {
#if NET6_0_OR_GREATER
          return  encoding.GetString(span);
#elif NET462_OR_GREATER
            fixed (byte* p = &span[0])
            {
                return encoding.GetString(p, span.Length);
            }
#else
            return encoding.GetString(span.ToArray());
#endif
        }
        #endregion

        #region DateTime
        private static readonly DateTime s_utc_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly DateTimeOffset s_utc1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// 格林尼治标准时间
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToGMTString(this DateTime dt)
        {
            return dt.ToString("r", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 将时间转为毫秒级别的短整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertTime(this in DateTime time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(s_utc_time).TotalMilliseconds) & 0xffffffff);
        }

        /// <summary>
        /// 将时间转为毫秒级别的短整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertTime(this in DateTimeOffset time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(s_utc1970).TotalMilliseconds) & 0xffffffff);
        }
        #endregion

        #region Stream
#if (!NET6_0_OR_GREATER)&&(!NETSTANDARD2_1_OR_GREATER)
        public static Task<int> ReadAsync(this Stream stream, Memory<byte> memory, CancellationToken cancellationToken)
        {
            var bytes = memory.GetArray();
            return stream.ReadAsync(bytes.Array, bytes.Offset, bytes.Count);
        }

        public static int Read(this Stream stream, Span<byte> span)
        {
            var len = span.Length;
            var buffer = BytePool.Default.Rent(len);
            try
            {
                int r = stream.Read(buffer, 0, len);
                Unsafe.CopyBlock(ref span[0], ref buffer[0], (uint)r);
                return r;
            }
            finally
            {
                BytePool.Default.Return(buffer);
            }
        }
#endif
        #endregion
    }
}