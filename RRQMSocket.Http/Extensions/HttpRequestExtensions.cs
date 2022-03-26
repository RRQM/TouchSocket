//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    /// <summary>
    /// HttpRequest扩展
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 设置Url，必须以“/”开头，可带参数
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpRequest SetUrl(this HttpRequest httpRequest, string url)
        {
            if (url.StartsWith("/"))
            {
                httpRequest.URL = url;
            }
            else
            {
                httpRequest.URL = "/" + url;
            }
            return httpRequest;
        }

        #region 设置函数

        /// <summary>
        /// 作为Get访问
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static HttpRequest AsGet(this HttpRequest httpRequest)
        {
            httpRequest.Method = "GET";
            return httpRequest;
        }

        /// <summary>
        /// 作为Post访问
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static HttpRequest AsPost(this HttpRequest httpRequest)
        {
            httpRequest.Method = "POST";
            return httpRequest;
        }
        /// <summary>
        /// 作为Put访问
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static HttpRequest AsPUT(this HttpRequest httpRequest)
        {
            httpRequest.Method = "PUT";
            return httpRequest;
        }

        /// <summary>
        /// 作为Delete访问
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static HttpRequest AsDelete(this HttpRequest httpRequest)
        {
            httpRequest.Method = "DELETE";
            return httpRequest;
        }

        #endregion

        /// <summary>
        /// 初始化常规的请求头。
        /// <para>包含：</para>
        /// <list type="number">
        /// <item>Connection:keep-alive</item>
        /// <item>Pragma:no-cache</item>
        /// <item>UserAgent:RRQMSocket.Http</item>
        /// <item>Accept:*/*</item>
        /// <item>AcceptEncoding:deflate, br</item>
        /// </list>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpRequest InitHeaders(this HttpRequest request)
        {
            request.SetHeader(HttpHeaders.Connection, "keep-alive");
            request.SetHeader(HttpHeaders.Pragma, "no-cache");
            request.SetHeader(HttpHeaders.UserAgent, "RRQMSocket.Http");
            request.SetHeader(HttpHeaders.Accept, "*/*");
            request.SetHeader(HttpHeaders.AcceptEncoding, "deflate, br");
            return request;
        }

        /// <summary>
        /// 添加Host请求头
        /// </summary>
        /// <param name="request"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static HttpRequest SetHost(this HttpRequest request, string host)
        {
            request.SetHeader(HttpHeaders.Host, host);
            return request;
        }

        #region 设置内容

        /// <summary>
        /// 从Xml格式
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public static HttpRequest FromXML(this HttpRequest httpRequest, string xmlText)
        {
            httpRequest.SetContent(xmlText);
            httpRequest.SetHeader(HttpHeaders.ContentType,"application/xml");
            return httpRequest;
        }

        /// <summary>
        /// 从Json
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static HttpRequest FromJson(this HttpRequest httpRequest, string jsonText)
        {
            httpRequest.SetContent(jsonText);
            httpRequest.SetHeader(HttpHeaders.ContentType, "application/json");
            return httpRequest;
        }

        /// <summary>
        /// 从文本
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static HttpRequest FromText(this HttpRequest httpRequest, string text)
        {
            httpRequest.SetContent(text);
            httpRequest.SetHeader(HttpHeaders.ContentType, "text/plain");
            return httpRequest;
        }

        #endregion
    }
}
