//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 插件事件类
    /// </summary>
    public class PluginEventArgs : TouchSocketEventArgs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool m_end = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int m_index;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PluginModel m_pluginModel;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object m_sender;

        /// <summary>
        /// 由使用者自定义的状态对象。
        /// </summary>
        public object State { get; set; }

        /// <summary>
        /// 执行的插件数量。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 调用下一个插件。
        /// </summary>
        /// <returns></returns>
        public Task InvokeNext()
        {
            if (this.m_end || this.Handled)
            {
                return EasyTask.CompletedTask;
            }

            if (this.m_pluginModel.Count > this.m_index)
            {
                this.Count++;
                return this.m_pluginModel[this.m_index++].Invoke(this.m_sender, this);
            }
            else
            {
                this.m_end = true;
                return EasyTask.CompletedTask;
            }
        }

        internal void LoadModel(PluginModel pluginModel, object sender)
        {
            this.m_sender = sender;
            this.m_pluginModel = pluginModel;
            this.m_end = false;
            this.m_index = 0;
        }
    }
}