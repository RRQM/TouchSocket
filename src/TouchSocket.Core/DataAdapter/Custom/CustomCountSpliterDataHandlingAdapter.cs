using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public abstract class CustomCountSpliterDataHandlingAdapter<TCountSpliterRequestInfo> : CustomDataHandlingAdapter<TCountSpliterRequestInfo> where TCountSpliterRequestInfo : IRequestInfo
    {
        public int Count { get; }

        public CustomCountSpliterDataHandlingAdapter(int count, byte[] spliter)
        {
            if (count < 2)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(count), count, 2);
            }

            ThrowHelper.ThrowArgumentNullExceptionIf(spliter, nameof(spliter));

            this.Count = count;
            this.Spliter = spliter;
        }

        public byte[] Spliter { get; }

        protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TCountSpliterRequestInfo request, ref int tempCapacity)
        {
            var position = byteBlock.Position;
            var count = 0;

            var spanAll = byteBlock.Span.Slice(position);
            var startIndex = 0;
            var length = 0;

            var spliterSpan = new ReadOnlySpan<byte>(this.Spliter);

            while (true)
            {
                if (spanAll.Length<= startIndex + length)
                {
                    byteBlock.Position = position;
                    return FilterResult.Cache;
                }
                var currentSpan = spanAll.Slice(startIndex+length);
                var r = currentSpan.IndexOf(spliterSpan);
                if (r < 0)
                {
                    byteBlock.Position = position;
                    return FilterResult.Cache;
                }

                count++;

                if (count == 1)
                {
                    startIndex = r;
                }

                length += r + spliterSpan.Length;

                if (count == this.Count)
                {
                    var span = spanAll.Slice(startIndex, length);

                    request = this.GetInstance(span);
                    byteBlock.Position += length;
                    return FilterResult.Success;
                }
            }
        }

        protected abstract TCountSpliterRequestInfo GetInstance(in ReadOnlySpan<byte> dataSpan);
    }

    public interface ICountSpliterRequestInfo : IRequestInfo
    {

        bool OnParsing(ReadOnlySpan<byte> startCode);
    }
}
