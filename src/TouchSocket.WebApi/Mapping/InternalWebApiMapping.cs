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

using System.Collections;
using System.Text.RegularExpressions;
using TouchSocket.Http;
using TouchSocket.Rpc;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace TouchSocket.WebApi;

internal sealed class InternalWebApiMapping : IWebApiMapping
{
    private Dictionary<string, Dictionary<HttpMethod, RpcMethod>> m_mappingMethods = new(StringComparer.OrdinalIgnoreCase);
    private List<RegexRouteEntry> m_regexRoutes = new List<RegexRouteEntry>();

#if NET8_0_OR_GREATER
    private FrozenDictionary<string, FrozenDictionary<HttpMethod, RpcMethod>> m_frozenMappingMethods;
    private FrozenSet<RegexRouteEntry> m_frozenRegexRoutes;
#else
    private Dictionary<string, Dictionary<HttpMethod, RpcMethod>> m_frozenMappingMethods;
    private RegexRouteEntry[] m_frozenRegexRoutes;
#endif

    private bool m_isReadonly;

    public void AddRpcMethod(RpcMethod rpcMethod)
    {
        if (this.m_isReadonly)
        {
            throw new InvalidOperationException("映射已设置为只读,无法添加新的RPC方法");
        }

        if (rpcMethod.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
        {
            var httpMethod = new HttpMethod(attribute.Method.ToString());

            // 添加普通路由(精确匹配,使用字典)
            var actionUrls = attribute.GetRouteUrls(rpcMethod);
            if (actionUrls != null)
            {
                foreach (var url in actionUrls)
                {
                    if (!this.m_mappingMethods.TryGetValue(url, out var pairs))
                    {
                        pairs = new Dictionary<HttpMethod, RpcMethod>();
                        this.m_mappingMethods.Add(url, pairs);
                    }

                    pairs.Add(httpMethod, rpcMethod);
                }
            }

            // 添加正则路由(模式匹配,使用列表)
            var actionRegexUrls = attribute.GetRegexRouteUrls(rpcMethod);
            if (actionRegexUrls != null)
            {
                foreach (var pattern in actionRegexUrls)
                {
                    this.m_regexRoutes.Add(new RegexRouteEntry(pattern, httpMethod, rpcMethod));
                }
            }
        }
    }

    public IEnumerator<MappingMethod> GetEnumerator()
    {
        if (this.m_isReadonly)
        {
            // 使用冻结的数据
            foreach (var item1 in this.m_frozenMappingMethods)
            {
                foreach (var item2 in item1.Value)
                {
                    yield return new MappingMethod(item1.Key, item2.Key, item2.Value, false);
                }
            }

            foreach (var regexRoute in this.m_frozenRegexRoutes)
            {
                yield return new MappingMethod(regexRoute.Pattern, regexRoute.HttpMethod, regexRoute.RpcMethod, true);
            }
        }
        else
        {
            // 使用可变的数据
            foreach (var item1 in this.m_mappingMethods)
            {
                foreach (var item2 in item1.Value)
                {
                    yield return new MappingMethod(item1.Key, item2.Key, item2.Value, false);
                }
            }

            foreach (var regexRoute in this.m_regexRoutes)
            {
                yield return new MappingMethod(regexRoute.Pattern, regexRoute.HttpMethod, regexRoute.RpcMethod, true);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    /// 尝试匹配路由
    /// </summary>
    /// <param name="url">请求的URL</param>
    /// <param name="httpMethod">HTTP方法</param>
    /// <returns>匹配结果</returns>
    public RouteMatchResult TryMatch(string url, HttpMethod httpMethod)
    {
        if (this.m_isReadonly)
        {
            // 使用冻结集合进行查询(性能更优)
            return this.TryMatchFrozen(url, httpMethod);
        }
        else
        {
            // 使用普通字典进行查询
            return this.TryMatchMutable(url, httpMethod);
        }
    }

    /// <summary>
    /// 将映射设置为只读,并冻结所有集合以提升性能
    /// </summary>
    public void MakeReadonly()
    {
        if (this.m_isReadonly)
        {
            return;
        }

#if NET8_0_OR_GREATER
        // .NET 8+: 使用 FrozenDictionary 获得最佳性能
        var frozenBuilder = new Dictionary<string, FrozenDictionary<HttpMethod, RpcMethod>>(this.m_mappingMethods.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in this.m_mappingMethods)
        {
            frozenBuilder[kvp.Key] = kvp.Value.ToFrozenDictionary();
        }
        this.m_frozenMappingMethods = frozenBuilder.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        this.m_frozenRegexRoutes = this.m_regexRoutes.ToFrozenSet();
#else
        // .NET Framework/.NET Standard/.NET 6: 使用普通字典
        this.m_frozenMappingMethods = new Dictionary<string, Dictionary<HttpMethod, RpcMethod>>(this.m_mappingMethods, StringComparer.OrdinalIgnoreCase);
        this.m_frozenRegexRoutes = this.m_regexRoutes.ToArray();
#endif

        // 释放原始集合以节省内存
        this.m_mappingMethods = null;
        this.m_regexRoutes = null;

        this.m_isReadonly = true;
    }

    /// <summary>
    /// 使用冻结集合进行匹配(只读模式)
    /// </summary>
    private RouteMatchResult TryMatchFrozen(string url, HttpMethod httpMethod)
    {
        var isOptionsRequest = httpMethod == HttpMethod.Options;

        // 首先尝试精确匹配(冻结字典查询,性能最优)
        if (this.m_frozenMappingMethods.TryGetValue(url, out var pairs))
        {
            // 如果是OPTIONS请求,返回该路由支持的所有方法
            if (isOptionsRequest)
            {
                return RouteMatchResult.Options(pairs.Keys);
            }

            // 找到路由,检查HTTP方法是否匹配
            if (pairs.TryGetValue(httpMethod, out var rpcMethod))
            {
                return RouteMatchResult.Success(rpcMethod);
            }

            // 路由匹配但方法不匹配
            return RouteMatchResult.MethodNotAllowed(pairs.Keys);
        }

        // 精确匹配失败,尝试正则路由匹配
        var matchedRegexRoutes = new List<RegexRouteEntry>();

        foreach (var regexRoute in this.m_frozenRegexRoutes)
        {
            if (regexRoute.IsMatch(url))
            {
                matchedRegexRoutes.Add(regexRoute);
            }
        }

        // 如果正则路由有匹配
        if (matchedRegexRoutes.Count > 0)
        {
            var allowedMethods = matchedRegexRoutes.Select(r => r.HttpMethod).Distinct();

            // 如果是OPTIONS请求,返回该路由支持的所有方法
            if (isOptionsRequest)
            {
                return RouteMatchResult.Options(allowedMethods);
            }

            // 查找方法也匹配的路由
            var methodMatchedRoute = matchedRegexRoutes.FirstOrDefault(r => r.HttpMethod == httpMethod);
            if (methodMatchedRoute != null)
            {
                return RouteMatchResult.Success(methodMatchedRoute.RpcMethod);
            }

            // 路由匹配但方法不匹配
            return RouteMatchResult.MethodNotAllowed(allowedMethods);
        }

        // 没有找到任何匹配的路由
        return RouteMatchResult.NotFound();
    }

    /// <summary>
    /// 使用普通字典进行匹配(可变模式)
    /// </summary>
    private RouteMatchResult TryMatchMutable(string url, HttpMethod httpMethod)
    {
        var isOptionsRequest = httpMethod == HttpMethod.Options;

        // 首先尝试精确匹配(字典查询,O(1)性能)
        if (this.m_mappingMethods.TryGetValue(url, out var pairs))
        {
            // 如果是OPTIONS请求,返回该路由支持的所有方法
            if (isOptionsRequest)
            {
                return RouteMatchResult.Options(pairs.Keys);
            }

            // 找到路由,检查HTTP方法是否匹配
            if (pairs.TryGetValue(httpMethod, out var rpcMethod))
            {
                return RouteMatchResult.Success(rpcMethod);
            }

            // 路由匹配但方法不匹配
            return RouteMatchResult.MethodNotAllowed(pairs.Keys);
        }

        // 精确匹配失败,尝试正则路由匹配
        var matchedRegexRoutes = new List<RegexRouteEntry>();

        foreach (var regexRoute in this.m_regexRoutes)
        {
            if (regexRoute.IsMatch(url))
            {
                matchedRegexRoutes.Add(regexRoute);
            }
        }

        // 如果正则路由有匹配
        if (matchedRegexRoutes.Count > 0)
        {
            var allowedMethods = matchedRegexRoutes.Select(r => r.HttpMethod).Distinct();

            // 如果是OPTIONS请求,返回该路由支持的所有方法
            if (isOptionsRequest)
            {
                return RouteMatchResult.Options(allowedMethods);
            }

            // 查找方法也匹配的路由
            var methodMatchedRoute = matchedRegexRoutes.FirstOrDefault(r => r.HttpMethod == httpMethod);
            if (methodMatchedRoute != null)
            {
                return RouteMatchResult.Success(methodMatchedRoute.RpcMethod);
            }

            // 路由匹配但方法不匹配
            return RouteMatchResult.MethodNotAllowed(allowedMethods);
        }

        // 没有找到任何匹配的路由
        return RouteMatchResult.NotFound();
    }

    /// <summary>
    /// 正则路由条目
    /// </summary>
    private sealed class RegexRouteEntry
    {
        private readonly Regex m_regex;

        public RegexRouteEntry(string pattern, HttpMethod httpMethod, RpcMethod rpcMethod)
        {
            this.Pattern = pattern;
            this.HttpMethod = httpMethod;
            this.RpcMethod = rpcMethod;
            this.m_regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public string Pattern { get; }
        public HttpMethod HttpMethod { get; }
        public RpcMethod RpcMethod { get; }

        /// <summary>
        /// 判断URL是否匹配此正则路由
        /// </summary>
        public bool IsMatch(string url)
        {
            return this.m_regex.IsMatch(url);
        }
    }
}
