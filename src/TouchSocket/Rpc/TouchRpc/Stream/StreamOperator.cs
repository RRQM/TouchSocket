//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.Data.Security;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 流操作
    /// </summary>
    public class StreamOperator
    {
        /// <summary>
        /// 已完成长度
        /// </summary>
        protected long completedLength;

        /// <summary>
        /// 结果
        /// </summary>
        protected Result result;

        private long speed;

        /// <summary>
        /// 临时速度
        /// </summary>
        protected long speedTemp;

        /// <summary>
        /// 进度
        /// </summary>
        protected float progress;

        /// <summary>
        /// 已完成长度
        /// </summary>
        /// <returns></returns>
        public long CompletedLength => this.completedLength;

        /// <summary>
        /// 最大传输速度（企业版默认1024*1024字节，开源版不限速，所以此值无效。）
        /// </summary>
        public int MaxSpeed => int.MaxValue;

        /// <summary>
        /// 超时时间，默认10*1000ms。
        /// </summary>
        public int Timeout { get; set; } = 10 * 1000;

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => this.progress;

        /// <summary>
        /// 执行结果
        /// </summary>
        public Result Result => this.result;

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
            this.speed = this.speedTemp;
            this.speedTemp = 0;
            return this.speed;
        }

        internal void AddStreamFlow(int flow, long length)
        {
            this.speedTemp += flow;
            this.completedLength += flow;
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal Result SetStreamResult(Result result)
        {
            this.result = result;
            return result;
        }
    }
}