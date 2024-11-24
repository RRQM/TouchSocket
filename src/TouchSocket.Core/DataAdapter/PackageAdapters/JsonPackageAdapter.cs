using System;
using System.Text;

namespace TouchSocket.Core
{
    /// <summary>
    /// 表示 JSON 包的类型。
    /// </summary>
    public enum JsonPackageKind
    {
        /// <summary>
        /// 无类型。
        /// </summary>
        None,
        /// <summary>
        /// JSON 对象类型。
        /// </summary>
        Object,
        /// <summary>
        /// JSON 数组类型。
        /// </summary>
        Array
    }

    /// <summary>
    /// 表示一个 JSON 包。
    /// </summary>
    public class JsonPackage : IRequestInfo
    {
        private string m_dataString;

        /// <summary>
        /// 初始化 <see cref="JsonPackage"/> 类的新实例。
        /// </summary>
        /// <param name="kind">JSON 包的类型。</param>
        /// <param name="encoding">编码格式。</param>
        /// <param name="memory">数据内存。</param>
        /// <param name="impurityData">杂质数据。</param>
        public JsonPackage(JsonPackageKind kind, Encoding encoding, ReadOnlyMemory<byte> memory, ReadOnlyMemory<byte> impurityData)
        {
            this.Kind = kind;
            this.Encoding = encoding;
            this.Data = memory;
            this.ImpurityData = impurityData;
        }

        /// <summary>
        /// 获取数据内存。
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// 获取数据字符串。
        /// </summary>
        public string DataString
        {
            get
            {
                this.m_dataString ??= this.Data.Span.ToString(this.Encoding);
                return this.m_dataString;
            }
        }

        /// <summary>
        /// 获取 JSON 包的类型。
        /// </summary>
        public JsonPackageKind Kind { get; }

        /// <summary>
        /// 获取编码格式。
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// 获取杂质数据。
        /// </summary>
        public ReadOnlyMemory<byte> ImpurityData { get; }
    }

    /// <summary>
    /// 处理 JSON 包的适配器。
    /// </summary>
    public class JsonPackageAdapter : CustomDataHandlingAdapter<JsonPackage>
    {
        private readonly byte[] m_closeBrace;

        private readonly byte[] m_leftSquareBracket;

        private readonly byte[] m_openBrace;

        private readonly byte[] m_rightSquareBracket;

        private int m_endCount = 0;

        private int m_startCount = 0;

        /// <summary>
        /// 初始化 <see cref="JsonPackageAdapter"/> 类的新实例。
        /// </summary>
        /// <param name="encoding">编码格式。</param>
        public JsonPackageAdapter(Encoding encoding)
        {
            this.m_openBrace = encoding.GetBytes("{");
            this.m_closeBrace = encoding.GetBytes("}");
            this.m_leftSquareBracket = encoding.GetBytes("[");
            this.m_rightSquareBracket = encoding.GetBytes("]");
            this.Encoding = encoding;
        }

        /// <summary>
        /// 初始化 <see cref="JsonPackageAdapter"/> 类的新实例，使用 UTF-8 编码。
        /// </summary>
        public JsonPackageAdapter() : this(Encoding.UTF8)
        {
        }

        /// <summary>
        /// 获取编码格式。
        /// </summary>
        public Encoding Encoding { get; }

        /// <inheritdoc/>
        protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref JsonPackage request, ref int tempCapacity)
        {
            var stringSpan = byteBlock.Span.Slice(byteBlock.Position);

            var myEnum = this.GetJsonPackageKind(stringSpan);
            if (myEnum == JsonPackageKind.None)
            {
                return FilterResult.Cache;
            }

            ReadOnlySpan<byte> startSpan;
            ReadOnlySpan<byte> endSpan;
            if (myEnum == JsonPackageKind.Object)
            {
                startSpan = this.m_openBrace;
                endSpan = this.m_closeBrace;
            }
            else
            {
                startSpan = this.m_leftSquareBracket;
                endSpan = this.m_rightSquareBracket;
            }

            var index = 0;
            while (true)
            {
                var span = stringSpan.Slice(index);
                var endIndex = span.IndexOf(endSpan);
                if (endIndex < 0)
                {
                    this.m_startCount = 0;
                    this.m_endCount = 0;
                    return FilterResult.Cache;
                }

                var searchSpan = span.Slice(0, endIndex);

                this.m_startCount += GetIndexCount(searchSpan, startSpan);
                this.m_endCount++;

                index += (endIndex + endSpan.Length);

                if (this.m_startCount == this.m_endCount)
                {
                    var ss = stringSpan.Slice(0, index).ToString(Encoding.UTF8);
                    this.m_startCount = 0;
                    this.m_endCount = 0;

                    var memory = byteBlock.Memory.Slice(byteBlock.Position, index);
                    var r = myEnum == JsonPackageKind.Object ? memory.Span.IndexOf(this.m_openBrace) : memory.Span.IndexOf(this.m_leftSquareBracket);
                    var dataMemory = memory.Slice(r);
                    var impurityMemory = memory.Slice(0, r);

                    request = new JsonPackage(myEnum, this.Encoding, dataMemory, impurityMemory);
                    byteBlock.Position += index;
                    return FilterResult.Success;
                }
            }
        }

        private static int GetIndexCount(ReadOnlySpan<byte> span, ReadOnlySpan<byte> searchSpan)
        {
            var count = 0;
            while (true)
            {
                var index = span.IndexOf(searchSpan);
                if (index < 0)
                {
                    return count;
                }

                count++;
                span = span.Slice(index + searchSpan.Length);
            }
        }

        private JsonPackageKind GetJsonPackageKind(ReadOnlySpan<byte> span)
        {
            var openBraceIndex = span.IndexOf(m_openBrace);
            var leftSquareBracketIndex = span.IndexOf(m_leftSquareBracket);

            if (openBraceIndex < 0)
            {
                if (leftSquareBracketIndex < 0)
                {
                    return JsonPackageKind.None;
                }

                return JsonPackageKind.Array;
            }
            else
            {
                if (leftSquareBracketIndex < 0)
                {
                    return JsonPackageKind.Object;
                }
                else if (openBraceIndex < leftSquareBracketIndex)
                {
                    return JsonPackageKind.Object;
                }
                else
                {
                    return JsonPackageKind.Array;
                }

            }
        }
    }
}