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
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// TargetDmtpRpcActor
/// </summary>
internal class TargetDmtpRpcActor : IDmtpRpcActor
{
    private readonly IDmtpRpcActor m_rpcActor;
    private readonly string m_targetId;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="targetId">目标标识符</param>
    /// <param name="rpcActor">远程过程调用（RPC）行为接口</param>
    public TargetDmtpRpcActor(string targetId, IDmtpRpcActor rpcActor)
    {
        this.m_targetId = targetId; // 初始化目标标识符
        this.m_rpcActor = rpcActor; // 初始化RPC行为接口
    }

    public IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext> Dispatcher => this.m_rpcActor.Dispatcher;

    public IDmtpActor DmtpActor => this.m_rpcActor.DmtpActor;

    public bool DisposedValue => this.m_rpcActor.DisposedValue;

    public void Dispose()
    {
        this.m_rpcActor.Dispose();
    }

    public Task<bool> InputReceivedData(DmtpMessage message)
    {
        return this.m_rpcActor.InputReceivedData(message);
    }

    public Task<object> InvokeAsync(string invokeKey, Type returnType, InvokeOption invokeOption, params object[] parameters)
    {
        return this.m_rpcActor.InvokeAsync(this.m_targetId, invokeKey, returnType, invokeOption, parameters);
    }

    public Task<object> InvokeAsync(string targetId, string invokeKey, Type returnType, InvokeOption invokeOption, params object[] parameters)
    {
        return this.m_rpcActor.InvokeAsync(targetId, invokeKey, returnType, invokeOption, parameters);
    }
}