using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// UdpRPCParser
    /// </summary>
    public class UdpRPCParserConfig : UdpSessionConfig
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
        /// 序列化转换器, 所需类型<see cref="RRQMRPC.SerializeConverter"/>
        /// </summary>
        public static readonly DependencyProperty SerializeConverterProperty =
            DependencyProperty.Register("SerializeConverter", typeof(SerializeConverter), typeof(UdpRPCParserConfig), new BinarySerializeConverter());

        /// <summary>
        /// 代理源文件命名空间
        /// </summary>
        public string NameSpace
        {
            get { return (string)GetValue(NameSpaceProperty); }
            set { SetValue(NameSpaceProperty, value); }
        }

        /// <summary>
        /// 代理源文件命名空间, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty NameSpaceProperty =
            DependencyProperty.Register("NameSpace", typeof(string), typeof(UdpRPCParserConfig), null);

        /// <summary>
        /// RPC代理版本
        /// </summary>
        public Version RPCVersion
        {
            get { return (Version)GetValue(RPCVersionProperty); }
            set { SetValue(RPCVersionProperty, value); }
        }

        /// <summary>
        /// RPC代理版本, 所需类型<see cref="Version"/>
        /// </summary>
        public static readonly DependencyProperty RPCVersionProperty =
            DependencyProperty.Register("RPCVersion", typeof(Version), typeof(UdpRPCParserConfig), null);

        /// <summary>
        /// RPC编译器
        /// </summary>
        public IRPCCompiler RPCCompiler
        {
            get { return (IRPCCompiler)GetValue(RPCCompilerProperty); }
            set { SetValue(RPCCompilerProperty, value); }
        }

        /// <summary>
        /// RPC编译器, 所需类型<see cref="IRPCCompiler"/>
        /// </summary>
        public static readonly DependencyProperty RPCCompilerProperty =
            DependencyProperty.Register("RPCCompiler", typeof(IRPCCompiler), typeof(UdpRPCParserConfig), null);

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken
        {
            get { return (string)GetValue(ProxyTokenProperty); }
            set { SetValue(ProxyTokenProperty, value); }
        }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ProxyTokenProperty =
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(UdpRPCParserConfig), null);
    }
}
