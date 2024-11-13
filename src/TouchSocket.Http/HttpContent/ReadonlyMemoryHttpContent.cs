using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 只读内存级别的Http内容。
    /// </summary>
    public class ReadonlyMemoryHttpContent : HttpContent
    {
        private readonly ReadOnlyMemory<byte> m_memory;

        public ReadonlyMemoryHttpContent(ReadOnlyMemory<byte> memory)
        {
            this.m_memory = memory;
        }

        /// <inheritdoc/>
        protected override bool OnBuildingContent<TByteBlock>(ref TByteBlock byteBlock)
        {
            if (this.m_memory.IsEmpty)
            {
                return true;//直接构建成功，也不用调用后续的WriteContent
            }
            if (byteBlock.FreeLength>this.m_memory.Length)
            {
                //如果空闲空间足够，构建成功，也不用调用后续的WriteContent
                byteBlock.Write(this.m_memory.Span);
                return true;
            }

            //返回false，提示后续数据可能太大，通过WriteContent执行。
            return false;
        }

        /// <inheritdoc/>
        protected override void OnBuildingHeader(IHttpHeader header)
        {
            header.Add(HttpHeaders.ContentLength,this.m_memory.Length.ToString());
        }

        /// <inheritdoc/>
        protected override async Task WriteContent(Func<ReadOnlyMemory<byte>, Task> writeFunc, CancellationToken token)
        {
            await writeFunc(this.m_memory).ConfigureAwait(false);
        }
    }
}
