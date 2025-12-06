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

using System.IO.Pipelines;

namespace TouchSocket.Http;

/// <summary>
/// 表示HTTP响应的内容部分，是一个抽象类
/// </summary>
public abstract class HttpContent
{
    /// <summary>
    /// 内部方法，用于构建HTTP响应的内容
    /// </summary>
    /// <typeparam name="TWriter">实现IByteBlock接口的类型</typeparam>
    /// <param name="writer">字节块的引用</param>
    /// <returns>返回一个布尔值，表示构建内容是否成功</returns>
    internal bool InternalBuildingContent<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        return this.OnBuildingContent(ref writer);
    }

    /// <summary>
    /// 内部方法，用于构建HTTP头
    /// </summary>
    /// <param name="header">HTTP头的接口实现</param>
    internal void InternalBuildingHeader(IHttpHeader header)
    {
        this.OnBuildingHeader(header);
    }

    internal Task InternalWriteContent(PipeWriter writer, CancellationToken cancellationToken)
    {
        return this.WriteContent(writer, cancellationToken);
    }

    /// <summary>
    /// 抽象方法，由子类实现，用于构建HTTP响应的内容
    /// </summary>
    /// <typeparam name="TWriter">实现IByteBlock接口的类型</typeparam>
    /// <param name="writer">字节块的引用</param>
    /// <returns>返回一个布尔值，表示构建内容是否成功</returns>
    protected abstract bool OnBuildingContent<TWriter>(ref TWriter writer) where TWriter : IBytesWriter;

    /// <summary>
    /// 抽象方法，由子类实现，用于构建HTTP头
    /// </summary>
    /// <param name="header">HTTP头的接口实现</param>
    protected abstract void OnBuildingHeader(IHttpHeader header);

    protected abstract Task WriteContent(PipeWriter writer, CancellationToken cancellationToken);

    #region implicit

    /// <summary>
    /// 将字符串内容隐式转换为HttpContent对象，使用UTF-8编码。
    /// </summary>
    /// <param name="content">要转换的字符串内容。</param>
    /// <returns>一个新的StringHttpContent对象。</returns>
    public static implicit operator HttpContent(string content)
    {
        return StringHttpContent.FromText(content);
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

    #endregion implicit
}