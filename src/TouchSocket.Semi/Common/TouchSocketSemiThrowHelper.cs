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

namespace TouchSocket.Semi;

/// <summary>
/// 用于抛出 TouchSocket.Semi 相关异常的辅助类。
/// </summary>
public static class TouchSocketSemiThrowHelper
{
    /// <summary>
    /// 当 <see cref="SelectStatus"/> 不为 <see cref="SelectStatus.Success"/> 时抛出 <see cref="HsmsException"/>。
    /// </summary>
    /// <param name="status">要判断的选择状态。</param>
    /// <exception cref="HsmsException">当状态非成功时抛出此异常。</exception>
    public static void ThrowIfNotSuccess(SelectStatus status)
    {
        if (status != SelectStatus.Success)
        {
            throw new HsmsException($"HSMS Select 失败，状态码：{status}（{(byte)status}）。");
        }
    }
}
