//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Exceptions;

namespace RRQMSocket
{
    /// <summary>
    /// 协议服务器
    /// </summary>
    public class ProtocolService<TClient> : TokenService<TClient> where TClient : ProtocolSocketClient, new()
    {
        private bool canResetID;

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        public override void ResetID(WaitSetID waitSetID)
        {
            if (!canResetID)
            {
                throw new RRQMException("服务器不允许修改ID");
            }
            base.ResetID(waitSetID);
            if (this.TryGetSocketClient(waitSetID.NewID, out TClient client))
            {
                waitSetID.Status = 1;
                client.ChangeID(waitSetID);
            }
            else
            {
                throw new RRQMException("新ID不可用，请清理客户端重新修改ID");
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected override void LoadConfig(ServiceConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.canResetID = (bool)serverConfig.GetValue(ProtocolServiceConfig.CanResetIDProperty);
        }
    }
}