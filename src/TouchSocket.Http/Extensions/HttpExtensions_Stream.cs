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

namespace TouchSocket.Http;

public static partial class HttpExtensions
{
    /// <summary>
    /// 为指定的 HTTP 响应创建一个写入流。
    /// </summary>
    /// <param name="response">要关联写入流的 HTTP 响应对象。</param>
    /// <returns>一个 <see cref="Stream"/> 对象，用于向 HTTP 响应写入数据。</returns>
    /// <remarks>
    /// 此方法提供了一种方便的方式来获取一个写入流，该流可以直接与指定的 HTTP 响应对象关联，
    /// 从而简化了向 HTTP 响应写入数据的过程。
    /// </remarks>
    /// <typeparam name="TResponse">HTTP 响应的类型，必须是 <see cref="HttpResponse"/> 的一个子类。</typeparam>
    public static Stream CreateWriteStream<TResponse>(this TResponse response) where TResponse : HttpResponse
    {
        return new InternalWriteStream(response);
    }

    #region Class
    private class InternalWriteStream : Stream
    {
        private readonly HttpResponse m_httpResponse;

        public InternalWriteStream(HttpResponse httpResponse)
        {
            this.m_httpResponse = httpResponse;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_httpResponse.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count)).GetFalseAwaitResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await this.m_httpResponse.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count),cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    #endregion
}