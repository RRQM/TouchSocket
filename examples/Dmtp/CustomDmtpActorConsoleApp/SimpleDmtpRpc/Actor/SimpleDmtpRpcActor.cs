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

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc;

internal class SimpleDmtpRpcActor : DisposableObject, ISimpleDmtpRpcActor
{
    private readonly ushort m_invoke_Request = 1000;
    private readonly ushort m_invoke_Response = 1001;

    public IDmtpActor DmtpActor { get; private set; }
    public Func<string, MethodModel> TryFindMethod { get; set; }

    public SimpleDmtpRpcActor(IDmtpActor dmtpActor)
    {
        this.DmtpActor = dmtpActor;
    }

    public async Task<bool> InputReceivedData(DmtpMessage message)
    {
        var memory = message.Memory;
        BytesReader reader = new BytesReader(memory);
        if (message.ProtocolFlags == this.m_invoke_Request)
        {
            try
            {
                var rpcPackage = new SimpleDmtpRpcPackage();
                rpcPackage.UnpackageRouter(ref reader);
                if (rpcPackage.Route && this.DmtpActor.AllowRoute)
                {
                    if (await this.DmtpActor.TryRouteAsync(new PackageRouterEventArgs(new RouteType("SimpleRpc"), rpcPackage)))
                    {
                        if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId) is DmtpActor actor)
                        {
                            await actor.SendAsync(this.m_invoke_Request, memory);
                            return true;
                        }
                        else
                        {
                            rpcPackage.Status = 2;
                        }
                    }
                    else
                    {
                        rpcPackage.Status = 3;
                    }

                    rpcPackage.SwitchId();

                    ByteBlock byteBlock = new ByteBlock(1024);
                    try
                    {
                        rpcPackage.Package(ref byteBlock);
                        await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory);
                    }
                    finally
                    {
                        byteBlock.Dispose();
                    }

                }
                else
                {
                    rpcPackage.UnpackageBody(ref reader);
                    _ = Task.Factory.StartNew(this.InvokeThis, rpcPackage);
                }
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
            }
            return true;
        }
        else if (message.ProtocolFlags == this.m_invoke_Response)
        {
            try
            {
                var rpcPackage = new SimpleDmtpRpcPackage();
                rpcPackage.UnpackageRouter(ref reader);
                if (this.DmtpActor.AllowRoute && rpcPackage.Route)
                {
                    if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId) is DmtpActor actor)
                    {
                        await actor.SendAsync(this.m_invoke_Response, memory);
                    }
                }
                else
                {
                    rpcPackage.UnpackageBody(ref reader);
                    this.DmtpActor.WaitHandlePool.Set(rpcPackage);
                }
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
            }
            return true;
        }
        return false;
    }

    private async void InvokeThis(object obj)
    {
        var package = (SimpleDmtpRpcPackage)obj;

        var methodModel = this.TryFindMethod.Invoke(package.MethodName);
        if (methodModel == null)
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                package.Status = 4;
                package.SwitchId();
                package.Package(ref byteBlock);
                await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory);
                return;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        try
        {
            methodModel.Method.Invoke(methodModel.Target, default);
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                package.Status = 1;
                package.SwitchId();
                package.Package(ref byteBlock);
                await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory);
                return;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
        catch (Exception ex)
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                package.Status = 5;
                package.Message = ex.Message;
                package.SwitchId();
                package.Package(ref byteBlock);
                await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory);
                return;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }

    private async Task<SimpleDmtpRpcActor> TryFindDmtpRpcActor(string targetId)
    {
        if (targetId == this.DmtpActor.Id)
        {
            return this;
        }
        if (await this.DmtpActor.TryFindDmtpActor(targetId) is DmtpActor dmtpActor)
        {
            if (dmtpActor.GetSimpleDmtpRpcActor() is SimpleDmtpRpcActor newActor)
            {
                return newActor;
            }
        }
        return default;
    }

    public async Task Invoke(string methodName, CancellationToken token)
    {
        await this.PrivateInvoke(default, methodName, token);
    }

    public async Task Invoke(string targetId, string methodName, CancellationToken token)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        }

        if (string.IsNullOrEmpty(methodName))
        {
            throw new ArgumentException($"“{nameof(methodName)}”不能为 null 或空。", nameof(methodName));
        }

        if (this.DmtpActor.AllowRoute && await this.TryFindDmtpRpcActor(targetId) is SimpleDmtpRpcActor actor)
        {
            await actor.Invoke(methodName, token);
            return;
        }

        await this.PrivateInvoke(targetId, methodName, token);
    }

    private async Task PrivateInvoke(string id, string methodName, CancellationToken token)
    {
        var package = new SimpleDmtpRpcPackage()
        {
            MethodName = methodName,
            SourceId = this.DmtpActor.Id,
            TargetId = id
        };

        var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(package);

        try
        {
            var byteBlock = new ByteBlock(1024 * 64);
            try
            {
                package.Package(ref byteBlock);
                await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory);
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (await waitData.WaitAsync(token))
            {
                case WaitDataStatus.Success:
                    var result = (SimpleDmtpRpcPackage)waitData.CompletedData;
                    result.CheckStatus();
                    return;

                case WaitDataStatus.Overtime:
                    throw new TimeoutException();
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Default:
                case WaitDataStatus.Disposed:
                default:
                    throw new Exception("未知异常");
            }
        }
        finally
        {
            waitData.Dispose();
        }
    }
}