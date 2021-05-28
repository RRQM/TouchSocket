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

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// 路由映射图
    /// </summary>
    public class RouteMap : IEnumerable<KeyValuePair<string, MethodInstance>>
    {
        internal RouteMap()
        {
            this.routeMap = new Dictionary<string, MethodInstance>();
        }

        private Dictionary<string, MethodInstance> routeMap;

        internal void Add(string routeUrl, MethodInstance methodInstance)
        {
            this.routeMap.Add(routeUrl, methodInstance);
        }

        /// <summary>
        /// 路由路径集合
        /// </summary>
        public IEnumerable<string> Urls { get { return this.routeMap.Keys; } }

        /// <summary>
        /// 通过routeUrl获取函数实例
        /// </summary>
        /// <param name="routeUrl"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public bool TryGet(string routeUrl, out MethodInstance methodInstance)
        {
            if (this.routeMap.ContainsKey(routeUrl))
            {
                methodInstance = this.routeMap[routeUrl];
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
            return this.routeMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.routeMap.GetEnumerator();
        }
    }
}