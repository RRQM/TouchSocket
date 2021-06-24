//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// 服务映射图
    /// </summary>
    public class ActionMap : IEnumerable<KeyValuePair<string, MethodInstance>>
    {
        internal ActionMap()
        {
            this.actionMap = new Dictionary<string, MethodInstance>();
        }

        private Dictionary<string, MethodInstance> actionMap;

        internal void Add(string actionKey, MethodInstance methodInstance)
        {
            this.actionMap.Add(actionKey, methodInstance);
        }

        /// <summary>
        /// 服务键集合
        /// </summary>
        public IEnumerable<string> ActionKeys { get { return this.actionMap.Keys; } }

        /// <summary>
        /// 通过routeUrl获取函数实例
        /// </summary>
        /// <param name="actionKey"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public bool TryGet(string actionKey, out MethodInstance methodInstance)
        {
            if (this.actionMap.ContainsKey(actionKey))
            {
                methodInstance = this.actionMap[actionKey];
                return true;
            }
            methodInstance = null;
            return false;
        }

        /// <summary>
        /// 返回迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, MethodInstance>> GetEnumerator()
        {
            return this.actionMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.actionMap.GetEnumerator();
        }
    }
}