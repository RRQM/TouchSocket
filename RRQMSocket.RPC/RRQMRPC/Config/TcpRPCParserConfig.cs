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

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RRQMRPC解析器配置
    /// </summary>
    public class TcpRPCParserConfig : ProtocolServerConfig
    {
        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter
        {
            get { return (SerializeConverter)GetValue(SerializeConverterProperty); }
            set { SetValue(SerializeConverterProperty, value); }
        }

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public static readonly DependencyProperty SerializeConverterProperty =
            DependencyProperty.Register("SerializeConverter", typeof(SerializeConverter), typeof(TcpRPCParserConfig), new BinarySerializeConverter());

        /// <summary>
        /// 代理源文件命名空间
        /// </summary>
        public string NameSpace
        {
            get { return (string)GetValue(NameSpaceProperty); }
            set { SetValue(NameSpaceProperty, value); }
        }

        /// <summary>
        /// 代理源文件命名空间
        /// </summary>
        public static readonly DependencyProperty NameSpaceProperty =
            DependencyProperty.Register("NameSpace", typeof(string), typeof(TcpRPCParserConfig), null);

        /// <summary>
        /// RPC代理版本
        /// </summary>
        public Version RPCVersion
        {
            get { return (Version)GetValue(RPCVersionProperty); }
            set { SetValue(RPCVersionProperty, value); }
        }

        /// <summary>
        /// RPC代理版本
        /// </summary>
        public static readonly DependencyProperty RPCVersionProperty =
            DependencyProperty.Register("RPCVersion", typeof(Version), typeof(TcpRPCParserConfig), null);

        /// <summary>
        /// RPC编译器
        /// </summary>
        public IRPCCompiler RPCCompiler
        {
            get { return (IRPCCompiler)GetValue(RPCCompilerProperty); }
            set { SetValue(RPCCompilerProperty, value); }
        }

        /// <summary>
        /// RPC编译器
        /// </summary>
        public static readonly DependencyProperty RPCCompilerProperty =
            DependencyProperty.Register("RPCCompiler", typeof(IRPCCompiler), typeof(TcpRPCParserConfig), null);

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken
        {
            get { return (string)GetValue(ProxyTokenProperty); }
            set { SetValue(ProxyTokenProperty, value); }
        }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public static readonly DependencyProperty ProxyTokenProperty =
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(TcpRPCParserConfig), null);
    }
}