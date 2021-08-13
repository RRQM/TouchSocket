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
using System;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UdpRPCParser
    /// </summary>
    public class UdpRpcParserConfig : UdpSessionConfig
    {
        /// <summary>
        /// 代理源文件命名空间, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty NameSpaceProperty =
            DependencyProperty.Register("NameSpace", typeof(string), typeof(UdpRpcParserConfig), null);

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ProxyTokenProperty =
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(UdpRpcParserConfig), null);

        /// <summary>
        /// RPC代理版本, 所需类型<see cref="Version"/>
        /// </summary>
        public static readonly DependencyProperty RPCVersionProperty =
            DependencyProperty.Register("RPCVersion", typeof(Version), typeof(UdpRpcParserConfig), null);

        /// <summary>
        /// 序列化转换器, 所需类型<see cref="RRQMRPC.SerializeConverter"/>
        /// </summary>
        public static readonly DependencyProperty SerializeConverterProperty =
            DependencyProperty.Register("SerializeConverter", typeof(SerializeConverter), typeof(UdpRpcParserConfig), new BinarySerializeConverter());

        /// <summary>
        /// 代理源文件命名空间
        /// </summary>
        public string NameSpace
        {
            get { return (string)GetValue(NameSpaceProperty); }
            set { SetValue(NameSpaceProperty, value); }
        }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken
        {
            get { return (string)GetValue(ProxyTokenProperty); }
            set { SetValue(ProxyTokenProperty, value); }
        }

        /// <summary>
        /// RPC代理版本
        /// </summary>
        public Version RPCVersion
        {
            get { return (Version)GetValue(RPCVersionProperty); }
            set { SetValue(RPCVersionProperty, value); }
        }

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter
        {
            get { return (SerializeConverter)GetValue(SerializeConverterProperty); }
            set { SetValue(SerializeConverterProperty, value); }
        }
    }
}