// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/()
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace TouchSocket.Core;
/// <summary>
/// 提供用于创建 <see cref="ReadOnlySpan{T}"/> 和 <see cref="Span{T}"/> 的辅助方法。
/// </summary>
public static class EasyMemoryMarshal
{



#if NET6_0_OR_GREATER
    /// <summary>
    /// 创建一个可写的 <see cref="Span{T}"/>，其引用指定的内存地址和长度。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（首元素）。</param>
    /// <param name="length">引用的元素个数。</param>
    /// <returns>一个新的 <see cref="Span{T}"/>。</returns>
    public static Span<T> CreateSpan<T>(ref T reference, int length) where T : unmanaged
    {
        return MemoryMarshal.CreateSpan(ref reference, length);
    }

    /// <summary>
    /// 创建一个可写的 <see cref="Span{T}"/>，其引用指定的内存地址，长度为 1。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（单个元素）。</param>
    /// <returns>一个新的长度为 1 的 <see cref="Span{T}"/>。</returns>
    public static Span<T> CreateSpan<T>(ref T reference) where T : unmanaged
    {
        return MemoryMarshal.CreateSpan(ref reference, 1);
    }

    /// <summary>
    /// 创建一个只读的 <see cref="ReadOnlySpan{T}"/>，其引用指定的内存地址和长度。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（首元素）。</param>
    /// <param name="length">引用的元素个数。</param>
    /// <returns>一个新的 <see cref="ReadOnlySpan{T}"/>。</returns>
    public static ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length) where T : unmanaged
    {
        return MemoryMarshal.CreateReadOnlySpan(ref reference, length);
    }
    /// <summary>
    /// 创建一个只读的 <see cref="ReadOnlySpan{T}"/>，其引用指定的内存地址，长度为 1。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（单个元素）。</param>
    /// <returns>一个新的长度为 1 的 <see cref="ReadOnlySpan{T}"/>。</returns>
    public static ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference) where T : unmanaged
    {
        return MemoryMarshal.CreateReadOnlySpan(ref reference, 1);
    }
#else

    /// <summary>
    /// 创建一个 <see cref="ReadOnlySpan{T}"/>，其引用指定的内存地址和长度。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（首元素）。</param>
    /// <param name="length">引用的元素个数。</param>
    /// <returns>一个新的 <see cref="ReadOnlySpan{T}"/>。</returns>
    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length)
        where T : unmanaged
    {
        // 使用不安全代码创建  
        fixed (T* ptr = &reference)
        {
            return new ReadOnlySpan<T>(ptr, length);
        }
    }

    /// <summary>
    /// 创建一个 <see cref="ReadOnlySpan{T}"/>，其引用指定的内存地址，长度为 1。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（单个元素）。</param>
    /// <returns>一个新的长度为 1 的 <see cref="ReadOnlySpan{T}"/>。</returns>
    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference)
        where T : unmanaged
    {
        // 使用不安全代码创建  
        fixed (T* ptr = &reference)
        {
            return new ReadOnlySpan<T>(ptr, 1);
        }
    }
    /// <summary>
    /// 创建一个可写的 <see cref="Span{T}"/>，其引用指定的内存地址和长度。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（首元素）。</param>
    /// <param name="length">引用的元素个数。</param>
    /// <returns>一个新的 <see cref="Span{T}"/>。</returns>
    public static unsafe Span<T> CreateSpan<T>(ref T reference, int length) where T : unmanaged
    {
        // 使用不安全代码创建  
        fixed (T* ptr = &reference)
        {
            return new Span<T>(ptr, length);
        }
    }

    /// <summary>
    /// 创建一个可写的 <see cref="Span{T}"/>，其引用指定的内存地址，长度为 1。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是非托管类型。</typeparam>
    /// <param name="reference">引用的内存地址（单个元素）。</param>
    /// <returns>一个新的长度为 1 的 <see cref="Span{T}"/>。</returns>
    public static unsafe Span<T> CreateSpan<T>(ref T reference) where T : unmanaged
    {
        // 使用不安全代码创建  
        fixed (T* ptr = &reference)
        {
            return new Span<T>(ptr, 1);
        }
    }
#endif



}
