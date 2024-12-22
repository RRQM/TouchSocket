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

namespace TouchSocket.Core
{
    /// <summary>
    /// 提供字节块扩展方法的静态类。
    /// </summary>
    public static class ByteBlockExtension
    {
        /// <summary>
        /// 将值类型的字节块转换为普通的字节块。
        /// </summary>
        /// <param name="valueByteBlock">要转换的值类型字节块。</param>
        /// <returns>一个新的字节块对象。</returns>
        public static ByteBlock AsByteBlock(this in ValueByteBlock valueByteBlock)
        {
            return new ByteBlock(valueByteBlock);
        }

        /// <summary>
        /// 将字节块转换为字节块流。
        /// </summary>
        /// <param name="byteBlock">要转换的字节块。</param>
        /// <param name="releaseTogether">是否在释放字节块时一起释放关联的资源，默认为true。</param>
        /// <returns>一个新的字节块流对象。</returns>
        public static ByteBlockStream AsStream(this ByteBlock byteBlock, bool releaseTogether = true)
        {
            return new ByteBlockStream(byteBlock, releaseTogether);
        }

        #region ToArray

        /// <summary>
        /// 将指定的字节块转换为【新】字节数组。
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块对象。</param>
        /// <param name="offset">起始偏移量。</param>
        /// <param name="length">要转换为数组的长度。</param>
        /// <returns>包含指定长度的【新】字节数组。</returns>
        public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlock
        {
            return byteBlock.Span.Slice(offset, length).ToArray();
        }

        /// <summary>
        /// 将指定的字节块转换为【新】字节数组，从指定偏移量开始，直到字节块的末尾。
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块对象。</param>
        /// <param name="offset">起始偏移量。</param>
        /// <returns>从指定偏移量到字节块末尾的【新】字节数组。</returns>
        public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlock
        {
            return ToArray(byteBlock, offset, byteBlock.Length - offset);
        }

        /// <summary>
        /// 将指定的字节块转换为【新】字节数组，从索引0开始，直到字节块的末尾。
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块对象。</param>
        /// <returns>整个字节块的【新】字节数组。</returns>
        public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            return ToArray(byteBlock, 0, byteBlock.Length);
        }

        /// <summary>
        /// 将指定的字节块从当前位置<see cref="IByteBlock.Position"/>转换为【新】字节数组，直到字节块的末尾。
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块对象。</param>
        /// <returns>从当前位置到字节块末尾的【新】字节数组。</returns>
        public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            return ToArray(byteBlock, byteBlock.Position, byteBlock.CanReadLength);
        }

        /// <summary>
        /// 将指定的字节块从当前位置<see cref="IByteBlock.Position"/>转换为【新】字节数组，指定长度。
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块对象。</param>
        /// <param name="length">要转换为数组的长度。</param>
        /// <returns>从当前位置开始，指定长度的【新】字节数组。</returns>
        public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock, int length) where TByteBlock : IByteBlock
        {
            return ToArray(byteBlock, byteBlock.Position, length);
        }

        #endregion ToArray

        #region AsSegment

        /// <summary>
        /// 将字节块【作为】数组段。
        /// <para>
        /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlock"/>生命周期内的，不能脱离生命周期使用。
        /// </para>
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">要转换的字节块实例。</param>
        /// <param name="offset">数组段的起始偏移量。</param>
        /// <param name="length">数组段的长度。</param>
        /// <returns>一个包含指定偏移量和长度的数组段。</returns>
        public static ArraySegment<byte> AsSegment<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlock
        {
            return byteBlock.TotalMemory.Slice(offset, length).GetArray();
        }

        /// <summary>
        /// 将字节块【作为】数组段，从指定偏移量开始，长度为可读长度。
        /// <para>
        /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlock"/>生命周期内的，不能脱离生命周期使用。
        /// </para>
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">要转换的字节块实例。</param>
        /// <param name="offset">数组段的起始偏移量。</param>
        /// <returns>一个从指定偏移量开始，长度为可读长度的数组段。</returns>
        public static ArraySegment<byte> AsSegment<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlock
        {
            return AsSegment(byteBlock, offset, byteBlock.Length - offset);
        }

        /// <summary>
        /// 将字节块【作为】数组段，从头开始，长度为指定长度。
        /// <para>
        /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlock"/>生命周期内的，不能脱离生命周期使用。
        /// </para>
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">要转换的字节块实例。</param>
        /// <returns>一个从头开始，长度为字节块长度的数组段。</returns>
        public static ArraySegment<byte> AsSegment<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            return AsSegment(byteBlock, 0, byteBlock.Length);
        }

        /// <summary>
        /// 将字节块【作为】数组段，从当前位置开始，指定长度。
        /// <para>
        /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlock"/>生命周期内的，不能脱离生命周期使用。
        /// </para>
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">要转换的字节块实例。</param>
        /// <param name="length">数组段的长度。</param>
        /// <returns>一个从当前位置开始，指定长度的数组段。</returns>
        public static ArraySegment<byte> AsSegmentTake<TByteBlock>(this TByteBlock byteBlock, int length) where TByteBlock : IByteBlock
        {
            return AsSegment(byteBlock, byteBlock.Position, length);
        }

        /// <summary>
        /// 将字节块【作为】数组段，从当前位置开始，长度为可读长度。
        /// <para>
        /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlock"/>生命周期内的，不能脱离生命周期使用。
        /// </para>
        /// </summary>
        /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
        /// <param name="byteBlock">要转换的字节块实例。</param>
        /// <returns>一个从当前位置开始，长度为可读长度的数组段。</returns>
        public static ArraySegment<byte> AsSegmentTake<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            return AsSegment(byteBlock, byteBlock.Position, byteBlock.CanReadLength);
        }

        #endregion AsSegment
    }
}