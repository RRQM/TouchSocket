using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpcParser配置
    /// </summary>
    public class JsonRpcParserConfig : TcpServerConfig
    {
        /// <summary>
        /// Json格式
        /// </summary>
        public JsonFormatConverter JsonFormatConverter
        {
            get { return (JsonFormatConverter)GetValue(JsonFormatConverterProperty); }
            set { SetValue(JsonFormatConverterProperty, value); }
        }

        /// <summary>
        /// Json格式
        /// 所需类型<see cref="RRQMSocket.RPC.JsonRpc.JsonFormatConverter"/>
        /// </summary>
        public static readonly DependencyProperty JsonFormatConverterProperty =
            DependencyProperty.Register("JsonFormatConverter", typeof(JsonFormatConverter), typeof(JsonRpcParserConfig), new DataContractJsonConverter());


    }
}
