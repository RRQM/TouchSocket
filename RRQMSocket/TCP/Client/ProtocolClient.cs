using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端
    /// </summary>
    public abstract class ProtocolClient : TokenClient
    {
        /// <summary>
        /// 载入配置，协议客户端数据处理适配器不可更改。
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
        }

        /// <summary>
        /// 密封方法
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            int agreement = BitConverter.ToInt32(byteBlock.Buffer, 0);
            switch (agreement)
            {
                case 0:
                    {
                        break;
                    }
                default:
                    {
                        HandleProtocolData(agreement,byteBlock);
                             break;
                    }
                   
            }
        }

        /// <summary>
        /// 收到协议数据，由于性能考虑，
        /// byteBlock数据源并未剔除协议数据，
        /// 所以真实数据起点为4，
        /// 长度为Length-4。
        /// </summary>
        /// <param name="agreement"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleProtocolData(int agreement,ByteBlock byteBlock);
    }
}
