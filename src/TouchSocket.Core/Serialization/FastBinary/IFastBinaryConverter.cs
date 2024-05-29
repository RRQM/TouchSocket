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
    /// FastBinary转换器
    /// </summary>
    public interface IFastBinaryConverter
    {
        /// <summary>
        /// 读取对象，不需要考虑为null的情况。
        /// </summary>
        /// <param name="buffer">读取的内存</param>
        /// <param name="offset">内存偏移</param>
        /// <param name="len">该数据对象应该占用的长度</param>
        /// <returns>返回实际对象</returns>
        object Read<TByteBlock>(ref TByteBlock byteBlock, Type type) where TByteBlock : IByteBlock;

        /// <summary>
        /// 写入对象，不需要考虑为null的情况。
        /// </summary>
        /// <param name="byteBlock">存储内存块</param>
        /// <param name="obj">需要序列化的对象</param>
        /// <returns>返回该对象实际占用的字节长度。</returns>
        void Write<TByteBlock>(ref TByteBlock byteBlock,in object obj) where TByteBlock : IByteBlock;
    }

    /// <summary>
    /// FastBinary转换器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FastBinaryConverter<T> : IFastBinaryConverter
    {
        object IFastBinaryConverter.Read<TByteBlock>(ref TByteBlock byteBlock, Type type)
        {
            return this.Read(ref byteBlock, type);
        }

        void IFastBinaryConverter.Write<TByteBlock>(ref TByteBlock byteBlock,in object obj)
        {
            this.Write(ref byteBlock, (T)obj);
        }

        /// <inheritdoc cref="IFastBinaryConverter.Read{TByteBlock}(ref TByteBlock, Type)"/>
        protected abstract T Read<TByteBlock>(ref TByteBlock byteBlock, Type type) where TByteBlock : IByteBlock;

        /// <inheritdoc cref="IFastBinaryConverter.Write{TByteBlock}(ref TByteBlock,in object)"/>
        protected abstract void Write<TByteBlock>(ref TByteBlock byteBlock,in T obj) where TByteBlock : IByteBlock;
    }
}