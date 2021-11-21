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

        private int packageSize=1024*64;

        /// <summary>
        /// 包长度，默认64Kb
        /// </summary>
        public int PackageSize
        {
            get { return packageSize; }
            set { packageSize = value; }
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
