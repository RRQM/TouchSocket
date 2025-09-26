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

namespace TouchSocket.Core;

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
    /// 获取编码格式。
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    /// 获取杂质数据。
    /// </summary>
    public ReadOnlyMemory<byte> ImpurityData { get; }

    /// <summary>
    /// 获取 JSON 包的类型。
    /// </summary>
    public JsonPackageKind Kind { get; }
}

/// <summary>
/// 处理 JSON 包的适配器。
/// </summary>
public class JsonPackageAdapter : CustomJsonDataHandlingAdapter<JsonPackage>
{
    /// <summary>
    /// 初始化 <see cref="JsonPackageAdapter"/> 类的新实例。
    /// </summary>
    /// <param name="encoding">编码格式。</param>
    public JsonPackageAdapter(Encoding encoding) : base(encoding)
    {
    }

    /// <summary>
    /// 初始化 <see cref="JsonPackageAdapter"/> 类的新实例，使用 UTF-8 编码格式。
    /// </summary>
    public JsonPackageAdapter() : base(Encoding.UTF8)
    {
    }

    /// <inheritdoc/>
    protected override JsonPackage GetInstance(JsonPackageKind packageKind, Encoding encoding, ReadOnlyMemory<byte> dataMemory, ReadOnlyMemory<byte> impurityMemory)
    {
        return new JsonPackage(packageKind, encoding, dataMemory, impurityMemory);
    }
}