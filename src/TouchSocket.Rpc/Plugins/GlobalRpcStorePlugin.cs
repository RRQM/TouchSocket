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

//using System;
//using TouchSocket.Core;

//namespace TouchSocket.Rpc
//{
//    /// <summary>
//    /// 全局Rpc仓库配置插件。
//    /// </summary>
//    [Obsolete("此配置已被弃用，请在IContainer中使用AddRpcStore替代", true)]
//    public class GlobalRpcStorePlugin : PluginBase
//    {
//        private readonly RpcStore m_rpcStore;

//        /// <summary>
//        /// 全局Rpc仓库配置插件。
//        /// </summary>
//        /// <param name="container"></param>
//        public GlobalRpcStorePlugin(IContainer container)
//        {
//            if (container.IsRegistered(typeof(RpcStore)))
//            {
//                this.m_rpcStore = container.Resolve<RpcStore>();
//            }
//            else
//            {
//                this.m_rpcStore = new RpcStore(container);
//                container.RegisterSingleton<RpcStore>(this.m_rpcStore);
//            }
//        }

//        /// <summary>
//        /// 全局配置Rpc服务
//        /// </summary>
//        /// <param name="action"></param>
//        public void ConfigureRpcStore(Action<RpcStore> action)
//        {
//            action?.Invoke(this.m_rpcStore);
//        }
//    }
//}