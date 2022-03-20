//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
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
            this.propertyDic = new Dictionary<Type, string>();
            this.genericTypeDic = new Dictionary<Type, string>();
        }

        private Dictionary<int, MethodItem> tokenToMethodItem;
        private Dictionary<string, MethodItem> methodKeyToMethodItem;
        internal Dictionary<Type, string> propertyDic;
        internal Dictionary<Type, string> genericTypeDic;

        /// <summary>
        /// 获取所有的方法
        /// </summary>
        /// <returns></returns>
        public string[] GetMethods()
        {
            return this.methodKeyToMethodItem.Keys.ToArray();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="methodItem"></param>
        internal void AddMethodItem(MethodItem methodItem)
        {
            this.tokenToMethodItem.Add(methodItem.MethodToken, methodItem);
            this.methodKeyToMethodItem.Add(methodItem.Method, methodItem);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="methodToken"></param>
        internal void RemoveMethodItem(int methodToken)
        {
            if (this.tokenToMethodItem.TryGetValue(methodToken, out MethodItem methodItem))
            {
                this.tokenToMethodItem.Remove(methodToken);
                this.methodKeyToMethodItem.Remove(methodItem.Method);
            }
        }

        /// <summary>
        /// 获取所有
        /// </summary>
        /// <returns></returns>
        public MethodItem[] GetAllMethodItem()
        {
            return this.tokenToMethodItem.Values.ToArray();
        }

        /// <summary>
        /// 获取函数服务
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodItem"></param>
        /// <returns></returns>
        public bool TryGetMethodItem(string method, out MethodItem methodItem)
        {
            return this.methodKeyToMethodItem.TryGetValue(method, out methodItem);
        }
    }
}