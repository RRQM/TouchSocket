using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// 内部方法，用于构建HTTP响应头
        /// </summary>
        /// <param name="header">HTTP响应头的接口实现</param>
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
        /// 抽象方法，由子类实现，用于构建HTTP响应头
        /// </summary>
        /// <param name="header">HTTP响应头的接口实现</param>
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
    }
}