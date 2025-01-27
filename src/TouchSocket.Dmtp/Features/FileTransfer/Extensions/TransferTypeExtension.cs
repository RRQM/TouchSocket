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
/// 提供有关传输类型是否为拉取类型的扩展方法。
/// </summary>
public static class TransferTypeExtension
{
    /// <summary>
    /// 表示当前传输类型是否属于<see cref="TransferType.Pull"/>、<see cref="TransferType.SmallPull"/>其中的一种。
    /// </summary>
    /// <param name="transferType">要检查的传输类型。</param>
    /// <returns>如果传输类型是<see cref="TransferType.Pull"/>或<see cref="TransferType.SmallPull"/>，则返回true；否则返回false。</returns>
    public static bool IsPull(this TransferType transferType)
    {
        return transferType == TransferType.Pull || transferType == TransferType.SmallPull;
    }
}