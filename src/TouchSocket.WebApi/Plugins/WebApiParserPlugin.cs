//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    [PluginOption(Singleton = true)]
    public class WebApiParserPlugin : PluginBase
    {
        private readonly IResolver m_resolver;
        private readonly IRpcServerProvider m_rpcServerProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParserPlugin(IRpcServerProvider rpcServerProvider, IResolver resolver)
        {
            if (rpcServerProvider is null)
            {
                throw new ArgumentNullException(nameof(rpcServerProvider));
            }

            this.GetRouteMap = new ActionMap(true);
            this.PostRouteMap = new ActionMap(true);
            this.Converter = new StringConverter();

            this.RegisterServer(rpcServerProvider.GetMethods());
            this.m_rpcServerProvider = rpcServerProvider;
            this.m_resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// 转化器
        /// </summary>
        public StringConverter Converter { get; private set; }

        /// <summary>
        /// 获取Get函数路由映射图
        /// </summary>
        public ActionMap GetRouteMap { get; private set; }

        /// <summary>
        /// 获取Post函数路由映射图
        /// </summary>
        public ActionMap PostRouteMap { get; private set; }

        /// <summary>
        /// 配置转换器。可以实现json序列化或者xml序列化。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public WebApiParserPlugin ConfigureConverter(Action<StringConverter> action)
        {
            action.Invoke(this.Converter);
            return this;
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            pluginManager.Add(nameof(IHttpPlugin.OnHttpRequest), this.OnHttpRequest);
            base.Loaded(pluginManager);
        }

        private async Task OnHttpGet(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.GetRouteMap.TryGetMethodInstance(e.Context.Request.RelativeURL, out var methodInstance))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;

                var callContext = new WebApiCallContext(client, methodInstance, m_resolver, e.Context);

                if (methodInstance.IsEnable)
                {
                    try
                    {
                        ps = new object[methodInstance.Parameters.Length];
                        var i = 0;
                        if (methodInstance.IncludeCallContext)
                        {
                            ps[i] = callContext;
                            i++;
                        }
                        if (e.Context.Request.Query == null)
                        {
                            for (; i < methodInstance.Parameters.Length; i++)
                            {
                                ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                            }
                        }
                        else
                        {
                            for (; i < methodInstance.Parameters.Length; i++)
                            {
                                var value = e.Context.Request.Query.Get(methodInstance.ParameterNames[i]);
                                if (!value.IsNullOrEmpty())
                                {
                                    ps[i] = this.Converter.ConvertFrom(value, methodInstance.ParameterTypes[i]);
                                }
                                else
                                {
                                    ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.Message;
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps);
                }

                if (e.Context.Response.Responsed)
                {
                    return;
                }
                var httpResponse = e.Context.Response;
                switch (invokeResult.Status)
                {
                    case InvokeStatus.Success:
                        {
                            httpResponse.FromJson(this.Converter.ConvertTo(invokeResult.Result)).SetStatus();
                            break;
                        }
                    case InvokeStatus.UnFound:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(404);
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(405);
                            break;
                        }
                    case InvokeStatus.InvocationException:
                    case InvokeStatus.Exception:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(422);
                            break;
                        }
                }

                using (var byteBlock = new ByteBlock())
                {
                    httpResponse.Build(byteBlock);
                    client.DefaultSend(byteBlock);
                }

                if (!e.Context.Request.KeepAlive)
                {
                    client.TryShutdown(SocketShutdown.Both);
                }
            }
            await e.InvokeNext();
        }

        private async Task OnHttpPost(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.PostRouteMap.TryGetMethodInstance(e.Context.Request.RelativeURL, out var methodInstance))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;

                var callContext = new WebApiCallContext(client, methodInstance, m_resolver, e.Context);

                if (methodInstance.IsEnable)
                {
                    try
                    {
                        int index;
                        ps = new object[methodInstance.Parameters.Length];
                        var i = 0;
                        if (methodInstance.IncludeCallContext)
                        {
                            ps[i] = callContext;
                            i++;
                            index = methodInstance.Parameters.Length - 2;
                        }
                        else
                        {
                            index = methodInstance.Parameters.Length - 1;
                        }
                        if (e.Context.Request.Query == null)
                        {
                            for (; i < methodInstance.Parameters.Length - 1; i++)
                            {
                                ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                            }
                        }
                        else
                        {
                            for (; i < methodInstance.Parameters.Length - 1; i++)
                            {
                                var value = e.Context.Request.Query.Get(methodInstance.ParameterNames[i]);
                                if (!value.IsNullOrEmpty())
                                {
                                    ps[i] = this.Converter.ConvertFrom(value, methodInstance.ParameterTypes[i]);
                                }
                                else
                                {
                                    ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
                            }
                        }

                        if (index >= 0)
                        {
                            var str = e.Context.Request.GetBody();
                            if (methodInstance.IncludeCallContext)
                            {
                                index++;
                            }
                            ps[index] = this.Converter.ConvertFrom(str, methodInstance.ParameterTypes[index]);
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.Message;
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, ps);
                }

                if (e.Context.Response.Responsed)
                {
                    return;
                }
                var httpResponse = e.Context.Response;
                switch (invokeResult.Status)
                {
                    case InvokeStatus.Success:
                        {
                            httpResponse.FromJson(this.Converter.ConvertTo(invokeResult.Result)).SetStatus();
                            break;
                        }
                    case InvokeStatus.UnFound:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(404, invokeResult.Status.ToString());
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(405, invokeResult.Status.ToString());
                            break;
                        }
                    case InvokeStatus.InvocationException:
                    case InvokeStatus.Exception:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(422, invokeResult.Status.ToString());
                            break;
                        }
                }

                using (var byteBlock = new ByteBlock())
                {
                    httpResponse.Build(byteBlock);
                    client.DefaultSend(byteBlock);
                }

                if (!e.Context.Request.KeepAlive)
                {
                    client.TryShutdown(SocketShutdown.Both);
                }
            }
            await e.InvokeNext();
        }

        private Task OnHttpRequest(object sender, PluginEventArgs args)
        {
            var client = (IHttpSocketClient)sender;
            var e = (HttpContextEventArgs)args;

            if (e.Context.Request.Method == HttpMethod.Get)
            {
                return this.OnHttpGet(client, e);
            }
            else if (e.Context.Request.Method == HttpMethod.Post)
            {
                return this.OnHttpPost(client, e);
            }
            else
            {
                return e.InvokeNext();
            }
        }

        private void RegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
                {
                    var actionUrls = attribute.GetRouteUrls(methodInstance);
                    if (actionUrls != null)
                    {
                        foreach (var item in actionUrls)
                        {
                            if (attribute.Method == HttpMethodType.GET)
                            {
                                this.GetRouteMap.Add(item, methodInstance);
                            }
                            else if (attribute.Method == HttpMethodType.POST)
                            {
                                this.PostRouteMap.Add(item, methodInstance);
                            }
                        }
                    }
                }
            }
        }
    }
}