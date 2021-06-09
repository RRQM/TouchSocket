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
using System.Collections.Generic;
using System.Linq;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 函数仓库
    /// </summary>
    public class MethodStore
    {
        internal MethodStore()
        {
            this.tokenToMethodItem = new Dictionary<int, MethodItem>();
            this.methodKeyToMethodItem = new Dictionary<string, MethodItem>();
        }

        private Dictionary<int, MethodItem> tokenToMethodItem;
        private Dictionary<string, MethodItem> methodKeyToMethodItem;

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="methodItem"></param>
        public void AddMethodItem(MethodItem methodItem)
        {
            tokenToMethodItem.Add(methodItem.MethodToken, methodItem);
            methodKeyToMethodItem.Add(methodItem.Method, methodItem);
        }

        /// <summary>
        /// 获取所有
        /// </summary>
        /// <returns></returns>
        public List<MethodItem> GetAllMethodItem()
        {
            return this.tokenToMethodItem.Values.ToList();
        }

        /// <summary>
        /// 获取函数服务
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodItem"></param>
        /// <returns></returns>
        public bool TryGetMethodItem(string method, out MethodItem methodItem)
        {
            return methodKeyToMethodItem.TryGetValue(method, out methodItem);
        }
    }
}