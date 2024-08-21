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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// WebApiAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WebApiAttribute : RpcAttribute
    {
        /// <summary>
        /// 使用指定函数的WebApi特性。
        /// </summary>
        /// <param name="method"></param>
        public WebApiAttribute(HttpMethodType method)
        {
            this.Method = method;
        }

        /// <summary>
        /// 使用Get函数的WebApi特性
        /// </summary>
        public WebApiAttribute()
        {
        }

        /// <summary>
        /// 请求函数类型。
        /// </summary>
        public HttpMethodType Method { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override Type[] GetGenericConstraintTypes()
        {
            return new Type[] { typeof(IWebApiClientBase) };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="rpcMethod"></param>
        /// <returns></returns>
        public override string GetInvokeKey(RpcMethod rpcMethod)
        {
            var parameters = rpcMethod.GetNormalParameters().ToList();
            if (this.Method == HttpMethodType.GET)
            {
                var actionUrl = this.GetRouteUrls(rpcMethod)[0];
                if (parameters.Count > 0)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append(actionUrl);
                    stringBuilder.Append('?');
                    for (var i = 0; i < parameters.Count; i++)
                    {
                        stringBuilder.Append(parameters[i].Name + "={&}".Replace("&", i.ToString()));
                        if (i != parameters.Count - 1)
                        {
                            stringBuilder.Append('&');
                        }
                    }
                    actionUrl = stringBuilder.ToString();
                }
                return $"GET:{actionUrl}";
            }
            else if (this.Method == HttpMethodType.POST)
            {
                var actionUrl = this.GetRouteUrls(rpcMethod)[0];
                if (parameters.Count > 0)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append(actionUrl);
                    stringBuilder.Append('?');

                    var last = parameters.LastOrDefault();
                    if (!(last.Type.IsPrimitive || last.Type == typeof(string)))
                    {
                        parameters.Remove(last);
                    }
                    for (var i = 0; i < parameters.Count; i++)
                    {
                        stringBuilder.Append(parameters[i].Name + "={&}".Replace("&", i.ToString()));
                        if (i != parameters.Count - 1)
                        {
                            stringBuilder.Append('&');
                        }
                    }

                    //for (; i < rpcMethod.ParameterNames.Length - 1; i++)
                    //{
                    //    stringBuilder.Append(rpcMethod.ParameterNames[i] + "={&}".Replace("&", i.ToString()));
                    //    if (i != rpcMethod.ParameterNames.Length - 2)
                    //    {
                    //        stringBuilder.Append('&');
                    //    }
                    //}
                    actionUrl = stringBuilder.ToString();
                }
                return $"POST:{actionUrl}";
            }
            else
            {
                return base.GetInvokeKey(rpcMethod);
            }
        }

        /// <summary>
        /// 获取路由路径。
        /// <para>路由路径的第一个值会被当做调用值。</para>
        /// </summary>
        /// <param name="rpcMethod"></param>
        /// <returns></returns>
        public virtual string[] GetRouteUrls(RpcMethod rpcMethod)
        {
            if (rpcMethod.GetAttribute<WebApiAttribute>() is WebApiAttribute webApiAttribute)
            {
                var urls = new List<string>();

                var attrs = rpcMethod.Info.GetCustomAttributes(typeof(RouterAttribute), true);
                if (attrs.Length > 0)
                {
                    foreach (var item in attrs.Cast<RouterAttribute>())
                    {
                        var url = item.RouteTemple.ToLower()
                            .Replace("[api]", rpcMethod.ServerFromType.Name)
                            .Replace("[action]", webApiAttribute.GetMethodName(rpcMethod, false)).ToLower();

                        if (!urls.Contains(url))
                        {
                            urls.Add(url);
                        }
                    }
                }
                else
                {
                    attrs = rpcMethod.ServerFromType.GetCustomAttributes(typeof(RouterAttribute), true);
                    if (attrs.Length > 0)
                    {
                        foreach (var item in attrs.Cast<RouterAttribute>())
                        {
                            var url = item.RouteTemple.ToLower()
                                .Replace("[api]", rpcMethod.ServerFromType.Name)
                                .Replace("[action]", webApiAttribute.GetMethodName(rpcMethod, false)).ToLower();

                            if (!urls.Contains(url))
                            {
                                urls.Add(url);
                            }
                        }
                    }
                    else
                    {
                        urls.Add($"/{rpcMethod.Info.DeclaringType.Name}/{rpcMethod.Name}".ToLower());
                    }
                }

                return urls.ToArray();
            }
            return default;
        }
    }
}