using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 若汝棋茗内置TCP验证客户端
    /// </summary>
    public class RRQMTokenTcpClient : TokenTcpClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMTokenTcpClient()
        { 
        
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool">设置内存池实例</param>
        public RRQMTokenTcpClient(BytePool bytePool):base(bytePool)
        { 
        
        }
        /// <summary>
        /// 当收到数据时
        /// </summary>
        public event RRQMByteBlockEventHandler OnReceivedData;

        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock)
        {
            OnReceivedData?.Invoke(this,byteBlock);
        }
    }
}
