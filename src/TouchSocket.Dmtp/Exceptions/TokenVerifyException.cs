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
/// 表示在令牌验证过程中发生的异常。
/// </summary>
[Serializable]
public sealed class TokenVerifyException : Exception
{
    /// <summary>
    /// 获取与此异常关联的元数据。
    /// </summary>
    public Metadata Metadata { get; }

    /// <summary>
    /// 初始化 <see cref="TokenVerifyException"/> 类的新实例。
    /// </summary>
    /// <param name="metadata">与异常关联的元数据。</param>
    /// <param name="message">描述错误的消息。</param>
    public TokenVerifyException(Metadata metadata, string message) : base(message)
    {
        this.Metadata = metadata;
    }
}
