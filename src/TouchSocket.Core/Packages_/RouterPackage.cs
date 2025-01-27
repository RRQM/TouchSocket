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
    /// 打包所有的路由包信息。顺序为：先调用<see cref="PackageRouter{TByteBlock}(ref TByteBlock)"/>，然后<see cref="PackageBody{TByteBlock}(ref TByteBlock)"/>
    /// </summary>
    /// <param name="byteBlock"></param>
    public sealed override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.PackageRouter(ref byteBlock);
        this.PackageBody(ref byteBlock);
    }

    /// <summary>
    /// 打包数据体。一般不需要单独调用该方法。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    /// <param name="byteBlock"></param>
    public virtual void PackageBody<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
    }

    /// <summary>
    /// 打包路由。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    public virtual void PackageRouter<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.WriteString(this.SourceId, FixedHeaderType.Byte);
        byteBlock.WriteString(this.TargetId, FixedHeaderType.Byte);
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
    public sealed override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.UnpackageRouter(ref byteBlock);
        this.UnpackageBody(ref byteBlock);
    }

    /// <summary>
    /// 解包数据体。一般不需要单独调用该方法。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    /// <param name="byteBlock"></param>
    public virtual void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
    }

    /// <summary>
    /// 只解包路由部分。一般不需要单独调用该方法。
    /// <para>重写的话，约定基类方法必须先执行</para>
    /// </summary>
    /// <param name="byteBlock"></param>
    public virtual void UnpackageRouter<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        this.SourceId = byteBlock.ReadString(FixedHeaderType.Byte);
        this.TargetId = byteBlock.ReadString(FixedHeaderType.Byte);
    }
}