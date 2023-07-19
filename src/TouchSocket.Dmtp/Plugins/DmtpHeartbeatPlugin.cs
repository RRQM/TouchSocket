//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 基于Dmtp的心跳插件。服务器和客户端均适用
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = true)]
    public class DmtpHeartbeatPlugin<TClient> : PluginBase, IDmtpHandshakedPlugin<TClient> where TClient : IDmtpActorObject
    {
        /// <summary>
        /// 最大失败次数，默认3。
        /// </summary>
        public int MaxFailCount { get; set; } = 3;

        /// <summary>
        /// 心跳间隔。默认3秒。
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(3);


        /// <summary>
        /// 最大失败次数，默认3。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public DmtpHeartbeatPlugin<TClient> SetMaxFailCount(int value)
        {
            this.MaxFailCount = value;
            return this;
        }

        /// <summary>
        /// 心跳间隔。默认3秒。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public DmtpHeartbeatPlugin<TClient> SetTick(TimeSpan value)
        {
            this.Tick = value;
            return this;
        }

        Task IDmtpHandshakedPlugin<TClient>.OnDmtpHandshaked(TClient client, DmtpVerifyEventArgs e)
        {
            Task.Run(async() => 
            {
                var failedCount = 0;
                while (true)
                {
                    await Task.Delay(this.Tick);
                    if (client.DmtpActor == null || !client.DmtpActor.IsHandshaked)
                    {
                        return;
                    }
                    if ((DateTime.Now - client.DmtpActor.LastActiveTime < this.Tick))
                    {
                        continue;
                    }

                    if (client.DmtpActor.Ping())
                    {
                        failedCount = 0;
                    }
                    else
                    {
                        failedCount++;
                        if (failedCount > this.MaxFailCount)
                        {
                            client.DmtpActor.Close(true, "自动心跳失败次数达到最大，已断开连接。");
                        }
                    }
                }
            });

            return e.InvokeNext();
        }
    }
}