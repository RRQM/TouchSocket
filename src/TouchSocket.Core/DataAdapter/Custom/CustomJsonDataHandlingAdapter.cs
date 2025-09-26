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

using System.Buffers;

namespace TouchSocket.Core;

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
/// 自定义 JSON 数据处理适配器。
/// </summary>
/// <typeparam name="TJsonRequestInfo">请求信息类型。</typeparam>
public abstract class CustomJsonDataHandlingAdapter<TJsonRequestInfo> : CustomDataHandlingAdapter<TJsonRequestInfo> where TJsonRequestInfo : IRequestInfo
{
    private readonly byte[] m_closeBrace;

    private readonly byte[] m_leftSquareBracket;

    private readonly byte[] m_openBrace;

    private readonly byte[] m_rightSquareBracket;

    private int m_endCount = 0;

    private int m_startCount = 0;

    /// <summary>
    /// 初始化 <see cref="CustomJsonDataHandlingAdapter{TJsonRequestInfo}"/> 类的新实例。
    /// </summary>
    /// <param name="encoding">编码格式。</param>
    public CustomJsonDataHandlingAdapter(Encoding encoding)
    {
        this.m_openBrace = encoding.GetBytes("{");
        this.m_closeBrace = encoding.GetBytes("}");
        this.m_leftSquareBracket = encoding.GetBytes("[");
        this.m_rightSquareBracket = encoding.GetBytes("]");
        this.Encoding = encoding;
    }

    /// <summary>
    /// 获取编码格式。
    /// </summary>
    public Encoding Encoding { get; }

    /// <inheritdoc/>
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TJsonRequestInfo request)
    {
        var stringSpan = byteBlock.Sequence;

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

            index += (int)(endIndex + endSpan.Length);

            if (this.m_startCount == this.m_endCount)
            {
                //var ss = stringSpan.Slice(0, index).ToString(Encoding.UTF8);
                this.m_startCount = 0;
                this.m_endCount = 0;

                var memory = byteBlock.GetMemory(index);
                var r = myEnum == JsonPackageKind.Object ? memory.Span.IndexOf(this.m_openBrace) : memory.Span.IndexOf(this.m_leftSquareBracket);
                var dataMemory = memory.Slice(r);
                var impurityMemory = memory.Slice(0, r);

                request = this.GetInstance(myEnum, this.Encoding, dataMemory, impurityMemory);
                byteBlock.Advance(index);
                return FilterResult.Success;
            }
        }
    }

    ///<summary>
    /// 获取实例。
    /// </summary>
    /// <param name="packageKind">包类型。</param>
    /// <param name="encoding">编码格式。</param>
    /// <param name="dataMemory">数据内存。</param>
    /// <param name="impurityMemory">杂质内存。</param>
    /// <returns>请求信息实例。</returns>
    protected abstract TJsonRequestInfo GetInstance(JsonPackageKind packageKind, Encoding encoding, ReadOnlyMemory<byte> dataMemory, ReadOnlyMemory<byte> impurityMemory);

    private static int GetIndexCount(ReadOnlySequence<byte> sequence, ReadOnlySpan<byte> searchSpan)
    {
        var count = 0;
        while (true)
        {
            var index = sequence.IndexOf(searchSpan);
            if (index < 0)
            {
                return count;
            }

            count++;
            sequence = sequence.Slice(index + searchSpan.Length);
        }
    }

    private JsonPackageKind GetJsonPackageKind(ReadOnlySequence<byte> sequence)
    {
        var openBraceIndex = sequence.IndexOf(this.m_openBrace);
        var leftSquareBracketIndex = sequence.IndexOf(this.m_leftSquareBracket);

        if (openBraceIndex < 0)
        {
            return leftSquareBracketIndex < 0 ? JsonPackageKind.None : JsonPackageKind.Array;
        }
        else
        {
            if (leftSquareBracketIndex < 0)
            {
                return JsonPackageKind.Object;
            }
            else
            {
                return openBraceIndex < leftSquareBracketIndex ? JsonPackageKind.Object : JsonPackageKind.Array;
            }
        }
    }
}