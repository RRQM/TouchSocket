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
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 流操作
    /// </summary>
    public class StreamOperator
    {
        internal long completedLength;

        internal float progress;

        private long speed;

        internal long speedTemp;

        private CancellationTokenSource tokenSource;

        private int maxSpeed = 1024 * 1024;

        /// <summary>
        /// 最大传输速度（默认1024*1024字节）
        /// </summary>
        public int MaxSpeed
        {
            get { return maxSpeed; }
            set { maxSpeed = value; }
        }


        /// <summary>
        /// 已完成长度
        /// </summary>
        /// <returns></returns>
        public long CompletedLength { get => completedLength; }

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress
        {
            get { return progress; }
        }

        private int packageSize = 1024 * 64;

        /// <summary>
        /// 包长度，默认64Kb
        /// </summary>
        public int PackageSize
        {
            get { return packageSize; }
            set { packageSize = value; }
        }

        internal ChannelStatus status;
        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status
        {
            get { return status; }
        }


        /// <summary>
        /// 可取消令箭
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// 可取消令箭源
        /// </summary>
        public CancellationTokenSource TokenSource => this.tokenSource;

        /// <summary>
        /// 设置可取消令箭源
        /// </summary>
        /// <param name="tokenSource"></param>
        public void SetCancellationTokenSource(CancellationTokenSource tokenSource)
        {
            this.tokenSource = tokenSource;
            this.Token = tokenSource.Token;
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public void Cancel()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
            }
        }


        /// <summary>
        /// 从上次获取到此次获得的发送速度
        /// </summary>
        /// <returns></returns>
        public long Speed()
        {
            this.speed = this.speedTemp;
            this.speedTemp = 0;
            return this.speed;
        }
    }
}
