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
namespace RRQMSocket.RPC
{
    /*
    若汝棋茗
    */

    /// <summary>
    /// RPC范围类
    /// </summary>
    public abstract class ServerProvider
    {
        /// <summary>
        /// 该服务所属的服务器
        /// </summary>
        public RPCService RPCService { get; internal set; }

        /// <summary>
        /// 通过IDToken获得实例
        /// </summary>
        /// <param name="iDToken"></param>
        /// <returns></returns>
        protected ISocketClient GetSocketClient(string iDToken)
        {
            if (this.RPCService == null)
            {
                throw new RRQMRPCException("该实例并非服务器托管实例，获取对象为空");
            }
            return this.RPCService.GetTcpSocketClient(iDToken);
        }

        /// <summary>
        /// RPC即将进入时
        /// </summary>
        /// <param name="methodItem"></param>
        protected internal virtual void RPCEnter(MethodItem methodItem)
        {
        }

        /// <summary>
        /// 执行RPC发生错误
        /// </summary>
        /// <param name="methodItem"></param>
        protected internal virtual void RPCError(MethodItem methodItem)
        {
        }

        /// <summary>
        /// RPC方法执行完成
        /// </summary>
        /// <param name="methodItem"></param>
        protected internal virtual void RPCLeave(MethodItem methodItem)
        {
        }
    }
}