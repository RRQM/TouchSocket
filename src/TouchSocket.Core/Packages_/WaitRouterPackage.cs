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
/// 可等待的路由包。
/// </summary>
public class WaitRouterPackage : MsgRouterPackage, IWaitResult
{
    /// <inheritdoc/>
    public int Sign { get; set; }

    /// <inheritdoc/>
    public byte Status { get; set; }

    /// <summary>
    /// 是否将<see cref="Sign"/>和<see cref="Status"/>等参数放置在Router中。
    /// </summary>
    protected virtual bool IncludedRouter { get; }

    /// <inheritdoc/>
    public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.PackageBody(ref byteBlock);
        if (this.IncludedRouter)
        {
            return;
        }

        byteBlock.WriteInt32(this.Sign);
        byteBlock.WriteByte(this.Status);
    }

    /// <inheritdoc/>
    public override void PackageRouter<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.PackageRouter(ref byteBlock);
        if (!this.IncludedRouter)
        {
            return;
        }

        byteBlock.WriteInt32(this.Sign);
        byteBlock.WriteByte(this.Status);
    }

    /// <inheritdoc/>
    public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.UnpackageBody(ref byteBlock);
        if (!this.IncludedRouter)
        {
            this.Sign = byteBlock.ReadInt32();
            this.Status = byteBlock.ReadByte();
        }
    }

    /// <inheritdoc/>
    public override void UnpackageRouter<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.UnpackageRouter(ref byteBlock);
        if (this.IncludedRouter)
        {
            this.Sign = byteBlock.ReadInt32();
            this.Status = byteBlock.ReadByte();
        }
    }
}