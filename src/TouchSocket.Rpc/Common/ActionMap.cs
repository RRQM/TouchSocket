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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 服务映射图
    /// </summary>
    public sealed class ActionMap : Dictionary<string, RpcMethod>
    {
        /// <summary>
        /// 服务映射图
        /// </summary>
        /// <param name="ignoreCase"></param>
        public ActionMap(bool ignoreCase) : base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// 通过actionKey获取函数实例
        /// </summary>
        /// <param name="actionKey"></param>
        /// <returns></returns>
        public RpcMethod GetMethodInstance(string actionKey)
        {
            this.TryGetValue(actionKey, out var methodInstance);
            return methodInstance;
        }

        /// <summary>
        /// 通过actionKey获取函数实例
        /// </summary>
        /// <param name="actionKey"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public bool TryGetMethodInstance(string actionKey, out RpcMethod methodInstance)
        {
            return this.TryGetValue(actionKey, out methodInstance);
        }
    }
}