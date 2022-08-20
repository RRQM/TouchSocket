using TouchSocket.Core.Dependency;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcServerFactory
    /// </summary>
    public class RpcServerFactory : IRpcServerFactory
    {
        private readonly IContainer m_container;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="container"></param>
        public RpcServerFactory(IContainer container)
        {
            this.m_container = container;
        }

        IRpcServer IRpcServerFactory.Create(ICallContext callContext, object[] ps)
        {
            return (IRpcServer)this.m_container.Resolve(callContext.MethodInstance.ServerType);
        }
    }
}
