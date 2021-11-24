using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
