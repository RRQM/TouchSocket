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

using System;
using System.Collections.Generic;

namespace TouchSocket.Core
{
    internal sealed class PluginInvokeLine
    {
        private List<PluginEntity> m_pluginEntities = new List<PluginEntity>();
        private int m_fromIocCount;

        public int FromIocCount => this.m_fromIocCount;

        public void Add(PluginEntity pluginEntity)
        {
            if (pluginEntity.FromIoc)
            {
                this.m_fromIocCount++;
            }
            //调用方线程安全
            var list = new List<PluginEntity>(this.m_pluginEntities);
            list.Add(pluginEntity);
            this.m_pluginEntities = list;
        }

        public List<PluginEntity> GetPluginEntities()
        {
            return this.m_pluginEntities;
        }

        public void Remove(IPlugin plugin)
        {
            //调用方线程安全
            var list = new List<PluginEntity>(this.m_pluginEntities);

            foreach (var item in list)
            {
                if (!item.IsDelegate && item.Plugin == plugin)
                {
                    list.Remove(item);
                    break;
                }
            }

            this.m_pluginEntities = list;
        }

        internal void Remove(Delegate func)
        {
            //调用方线程安全
            var list = new List<PluginEntity>(this.m_pluginEntities);

            foreach (var item in list)
            {
                if (item.IsDelegate && item.SourceDelegate == func)
                {
                    list.Remove(item);
                    break;
                }
            }
            this.m_pluginEntities = list;
        }
    }
}