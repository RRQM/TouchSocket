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

namespace TouchSocket.Dmtp;

/// <summary>
/// 具有目标id和源id的路由包
/// </summary>
public class RouterPackage : PackageBase, IReadonlyRouterPackage
{
    /// <summary>
    /// 获取是否路由此包数据。实际上是判断<see cref="TargetId"/>与<see cref="SourceId"/>是否有值。
    /// </summary>
    public bool Route => this.TargetId.HasValue() && this.SourceId.HasValue();

    /// <summary>
    /// 源Id
    /// </summary>
    public string SourceId { get; set; }

    /// <summary>
    /// 目标Id
    /// </summary>
    public string TargetId { get; set; }

    /// <summary>
    /// 版本信息，用于后续按版本解析数据。
    /// </summary>
    public byte Version { get; set; } = 1;

    /// <summary>
    /// 元数据
    /// </summary>
    public Metadata Metadata { get; set; }

    /// <summary>
    /// 打包所有的路由包信息。顺序为：先调用<see cref="PackageRouter{TByteBlock}(ref TByteBlock)"/>，然后<see cref="PackageBody{TByteBlock}(ref TByteBlock)"/>
    /// </summary>
    public override sealed void Package<TWriter>(ref TWriter writer)
    {
        this.PackageRouter(ref writer);
        this.PackageBody(ref writer);
    }

    /// <summary>
    /// 打包数据体。一般不需要单独调用该方法。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    public virtual void PackageBody<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
    }

    /// <summary>
    /// 打包路由。包含版本、源Id、目标Id、元数据。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    public virtual void PackageRouter<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriterExtension.WriteValue<TWriter, byte>(ref writer, this.Version);
        WriterExtension.WriteString(ref writer, this.SourceId, FixedHeaderType.Byte);
        WriterExtension.WriteString(ref writer, this.TargetId, FixedHeaderType.Byte);
        if (this.Metadata is null)
        {
            WriterExtension.WriteNull(ref writer);
        }
        else
        {
            WriterExtension.WriteNotNull(ref writer);
            this.Metadata.Package(ref writer);
        }
    }

    /// <summary>
    /// 转换目标和源的id。
    /// </summary>
    public void SwitchId()
    {
        var value = this.SourceId;
        this.SourceId = this.TargetId;
        this.TargetId = value;
    }

    /// <inheritdoc/>
    public override sealed void Unpackage<TReader>(ref TReader reader)
    {
        this.UnpackageRouter(ref reader);
        this.UnpackageBody(ref reader);
    }

    /// <summary>
    /// 解包数据体。一般不需要单独调用该方法。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    public virtual void UnpackageBody<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
    }

    /// <summary>
    /// 只解包路由部分。一般不需要单独调用该方法。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    public virtual void UnpackageRouter<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        this.Version = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        this.SourceId = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
        this.TargetId = ReaderExtension.ReadString(ref reader, FixedHeaderType.Byte);
        if (ReaderExtension.ReadIsNull(ref reader))
        {
            this.Metadata = null;
        }
        else
        {
            this.Metadata = new Metadata();
            this.Metadata.Unpackage(ref reader);
        }
    }
}
