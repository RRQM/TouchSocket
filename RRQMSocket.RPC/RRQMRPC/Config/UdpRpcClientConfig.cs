//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Dependency;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UdpRpc
    /// </summary>
    public class UdpRpcClientConfig : UdpSessionConfig
    {
        /// <summary>
        /// 代理文件令箭
        /// </summary>
        public string ProxyToken
        {
            get { return (string)GetValue(ProxyTokenProperty); }
            set { SetValue(ProxyTokenProperty, value); }
        }

        /// <summary>
        /// 代理文件令箭, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ProxyTokenProperty =
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(UdpRpcClientConfig), null);

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializationSelector SerializationSelector
        {
            get { return (SerializationSelector)GetValue(SerializationSelectorProperty); }
            set { SetValue(SerializationSelectorProperty, value); }
        }

        /// <summary>
        /// 序列化转换器, 所需类型<see cref="RRQMRPC.SerializationSelector"/>
        /// </summary>
        public static readonly DependencyProperty SerializationSelectorProperty =
            DependencyProperty.Register("SerializationSelector", typeof(SerializationSelector), typeof(UdpRpcClientConfig), new DefaultSerializationSelector());
    }
}