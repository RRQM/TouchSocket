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
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件传输操作器。
    /// </summary>
    public partial class FileOperator : FlowOperator
    {
        private readonly FlowGate m_flowGate = new FlowGate();

        /// <summary>
        /// 文件分块大小，默认512*1024字节。
        /// 不要超过1024*1024字节。
        /// </summary>
        public int FileSectionSize { get; set; } = 512 * 1024;

        /// <summary>
        /// 文件资源信息。此值不为空时复用，可能会尝试断点续传。
        /// </summary>
        public FileResourceInfo ResourceInfo { get; set; }

        /// <summary>
        /// 资源文件路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// 存放路径，
        /// 可输入绝对路径，也可以输入相对路径。
        /// 但是必须包含文件名及扩展名。
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 失败重试次数。默认10。
        /// </summary>
        public int TryCount { get; set; } = 10;

        /// <inheritdoc/>
        public override bool SetMaxSpeed(int speed)
        {
            this.m_flowGate.Maximum = speed;
            this.MaxSpeed = speed;
            return true;
        }

        /// <inheritdoc/>
        internal void AddFlow(int flow)
        {
            this.ProtectedAddFlow(flow);
        }

        /// <inheritdoc/>
        protected override void ProtectedAddFlow(int flow)
        {
            this.m_flowGate.AddCheckWait(flow);
            base.ProtectedAddFlow(flow);
        }
    }
}