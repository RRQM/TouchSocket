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


        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return (JsonRpcProtocolType)GetValue(ProtocolTypeProperty); }
            set { SetValue(ProtocolTypeProperty, value); }
        }

        /// <summary>
        /// 协议类型，
        /// 所需类型<see cref="JsonRpcProtocolType"/>
        /// </summary>       
        public static readonly DependencyProperty ProtocolTypeProperty =
        DependencyProperty.Register("ProtocolType", typeof(JsonRpcProtocolType), typeof(JsonRpcParserConfig),  JsonRpcProtocolType.Tcp);


    }
}