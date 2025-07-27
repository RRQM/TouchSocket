// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace TouchSocket.Core;

public abstract class DataHandlingAdapterSlim<TRequest> : SafetyDisposableObject
     where TRequest : IRequestInfo
{

    /// <summary>
    /// 缓存超时时间。默认1秒。
    /// </summary>
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 是否启用缓存超时。默认<see langword="false"/>。
    /// </summary>
    public bool CacheTimeoutEnable { get; set; } = false;

    protected override void SafetyDispose(bool disposing)
    {

    }

    public abstract bool TryParseRequest<TReader>(ref TReader reader, out TRequest request)
        where TReader : IBytesReader;
}
