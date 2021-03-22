using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 若汝棋茗内置TCP验证服务器
    /// </summary>
    public class RRQMTokenTcpService<T> : TokenTcpService<T> where T : TcpSocketClient,new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMTokenTcpService() : this(new BytePool(1024 * 1024 * 1000, 1024 * 1024 * 20))
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytePool">内存池实例</param>
        public RRQMTokenTcpService(BytePool bytePool) : base(bytePool)
        {

        }
        /// <summary>
        /// 创建泛型T时
        /// </summary>
        public event Action<T, bool> CreatSocketCliect;

        /// <summary>
        /// 重写函数
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="newCreat"></param>
        protected override void OnCreatSocketCliect(T tcpSocketClient, bool newCreat)
        {
            CreatSocketCliect?.Invoke(tcpSocketClient,newCreat);
        }
    }
}
