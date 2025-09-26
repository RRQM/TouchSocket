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

namespace TouchSocket.WebApi;

internal sealed class InternalWebApiMapping : IWebApiMapping
{
    private static readonly string[] s_regexString = new string[] { "*", "^", "-", ".", "\\" };
    private readonly Dictionary<string, Dictionary<HttpMethod, RpcMethod>> m_mappingMethods = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<HttpMethod, RpcMethod>> m_regexMethods = new(StringComparer.OrdinalIgnoreCase);

    public void AddRpcMethod(RpcMethod rpcMethod)
    {
        if (rpcMethod.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
        {
            var actionUrls = attribute.GetRouteUrls(rpcMethod);
            if (actionUrls != null)
            {
                foreach (var item in actionUrls)
                {
                    if (!this.m_mappingMethods.TryGetValue(item, out var pairs))
                    {
                        pairs = new Dictionary<HttpMethod, RpcMethod>();
                        this.m_mappingMethods.Add(item, pairs);
                    }

                    pairs.Add(new HttpMethod(attribute.Method.ToString()), rpcMethod);
                }
            }

            var actionRegexUrls = attribute.GetRegexRouteUrls(rpcMethod);
            if (actionRegexUrls != null)
            {
                foreach (var item in actionRegexUrls)
                {
                    if (!this.m_regexMethods.TryGetValue(item, out var pairs))
                    {
                        pairs = new Dictionary<HttpMethod, RpcMethod>();
                        this.m_regexMethods.Add(item, pairs);
                    }

                    pairs.Add(new HttpMethod(attribute.Method.ToString()), rpcMethod);
                }
            }
        }
    }

    public IEnumerator<MappingMethod> GetEnumerator()
    {
        foreach (var item1 in this.m_mappingMethods)
        {
            foreach (var item2 in item1.Value)
            {
                yield return new MappingMethod(item1.Key, item2.Key, item2.Value, false);
            }
        }

        foreach (var item1 in this.m_regexMethods)
        {
            foreach (var item2 in item1.Value)
            {
                yield return new MappingMethod(item1.Key, item2.Key, item2.Value, true);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public RpcMethod Match(string url, HttpMethod httpMethod)
    {
        if (!this.m_mappingMethods.TryGetValue(url, out var pairs))
        {
            foreach (var item in this.m_regexMethods)
            {
                var pattern = item.Key;
                var isMatch = Regex.IsMatch(url, pattern);
                if (isMatch)
                {
                    pairs = item.Value;
                    break;
                }
            }
        }

        if (pairs is null)
        {
            return default;
        }

        if (pairs.TryGetValue(httpMethod, out var rpcMethod))
        {
            return rpcMethod;
        }

        return default;
    }

    private bool IsRegexUrl(string url)
    {
        foreach (var item in s_regexString)
        {
            if (url.Contains(item))
            {
                return true;
            }
        }
        return false;
    }

    public void MakeReadonly()
    {

    }
}