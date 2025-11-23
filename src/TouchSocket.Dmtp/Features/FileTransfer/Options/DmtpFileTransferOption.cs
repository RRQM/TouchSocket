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

/// <summary>
/// Dmtp文件传输配置选项
/// </summary>
public class DmtpFileTransferOption : DmtpFeatureOption
{
    public DmtpFileTransferOption()
    {
        this.StartProtocol = 30;
    }
    /// <summary>
    /// 文件资源控制器
    /// </summary>
    public IFileResourceController FileResourceController { get; set; } = TouchSocket.Dmtp.FileTransfer.FileResourceController.Default;

    /// <summary>
    /// 小文件最大长度
    /// </summary>
    public int MaxSmallFileLength { get; set; } = 1024 * 1024;

    /// <summary>
    /// 根路径
    /// </summary>
    public string RootPath { get; set; }
}