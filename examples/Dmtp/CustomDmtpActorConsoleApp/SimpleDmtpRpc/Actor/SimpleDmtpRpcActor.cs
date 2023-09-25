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
        private ushort m_invoke_Request=1000;
        private ushort m_invoke_Response=1001;

        public IDmtpActor DmtpActor { get; private set; }
        public Func<string, MethodModel> TryFindMethod { get; set; }

        public SimpleDmtpRpcActor(IDmtpActor dmtpActor)
        {
            this.DmtpActor = dmtpActor;
        }

        public bool InputReceivedData(DmtpMessage message)
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
                        if (this.DmtpActor.TryRoute(new RouteType("SimpleRpc"), rpcPackage))
                        {
                            if (this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId, out var actor))
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
                        Task.Factory.StartNew(this.InvokeThis, rpcPackage);
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
                        if (this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId, out var actor))
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
                using (var byteBlock=new ByteBlock())
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
                methodModel.Method.Invoke(methodModel.Target,default);
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

        private bool TryFindDmtpRpcActor(string targetId, out SimpleDmtpRpcActor rpcActor)
        {
            if (targetId == this.DmtpActor.Id)
            {
                rpcActor = this;
                return true;
            }
            if (this.DmtpActor.TryFindDmtpActor(targetId, out var smtpActor))
            {
                if (smtpActor.GetSimpleDmtpRpcActor() is SimpleDmtpRpcActor newActor)
                {
                    rpcActor = newActor;
                    return true;
                }
            }

            rpcActor = default;
            return false;
        }

        public void Invoke(string methodName)
        {
            this.PrivateInvoke(default, methodName);
        }
        public void Invoke(string targetId, string methodName)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"“{nameof(methodName)}”不能为 null 或空。", nameof(methodName));
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpRpcActor(targetId, out var actor))
            {
                actor.Invoke(methodName);
                return;
            }

            this.PrivateInvoke(targetId,methodName);
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
