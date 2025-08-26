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

using TouchSocket.Core;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc;

internal class SimpleDmtpRpcPackage : WaitRouterPackage
{
    protected override bool IncludedRouter => true;

    public string MethodName { get; set; }


    public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.PackageBody(ref byteBlock);
        WriterExtension.WriteString(ref byteBlock, this.MethodName);
    }

    public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.UnpackageBody(ref byteBlock);
        this.MethodName = ReaderExtension.ReadString<TByteBlock>(ref byteBlock);
    }

    public void CheckStatus()
    {
        switch (this.Status)
        {
            case 0:
                throw new TimeoutException();
            case 1: return;
            case 2: throw new Exception("没有找到目标Id");
            case 3: throw new Exception("不允许路由");
            case 4: throw new Exception("没找到Rpc");
            case 5: throw new Exception($"其他异常：{this.Message}");
        }
    }
}