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
using System;
using System.Diagnostics;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 流量控制
    /// </summary>
    public class FlowGate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FlowGate()
        {
            this.stopwatch = new Stopwatch();
        }

        private long maximum;

        /// <summary>
        /// 最大值
        /// </summary>
        public long Maximum
        {
            get { return maximum; }
            set { maximum = value; }
        }

        long timeTick;

        Stopwatch stopwatch;
        long transferLength;

        /// <summary>
        /// 添加增量
        /// </summary>
        /// <param name="increment"></param>
        public void AddLength(int increment)
        {
            this.transferLength += increment;
        }

        /// <summary>
        /// 检测等待
        /// </summary>
        public void CheckWait()
        {
            if (this.GetNowTick() - timeTick > 0)
            {
                //时间过了一秒
                this.timeTick = GetNowTick();
                transferLength = 0;
                stopwatch.Restart();
            }
            else
            {
                //在这一秒中
                if (transferLength > this.maximum)
                {
                    //上传饱和
                    stopwatch.Stop();
                    int sleepTime = 1000 - (int)stopwatch.ElapsedMilliseconds <= 0 ? 0 : 1000 - (int)stopwatch.ElapsedMilliseconds;
                    Thread.Sleep(sleepTime);
                }
            }
        }

        /// <summary>
        /// 获取当前时间帧
        /// </summary>
        /// <returns></returns>
        private long GetNowTick()
        {
            return DateTime.Now.Ticks / 10000000;
        }
    }
}
