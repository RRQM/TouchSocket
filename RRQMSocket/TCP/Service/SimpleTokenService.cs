using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 简单Token服务器
    /// </summary>
    public class SimpleTokenService : TokenService<SimpleSocketClient>
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public event Action<SimpleSocketClient, ByteBlock, object> Received;

        /// <summary>
        /// 成功连接后创建（或从对象池中获得）辅助类,
        /// 用户可以在该方法中再进行自定义设置，
        /// 但是如果该对象是从对象池获得的话，为避免重复设定某些值，
        /// 例如事件等，请先判断CreatOption.NewCreat值再做处理。
        /// </summary>
        public event Action<SimpleSocketClient, CreateOption> CreateSocketCliect;

        /// <summary>
        /// 成功连接后创建（或从对象池中获得）辅助类,
        /// 用户可以在该方法中再进行自定义设置，
        /// 但是如果该对象是从对象池获得的话，为避免重复设定某些值，
        /// 例如事件等，请先判断CreatOption.NewCreat值再做处理。
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="creatOption"></param>
        protected override void OnCreateSocketCliect(SimpleSocketClient tcpSocketClient, CreateOption creatOption)
        {
            this.CreateSocketCliect?.Invoke(tcpSocketClient, creatOption);
            if (creatOption.NewCreate)
            {
                tcpSocketClient.OnReceived = this.OnReceive;
            }
        }

        private void OnReceive(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            this.Received?.Invoke(socketClient, byteBlock, obj);
        }
    }
}
