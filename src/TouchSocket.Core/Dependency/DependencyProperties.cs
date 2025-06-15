// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TouchSocket.Core;

[DebuggerTypeProxy(typeof(DependencyObjectDebugView))]
internal class DependencyProperties : Dictionary<int, object>
{
    #region DebugView

    private sealed class DependencyObjectDebugView
    {
        private readonly DependencyProperties m_instance;

        public DependencyObjectDebugView(DependencyProperties instance) => this.m_instance = instance;

        // 将字典转换为映射后的属性形式
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<string, object>[] Items
        {
            get
            {
                if (this.m_instance is null)
                {
                    return [];
                }
                return this.m_instance
                 .Select(kvp => new KeyValuePair<string, object>(
                     DependencyPropertyBase.s_keyNameMap.TryGetValue(kvp.Key, out var name)
                         ? name
                         : $"UnknownKey_{kvp.Key}",
                     kvp.Value))
                 .ToArray();
            }
        }
    }

    #endregion DebugView
}