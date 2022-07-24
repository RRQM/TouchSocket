//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// Rpc配置扩展
    /// </summary>
    public static class TouchRpcConfigExtensions
    {
        /// <summary>
        /// 心跳频率，默认为间隔2000ms，3次。（设置为null时禁止心跳），
        ///  所需类型<see cref="HeartbeatValue"/>
        /// </summary>
        public static readonly DependencyProperty HeartbeatFrequencyProperty =
            DependencyProperty.Register("HeartbeatFrequency", typeof(HeartbeatValue), typeof(TouchRpcConfigExtensions), new HeartbeatValue() { Interval = 2000, MaxFailCount = 3 });

        /// <summary>
        /// 序列化转换器, 所需类型<see cref="SerializationSelector"/>
        /// </summary>
        public static readonly DependencyProperty SerializationSelectorProperty =
            DependencyProperty.Register("SerializationSelector", typeof(SerializationSelector), typeof(TouchRpcConfigExtensions), new DefaultSerializationSelector());

        /// <summary>
        /// 验证超时时间,默认为3000ms, 所需类型<see cref="int"/>
        /// </summary>
        public static readonly DependencyProperty VerifyTimeoutProperty =
            DependencyProperty.Register("VerifyTimeout", typeof(int), typeof(TouchRpcConfigExtensions), 3000);

        /// <summary>
        /// 连接令箭,当为null或空时，重置为默认值“rrqm”, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty VerifyTokenProperty =
            DependencyProperty.Register("VerifyToken", typeof(string), typeof(TouchRpcConfigExtensions), "rrqm");

        /// <summary>
        /// TouchClient连接时的元数据, 所需类型<see cref="Metadata"/>
        /// </summary>
        public static readonly DependencyProperty MetadataProperty =
            DependencyProperty.Register("Metadata", typeof(Metadata), typeof(TouchRpcConfigExtensions), null);

        /// <summary>
        /// 心跳频率，默认为间隔2000ms，3次。（设置为null时禁止心跳）
        /// <para>仅适用于TouchRpcClient系类</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetHeartbeatFrequency(this TouchSocketConfig config, HeartbeatValue value)
        {
            config.SetValue(HeartbeatFrequencyProperty, value);
            return config;
        }

        /// <summary>
        /// 设置TouchClient连接时的元数据
        /// <para>仅适用于TouchRpcClient系类</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetMetadata(this TouchSocketConfig config, Metadata value)
        {
            config.SetValue(MetadataProperty, value);
            return config;
        }

        /// <summary>
        /// 设置序列化转换器
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerializationSelector(this TouchSocketConfig config, SerializationSelector value)
        {
            config.SetValue(SerializationSelectorProperty, value);
            return config;
        }

        /// <summary>
        /// 验证超时时间,默认为3000ms.
        /// <para>该配置仅<see cref="TcpTouchRpcService"/>有效</para>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetVerifyTimeout(this TouchSocketConfig config, int value)
        {
            config.SetValue(VerifyTimeoutProperty, value);
            return config;
        }

        /// <summary>
        /// 连接令箭，当为null或空时，重置为默认值“rrqm”
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetVerifyToken(this TouchSocketConfig config, string value)
        {
            config.SetValue(VerifyTokenProperty, value);
            return config;
        }

        ///// <summary>
        ///// HttpRpcUrl, 所需类型<see cref="string"/>
        ///// </summary>
        //public static readonly DependencyProperty HttpRpcUrlProperty =
        //    DependencyProperty.Register("HttpRpcUrl", typeof(string), typeof(TouchRpcConfigExtensions), "/HttpRpc");

        ///// <summary>
        ///// 设置序列化转换器
        ///// </summary>
        ///// <param name="config"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static TouchSocketConfigBuilder SetHttpRpcUrl(this TouchSocketConfigBuilder config, string value)
        //{
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        value = "/";
        //    }
        //    else
        //    {
        //        if (!value.StartsWith("/"))
        //        {
        //            value = "/" + value;
        //        }
        //    }
        //    config.SetValue(HttpRpcUrlProperty, value);
        //    return config;
        //}

        #region FileTransfer

        /// <summary>
        /// 允许的响应类型,
        ///  所需类型<see cref="TouchSocket.Rpc.TouchRpc.ResponseType"/>
        /// </summary>
        public static readonly DependencyProperty ResponseTypeProperty =
            DependencyProperty.Register("ResponseType", typeof(ResponseType), typeof(TouchRpcConfigExtensions), ResponseType.Both);

        /// <summary>
        /// 根目录
        /// 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty RootPathProperty =
            DependencyProperty.Register("RootPath", typeof(string), typeof(TouchRpcConfigExtensions), string.Empty);

        /// <summary>
        /// 设置允许的响应类型
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetResponseType(this TouchSocketConfig config, ResponseType value)
        {
            config.SetValue(ResponseTypeProperty, value);
            return config;
        }

        /// <summary>
        /// 设置根路径
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetRootPath(this TouchSocketConfig config, string value)
        {
            config.SetValue(RootPathProperty, value);
            return config;
        }

        #endregion FileTransfer

        #region 创建TcpTouchRpc

        /// <summary>
        /// 构建TcpTouchRpc类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithTcpTouchRpcClient<TClient>(this TouchSocketConfig config) where TClient : ITcpTouchRpcClient
        {
            TClient client = config.Container.Resolve<TClient>();
            client.Setup(config);
            client.Connect();
            return client;
        }

        /// <summary>
        /// 构建TcpTouchRpc类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpTouchRpcClient BuildWithTcpTouchRpcClient(this TouchSocketConfig config)
        {
            return BuildWithTcpTouchRpcClient<TcpTouchRpcClient>(config);
        }

        /// <summary>
        /// 构建TcpTouchRpc类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithTcpTouchRpcService<TService>(this TouchSocketConfig config) where TService : ITcpTouchRpcService
        {
            TService service = config.Container.Resolve<TService>();
            service.Setup(config);
            service.Start();
            return service;
        }

        /// <summary>
        /// 构建TcpTouchRpc类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpTouchRpcService BuildWithTcpTouchRpcService(this TouchSocketConfig config)
        {
            return BuildWithTcpTouchRpcService<TcpTouchRpcService>(config);
        }

        #endregion 创建TcpTouchRpc

        #region 创建HttpTouchRpc

        /// <summary>
        /// 构建HttpTouchRpc类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithHttpTouchRpcClient<TClient>(this TouchSocketConfig config) where TClient : IHttpTouchRpcClient
        {
            TClient client = config.Container.Resolve<TClient>();
            client.Setup(config);
            client.Connect();
            return client;
        }

        /// <summary>
        /// 构建HttpTouchRpc类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpTouchRpcClient BuildWithHttpTouchRpcClient(this TouchSocketConfig config)
        {
            return BuildWithHttpTouchRpcClient<HttpTouchRpcClient>(config);
        }

        /// <summary>
        /// 构建HttpTouchRpc类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithHttpTouchRpcService<TService>(this TouchSocketConfig config) where TService : IHttpTouchRpcService
        {
            TService service = config.Container.Resolve<TService>();
            service.Setup(config);
            service.Start();
            return service;
        }

        /// <summary>
        /// 构建HttpTouchRpc类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpTouchRpcService BuildWithHttpTouchRpcService(this TouchSocketConfig config)
        {
            return BuildWithHttpTouchRpcService<HttpTouchRpcService>(config);
        }

        #endregion 创建HttpTouchRpc

        #region 创建UdpTouchRpc

        /// <summary>
        /// 构建UdpTouchRpc类
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithUdpTouchRpc<TClient>(this TouchSocketConfig config) where TClient : IUdpTouchRpc
        {
            TClient client = config.Container.Resolve<TClient>();
            client.Setup(config);
            client.Start();
            return client;
        }

        /// <summary>
        /// 构建UdpTouchRpc类客户端
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static UdpTouchRpc BuildWithUdpTouchRpc(this TouchSocketConfig config)
        {
            return BuildWithUdpTouchRpc<UdpTouchRpc>(config);
        }

        #endregion 创建UdpTouchRpc
    }
}