using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    class SimpleDmtpRpcActor : ISimpleDmtpRpcActor
    {
        private ushort m_invoke_Request;
        private ushort m_invoke_Response;

        public IDmtpActor DmtpActor { get; private set; }
        public Func<string, MethodModel> TryFindMethod { get; set; }

        public SimpleDmtpRpcActor(IDmtpActor dmtpActor)
        {
            this.DmtpActor = dmtpActor;
        }

        /// <summary>
        /// 设置处理协议标识的起始标识。
        /// </summary>
        /// <param name="start"></param>
        public void SetProtocolFlags(ushort start)
        {
            this.m_invoke_Request = start++;
            this.m_invoke_Response = start++;
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
                    package.Status = 3;
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
                    package.Status = 4;
                    package.Message = ex.Message;
                    package.SwitchId();
                    package.Package(byteBlock);
                    this.DmtpActor.Send(this.m_invoke_Response, byteBlock);
                    return;
                }
            }
        }

        private bool PrivateInvoke(string id, string methodName)
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
                        return true;
                    case WaitDataStatus.Overtime:
                        throw new TimeoutException();
                    case WaitDataStatus.Canceled:
                        break;
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception("未知异常");
                }
                return false;
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }
    }
}
