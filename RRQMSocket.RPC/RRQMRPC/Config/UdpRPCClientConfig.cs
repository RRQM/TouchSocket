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
using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UdpRpc
    /// </summary>
   public class UdpRPCClientConfig:UdpSessionConfig
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
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(UdpRPCClientConfig), null);

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter
        {
            get { return (SerializeConverter)GetValue(SerializeConverterProperty); }
            set { SetValue(SerializeConverterProperty, value); }
        }

        /// <summary>
        /// 序列化转换器, 所需类型<see cref="RRQMRPC.SerializeConverter"/>
        /// </summary>
        public static readonly DependencyProperty SerializeConverterProperty =
            DependencyProperty.Register("SerializeConverter", typeof(SerializeConverter), typeof(UdpRPCClientConfig), new BinarySerializeConverter());
    }
}
