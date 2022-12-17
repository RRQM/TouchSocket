using System;
using System.Threading;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 关于具有流速的操作器。
    /// </summary>
    public abstract class FlowOperator
    {
        /// <summary>
        /// 已完成长度
        /// </summary>
        protected long completedLength;

        /// <summary>
        /// 进度
        /// </summary>
        protected float m_progress;

        private long m_speed;
        private long m_speedTemp;

        /// <summary>
        /// 已完成长度
        /// </summary>
        /// <returns></returns>
        public long CompletedLength { get => completedLength; }

        /// <summary>
        /// 由<see cref="Result"/>的结果，判断是否已结束操作。
        /// </summary>
        public virtual bool IsEnd { get => Result.ResultCode != ResultCode.Default; }

        /// <summary>
        /// 数据源的全部长度。
        /// </summary>
        public long Length { get; protected set; }

        /// <summary>
        /// 最大传输速度。
        /// </summary>
        public int MaxSpeed { get; protected set; } = int.MaxValue;

        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => m_progress;

        /// <summary>
        /// 执行结果
        /// </summary>
        public Result Result { get; protected set; }

        /// <summary>
        /// 超时时间，默认10*1000ms。
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 可取消令箭
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// 从上次获取到此次获得的速度
        /// </summary>
        /// <returns></returns>
        public long Speed()
        {
            m_speed = m_speedTemp;
            m_speedTemp = 0;
            return m_speed;
        }

        internal void SetLength(long len)
        {
            Length = len;
        }

        /// <summary>
        /// 设置结果状态
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal Result SetResult(Result result)
        {
            Result = result;
            return result;
        }

        /// <summary>
        /// 添加流速(线程安全)
        /// </summary>
        /// <param name="flow"></param>
        protected internal virtual void AddFlow(int flow)
        {
            Interlocked.Add(ref m_speedTemp, flow);
            m_progress = (float)((double)Interlocked.Add(ref completedLength, flow) / Length);
        }
    }
}