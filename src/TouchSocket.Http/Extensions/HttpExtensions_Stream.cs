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
    public static partial class HttpExtensions
    {
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

            public override bool CanWrite => m_httpResponse.CanWrite;

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
                await this.m_httpResponse.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count)).ConfigureFalseAwait();
            }
        }

        #endregion
    }
}
