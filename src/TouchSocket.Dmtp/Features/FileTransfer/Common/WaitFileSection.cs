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

using System;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer;

internal class WaitFileSection : WaitRouterPackage, IDisposable
{
    public FileSection FileSection { get; set; }
    public ByteBlock Value { get; set; }

    public void Dispose()
    {
        this.Value?.Dispose();
    }

    public override void PackageBody<TWriter>(ref TWriter writer)
    {
        base.PackageBody(ref writer);
        if (this.FileSection is null)
        {
            WriterExtension.WriteNull(ref writer);
        }
        else
        {
             WriterExtension.WriteNotNull(ref writer);
            this.FileSection.Package(ref writer);
        }
        WriterExtension.WriteByteBlock(ref writer,this.Value);
    }

    public override void UnpackageBody<TReader>(ref TReader reader)
    {
        base.UnpackageBody(ref reader);
        if (ReaderExtension.ReadIsNull(ref reader))
        {
            this.FileSection = null;
        }
        else
        {
            this.FileSection = new FileSection();
            this.FileSection.Unpackage(ref reader);
        }
        this.Value = ReaderExtension.ReadByteBlock(ref reader);
    }
}