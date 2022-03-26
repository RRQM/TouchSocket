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
using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// Rpc配置扩展
    /// </summary>
    public static class RRQMRPCConfigExtensions
    {
        #region TcpRpcClient

        /// <summary>
        /// 序列化转换器, 所需类型<see cref="RRQMRPC.SerializationSelector"/>
        /// </summary>
        public static readonly DependencyProperty SerializationSelectorProperty =
            DependencyProperty.Register("SerializationSelector", typeof(SerializationSelector), typeof(RRQMRPCConfigExtensions), new DefaultSerializationSelector());


        /// <summary>
        /// 代理文件令箭默认验证令箭。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetProxyToken(this RRQMConfig config, string value)
        {
            config.SetValue(RpcConfigExtensions.ProxyTokenProperty, value);
            return config;
        }

        /// <summary>
        /// 设置序列化转换器
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetSerializationSelector(this RRQMConfig config, SerializationSelector value)
        {
            config.SetValue(SerializationSelectorProperty, value);
            return config;
        }

        #endregion TcpRpcClient

        #region TcpRpcParser
        #endregion TcpRpcParser
    }
}
