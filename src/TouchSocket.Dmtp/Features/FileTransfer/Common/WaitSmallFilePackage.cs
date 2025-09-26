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

namespace TouchSocket.Dmtp.FileTransfer;

internal class WaitSmallFilePackage : WaitRouterPackage
{
    protected override bool IncludedRouter => true;
    public byte[] Data { get; set; }
    public RemoteFileInfo FileInfo { get; set; }
    public int Len { get; set; }
    public Metadata Metadata { get; set; }
    public string Path { get; set; }

    public override void PackageBody<TWriter>(ref TWriter writer)
    {
        base.PackageBody(ref writer);
        WriterExtension.WriteString(ref writer, this.Path);
        if (this.Metadata is null)
        {
            WriterExtension.WriteNull(ref writer);
        }
        else
        {
            WriterExtension.WriteNotNull(ref writer);
            this.Metadata.Package(ref writer);
        }

        if (this.FileInfo is null)
        {
            WriterExtension.WriteNull(ref writer);
        }
        else
        {
            WriterExtension.WriteNotNull(ref writer);
            this.FileInfo.Package(ref writer);
        }

        WriterExtension.WriteByteSpan(ref writer, new System.ReadOnlySpan<byte>(this.Data, 0, this.Len));
    }

    public override void UnpackageBody<TReader>(ref TReader reader)
    {
        base.UnpackageBody(ref reader);
        this.Path = ReaderExtension.ReadString(ref reader);
        if (ReaderExtension.ReadIsNull(ref reader))
        {
            this.Metadata = null;
        }
        else
        {
            this.Metadata = new Metadata();
            this.Metadata.Unpackage(ref reader);
        }

        if (ReaderExtension.ReadIsNull(ref reader))
        {
            this.FileInfo = null;
        }
        else
        {
            this.FileInfo = new RemoteFileInfo();
            this.FileInfo.Unpackage(ref reader);
        }
        this.Data = ReaderExtension.ReadByteSpan(ref reader).ToArray();
    }
}