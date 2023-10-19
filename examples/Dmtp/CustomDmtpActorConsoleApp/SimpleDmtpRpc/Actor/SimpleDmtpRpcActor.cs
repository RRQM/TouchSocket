using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    class SimpleDmtpRpcActor : ISimpleDmtpRpcActor
    {
        private ushort m_invoke_Request = 1000;
        private ushort m_invoke_Response = 1001;

        public IDmtpActor DmtpActor { get; private set; }
        public Func<string, MethodModel> TryFindMethod { get; set; }

        public SimpleDmtpRpcActor(IDmtpActor dmtpActor)
        {
            this.DmtpActor = dmtpActor;
        }

        public async Task<bool> InputReceivedData(DmtpMessage message)
        {
            var byteBlock = message.BodyByteBlock;
            if (message.ProtocolFlags == this.m_invoke_Request)
            {
                try
                {
                    var rpcPackage = new SimpleDmtpRpcPackage();
                    rpcPackage.UnpackageRouter(byteBlock);
                    if (rpcPackage.Route && this.DmtpActor.AllowRoute)
                    {
                        if (await this.DmtpActor.TryRoute(new PackageRouterEventArgs(new RouteType("SimpleRpc"), rpcPackage)))
                        {
                            if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId) is DmtpActor actor)
                            {
                                actor.Send(this.m_invoke_Request, byteBlock);
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

                        byteBlock.Reset();
                        rpcPackage.SwitchId();

                        rpcPackage.Package(byteBlock);
                        this.DmtpActor.Send(this.m_invoke_Response, byteBlock);
                    }
                    else
                    {
                        rpcPackage.UnpackageBody(byteBlock);
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
                    rpcPackage.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && rpcPackage.Route)
                    {
                        if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId) is DmtpActor actor)
                        {
                            actor.Send(this.m_invoke_Response, byteBlock);
                        }
                    }
                    else
                    {
                        rpcPackage.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(rpcPackage);
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

        private void InvokeThis(object obj)
        {
            var package = (SimpleDmtpRpcPackage)obj;

            var methodModel = this.TryFindMethod.Invoke(package.MethodName);
            if (methodModel == null)
            {
                using (var byteBlock = new ByteBlock())
                {
                    package.Status = 4;
                    package.SwitchId();
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_invoke_Response, byteBlock);
                    return;
                }
            }

            try
            {
                methodModel.Method.Invoke(methodModel.Target, default);
                using (var byteBlock = new ByteBlock())
                {
                    package.Status = 1;
                    package.SwitchId();
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_invoke_Response, byteBlock);
                    return;
                }
            }
            catch (Exception ex)
            {
                using (var byteBlock = new ByteBlock())
                {
                    package.Status = 5;
                    package.Message = ex.Message;
                    package.SwitchId();
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_invoke_Response, byteBlock);
                    return;
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

        public void Invoke(string methodName)
        {
            this.PrivateInvoke(default, methodName);
        }
        public async void Invoke(string targetId, string methodName)
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
                actor.Invoke(methodName);
                return;
            }

            this.PrivateInvoke(targetId, methodName);
        }

        private void PrivateInvoke(string id, string methodName)
        {
            var package = new SimpleDmtpRpcPackage()
            {
                MethodName = methodName,
                Route = id.HasValue(),
                SourceId = this.DmtpActor.Id,
                TargetId = id
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetReverseWaitData(package);

            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_invoke_Request, byteBlock);
                }
                switch (waitData.Wait(5000))
                {
                    case WaitDataStatus.SetRunning:
                        var result = (SimpleDmtpRpcPackage)waitData.WaitResult;
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
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }
    }
}
