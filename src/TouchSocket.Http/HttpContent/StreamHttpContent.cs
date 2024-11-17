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
    /// 继承自HttpContent的类，用于将Stream数据转换为可发送的HTTP内容。
    /// </summary>
    public class StreamHttpContent : HttpContent
    {
        private readonly int m_bufferLength;
        private readonly int m_maxSpeed;
        private readonly Stream m_stream;

        /// <summary>
        /// 初始化StreamHttpContent类的新实例。
        /// </summary>
        /// <param name="stream">要包装的流。</param>
        /// <param name="bufferLength">读取数据时使用的缓冲区长度，默认为64KB。</param>
        /// <param name="maxSpeed">传输内容的最大速度，默认为Int32最大值，表示不限速。</param>
        public StreamHttpContent(Stream stream, int bufferLength = 1024 * 64, int maxSpeed = int.MaxValue)
        {
            // 将提供的流分配给内部变量m_stream
            this.m_stream = stream;
            // 将提供的缓冲区长度分配给内部变量m_bufferLength
            this.m_bufferLength = bufferLength;
            // 将提供的最大速度分配给内部变量m_maxSpeed
            this.m_maxSpeed = maxSpeed;
        }

        /// <inheritdoc/>
        protected override bool OnBuildingContent<TByteBlock>(ref TByteBlock byteBlock)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override void OnBuildingHeader(IHttpHeader header)
        {
            header.Add(HttpHeaders.ContentLength, this.m_stream.Length.ToString());
        }

        /// <inheritdoc/>
        protected override async Task WriteContent(Func<ReadOnlyMemory<byte>, Task> writeFunc, CancellationToken token)
        {
            Memory<byte> memory = new byte[this.m_bufferLength];

            while (true)
            {
                var r = await this.m_stream.ReadAsync(memory, token).ConfigureAwait(false);
                if (r == 0)
                {
                    return;
                }
                await writeFunc.Invoke(memory).ConfigureAwait(false);
            }
        }
    }
}