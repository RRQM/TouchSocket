using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    internal class SimpleDmtpRpcActor : ISimpleDmtpRpcActor
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
                    rpcPackage.UnpackageRouter(ref byteBlock);
                    if (rpcPackage.Route && this.DmtpActor.AllowRoute)
                    {
                        if (await this.DmtpActor.TryRouteAsync(new PackageRouterEventArgs(new RouteType("SimpleRpc"), rpcPackage)))
                        {
                            if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId) is DmtpActor actor)
                            {
                                await actor.SendAsync(this.m_invoke_Request, byteBlock.Memory);
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

                        rpcPackage.Package(ref byteBlock);
                        await this.DmtpActor.SendAsync(this.m_invoke_Response, byteBlock.Memory);
                    }
                    else
                    {
                        rpcPackage.UnpackageBody(ref byteBlock);
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
                    rpcPackage.UnpackageRouter(ref byteBlock);
                    if (this.DmtpActor.AllowRoute && rpcPackage.Route)
                    {
                        if (await this.DmtpActor.TryFindDmtpActor(rpcPackage.TargetId) is DmtpActor actor)
                        {
                            await actor.SendAsync(this.m_invoke_Response, byteBlock.Memory);
                        }
                    }
                    else
                    {
                        rpcPackage.UnpackageBody(ref byteBlock);
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

        private async void InvokeThis(object obj)
        {
            var package = (SimpleDmtpRpcPackage)obj;

            var methodModel = this.TryFindMethod.Invoke(package.MethodName);
            if (methodModel == null)
            {
                var byteBlock = new ByteBlock();
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
                var byteBlock = new ByteBlock();
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
                var byteBlock = new ByteBlock();
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

        private async void PrivateInvoke(string id, string methodName)
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
                var byteBlock = new ByteBlock();
                try
                {
                    package.Package(ref byteBlock);
                    await this.DmtpActor.SendAsync(this.m_invoke_Request, byteBlock.Memory);
                }
                finally
                {
                    byteBlock.Dispose();
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