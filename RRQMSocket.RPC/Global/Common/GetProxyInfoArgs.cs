using System.Collections.Generic;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 获取代理信息
    /// </summary>
    public class GetProxyInfoArgs
    {
        private string proxyToken;

        private RpcType rpcType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="rpcType"></param>
        public GetProxyInfoArgs(string proxyToken, RpcType rpcType)
        {
            this.proxyToken = proxyToken;
            this.rpcType = rpcType;
            this.codes = new List<ServerCellCode>();
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 代理令箭
        /// </summary>
        public string ProxyToken
        {
            get { return proxyToken; }
        }

        private List<ServerCellCode> codes;
        
        /// <summary>
        /// Rpc代理类
        /// </summary>
        public List<ServerCellCode> Codes
        {
            get { return codes; }
        }


        /// <summary>
        /// Rpc类型
        /// </summary>
        public RpcType RpcType
        {
            get { return rpcType; }
        }
    }
}