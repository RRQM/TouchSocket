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
    public class StreamHttpContent : HttpContent
    {
        private readonly int m_bufferLength;
        private readonly int m_maxSpeed;
        private readonly Stream m_stream;

        public StreamHttpContent(Stream stream, int bufferLength = 1024 * 64, int maxSpeed = int.MaxValue)
        {
            this.m_stream = stream;
            this.m_bufferLength = bufferLength;
            this.m_maxSpeed = maxSpeed;
        }

        protected override bool OnBuildingContent<TByteBlock>(ref TByteBlock byteBlock)
        {
            return false;
        }

        protected override void OnBuildingHeader(IHttpHeader header)
        {
            header.Add(HttpHeaders.ContentLength, this.m_stream.Length.ToString());
        }

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