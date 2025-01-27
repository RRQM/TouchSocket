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
/// WaitPackage
/// </summary>
public class WaitPackage : PackageBase, IWaitResult
{
    /// <inheritdoc/>
    public string Message { get; set; }

    /// <inheritdoc/>
    public int Sign { get; set; }

    /// <inheritdoc/>
    public byte Status { get; set; }

    /// <inheritdoc/>
    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteInt32(this.Sign);
        byteBlock.WriteByte(this.Status);
        byteBlock.WriteString(this.Message, FixedHeaderType.Ushort);
    }

    /// <inheritdoc/>
    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.Sign = byteBlock.ReadInt32();
        this.Status = byteBlock.ReadByte();
        this.Message = byteBlock.ReadString(FixedHeaderType.Ushort);
    }
}