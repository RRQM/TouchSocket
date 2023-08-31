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
        private bool m_func = true;

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
            if (this.m_func)
            {
                if (this.m_pluginModel.Funcs.Count > this.m_index)
                {
                    this.Count++;
                    return this.m_pluginModel.Funcs[this.m_index++].Invoke(this.m_sender, this);
                }
                else
                {
                    this.m_func = false;
                    this.m_index = 0;
                    return this.InvokeNext();
                }
            }
            else
            {
                if (this.m_pluginModel.PluginEntities.Count > this.m_index)
                {
                    this.Count++;
                    var entity = this.m_pluginModel.PluginEntities[this.m_index++];
                    return entity.Method.InvokeAsync(entity.Plugin, this.m_sender, this);
                }
                else
                {
                    this.m_end = true;
                    return EasyTask.CompletedTask;
                }
            }
        }

        internal void LoadModel(PluginModel pluginModel, object sender)
        {
            this.m_sender = sender;
            this.m_pluginModel = pluginModel;
            this.m_end = false;
            this.m_index = 0;
            this.m_func = true;
        }
    }
}