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

using System.Net.WebSockets;

namespace TouchSocket.Http.WebSockets;

public readonly struct WebSocketReceiveResult:IDisposable
{
    private readonly ReadLease<WSDataFrame> m_readLease;

    public WebSocketReceiveResult(ReadLease<WSDataFrame> readLease, WSDataFrame dataFrame, string message)
    {
        this.m_readLease = readLease;
        this.DataFrame = dataFrame;
        this.Message = message;
    }

    public WSDataFrame DataFrame { get; }


    public bool IsCompleted => this.m_readLease.IsCompleted;


    public string Message { get; }

    public void Dispose()
    {
        this.m_readLease.Dispose();
    }
}
