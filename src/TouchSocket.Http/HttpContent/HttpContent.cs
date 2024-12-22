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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// 表示HTTP响应的内容部分，是一个抽象类
    /// </summary>
    public abstract class HttpContent
    {
        /// <summary>
        /// 内部方法，用于构建HTTP头
        /// </summary>
        /// <param name="header">HTTP头的接口实现</param>
        internal void InternalBuildingHeader(IHttpHeader header)
        {
            this.OnBuildingHeader(header);
        }

        /// <summary>
        /// 内部方法，用于写入HTTP响应内容
        /// </summary>
        /// <param name="func">一个函数，用于处理字节块的写入操作</param>
        /// <param name="token">用于取消操作的令牌</param>
        /// <returns>返回一个任务对象，代表异步写入操作</returns>
        internal Task InternalWriteContent(Func<ReadOnlyMemory<byte>, Task> func, CancellationToken token)
        {
            return this.WriteContent(func, token);
        }

        /// <summary>
        /// 内部方法，用于构建HTTP响应的内容
        /// </summary>
        /// <typeparam name="TByteBlock">实现IByteBlock接口的类型</typeparam>
        /// <param name="byteBlock">字节块的引用</param>
        /// <returns>返回一个布尔值，表示构建内容是否成功</returns>
        internal bool InternalBuildingContent<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            return this.OnBuildingContent(ref byteBlock);
        }

        /// <summary>
        /// 抽象方法，由子类实现，用于构建HTTP头
        /// </summary>
        /// <param name="header">HTTP头的接口实现</param>
        protected abstract void OnBuildingHeader(IHttpHeader header);

        /// <summary>
        /// 抽象方法，由子类实现，用于构建HTTP响应的内容
        /// </summary>
        /// <typeparam name="TByteBlock">实现IByteBlock接口的类型</typeparam>
        /// <param name="byteBlock">字节块的引用</param>
        /// <returns>返回一个布尔值，表示构建内容是否成功</returns>
        protected abstract bool OnBuildingContent<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock;

        /// <summary>
        /// 抽象方法，由子类实现，用于写入HTTP响应内容
        /// </summary>
        /// <param name="writeFunc">一个函数，用于处理字节块的写入操作</param>
        /// <param name="token">用于取消操作的令牌</param>
        /// <returns>返回一个任务对象，代表异步写入操作</returns>
        protected abstract Task WriteContent(Func<ReadOnlyMemory<byte>, Task> writeFunc, CancellationToken token);

        /// <summary>
        /// 将字符串内容隐式转换为HttpContent对象，使用UTF-8编码。
        /// </summary>
        /// <param name="content">要转换的字符串内容。</param>
        /// <returns>一个新的StringHttpContent对象。</returns>
        public static implicit operator HttpContent(string content)
        {
            return new StringHttpContent(content, Encoding.UTF8);
        }

        /// <summary>
        /// 将只读内存字节内容隐式转换为HttpContent对象。
        /// </summary>
        /// <param name="content">要转换的只读内存字节内容。</param>
        /// <returns>一个新的ReadonlyMemoryHttpContent对象。</returns>
        public static implicit operator HttpContent(ReadOnlyMemory<byte> content)
        {
            return new ReadonlyMemoryHttpContent(content);
        }

        /// <summary>
        /// 将字节数组内容隐式转换为HttpContent对象。
        /// </summary>
        /// <param name="content">要转换的字节数组内容。</param>
        /// <returns>一个新的ReadonlyMemoryHttpContent对象。</returns>
        public static implicit operator HttpContent(byte[] content)
        {
            return new ReadonlyMemoryHttpContent(content);
        }

        /// <summary>
        /// 将流内容隐式转换为HttpContent对象。
        /// </summary>
        /// <param name="content">要转换的流内容。</param>
        /// <returns>一个新的StreamHttpContent对象。</returns>
        public static implicit operator HttpContent(Stream content)
        {
            return new StreamHttpContent(content);
        }
    }
}