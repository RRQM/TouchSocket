using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Dependency;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TcpRPCClient配置
    /// </summary>
    public class TcpRPCClientConfig : TokenClientConfig
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
        /// 代理文件令箭
        /// </summary>
        public static readonly DependencyProperty ProxyTokenProperty =
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(TcpRPCClientConfig), null);



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
            DependencyProperty.Register("SerializeConverter", typeof(SerializeConverter), typeof(TcpRPCClientConfig), new BinarySerializeConverter());


    }
}
