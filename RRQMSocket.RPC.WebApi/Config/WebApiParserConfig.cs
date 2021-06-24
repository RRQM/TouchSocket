using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// WebApiParser配置
    /// </summary>
    public class WebApiParserConfig : TcpServerConfig
    {

        /// <summary>
        /// 数据转化器
        /// </summary>
        public ApiDataConverter ApiDataConverter
        {
            get { return (ApiDataConverter)GetValue(ApiDataConverterProperty); }
            set { SetValue(ApiDataConverterProperty, value); }
        }

        /// <summary>
        /// 数据转化器
        /// 所需类型<see cref="RRQMSocket.RPC.WebApi.ApiDataConverter"/>
        /// </summary>
        public static readonly DependencyProperty ApiDataConverterProperty =
            DependencyProperty.Register("ApiDataConverter", typeof(ApiDataConverter), typeof(WebApiParserConfig), new XmlDataConverter());


    }
}
