//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
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
        public event Action<T, CreatOption> CreatSocketCliect;

        /// <summary>
        /// 重写函数
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        /// <param name="creatOption"></param>
        protected override void OnCreatSocketCliect(T tcpSocketClient, CreatOption creatOption)
        {
            CreatSocketCliect?.Invoke(tcpSocketClient,creatOption);
        }
    }
}
