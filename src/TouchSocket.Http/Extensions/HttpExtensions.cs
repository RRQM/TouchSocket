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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// Http扩展辅助
    /// </summary>
    public static partial class HttpExtensions
    {
        #region HttpBase

        /// <summary>
        /// 同步获取一次性内容。
        /// </summary>
        /// <returns>返回一个只读内存块，该内存块包含具体的字节内容。</returns>
        /// <param name="httpBase"></param>
        /// <param name="cancellationToken">一个CancellationToken对象，用于取消异步操作。</param>
        public static ReadOnlyMemory<byte> GetContent(this HttpBase httpBase, CancellationToken cancellationToken = default)
        {
            // 使用Task.Run来启动一个新的任务，该任务将异步地获取内容。
            // 这里使用GetFalseAwaitResult()方法来处理任务的结果，确保即使在同步上下文中也能正确处理异常。
            return Task.Run(async () => await httpBase.GetContentAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext)).GetFalseAwaitResult();
        }


        /// <summary>
        /// 添加Header参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TRequest AddHeader<TRequest>(this TRequest request, string key, string value) where TRequest : HttpBase
        {
            request.Headers.Add(key, value);
            return request;
        }

        /// <summary>
        /// 添加Header参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TRequest AddHeader<TRequest>(this TRequest request, HttpHeaders key, string value) where TRequest : HttpBase
        {
            request.Headers.Add(key, value);
            return request;
        }

        #region 设置内容

        /// <summary>
        /// 从Json
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromJson<T>(this T request, string value) where T : HttpBase
        {
            request.SetContent(Encoding.UTF8.GetBytes(value));
            request.Headers.Add(HttpHeaders.ContentType, "application/json;charset=UTF-8");
            return request;
        }

        /// <summary>
        /// 从文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromText<T>(this T request, string value) where T : HttpBase
        {
            request.SetContent(Encoding.UTF8.GetBytes(value));
            request.Headers.Add(HttpHeaders.ContentType, "text/plain;charset=UTF-8");
            return request;
        }

        /// <summary>
        /// 从Xml格式
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromXML<T>(this T request, string value) where T : HttpBase
        {
            request.SetContent(Encoding.UTF8.GetBytes(value));
            request.Headers.Add(HttpHeaders.ContentType, "application/xml;charset=UTF-8");
            return request;
        }

        #endregion 设置内容

        /// <summary>
        /// 获取Body的字符串
        /// </summary>
        /// <param name="httpBase"></param>
        /// <returns></returns>
        public static string GetBody(this HttpBase httpBase)
        {
            return GetBodyAsync(httpBase).GetFalseAwaitResult();
        }

        /// <summary>
        /// 异步获取 HTTP 请求的主体内容。
        /// </summary>
        /// <param name="httpBase">HttpBase 实例，用于发起 HTTP 请求。</param>
        /// <returns>返回主体内容的字符串表示，如果内容为空则返回 null。</returns>
        public static async Task<string> GetBodyAsync(this HttpBase httpBase)
        {
            // 异步获取 HTTP 响应的内容作为字节数组
            var bytes = await httpBase.GetContentAsync(CancellationToken.None);
            // 如果字节数组为空，则返回 null，否则使用 UTF-8 编码将字节数组转换为字符串并返回
            return bytes.IsEmpty ? null : bytes.Span.ToString(Encoding.UTF8);
        }

        /// <summary>
        /// 当数据类型为multipart/form-data时，获取boundary
        /// </summary>
        /// <param name="httpBase"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetBoundary(this HttpBase httpBase)
        {
            // 检查ContentType是否为空，如果为空则直接返回空字符串
            if (httpBase.ContentType.IsNullOrEmpty())
            {
                return string.Empty;
            }
            // 分割ContentType字符串，以";"为分隔符
            var strs = httpBase.ContentType.Split(';');
            // 如果分割后的长度为2，表示存在boundary信息
            if (strs.Length == 2)
            {
                // 进一步分割第二个部分，以"="为分隔符
                strs = strs[1].Split('=');
                // 如果分割后的长度为2，表示成功获取到boundary信息
                if (strs.Length == 2)
                {
                    // 移除boundary中的双引号并返回
                    return strs[1].Replace("\"", string.Empty).Trim();
                }
            }
            // 如果没有成功获取到boundary信息，返回空字符串
            return string.Empty;
        }

        /// <summary>
        /// 为HttpBase类型对象设置内容。
        /// </summary>
        /// <typeparam name="T">泛型参数T，表示HttpBase类型或其派生类型。</typeparam>
        /// <param name="httpBase">需要设置内容的HttpBase类型对象。</param>
        /// <param name="content">要设置的内容，类型为HttpContent。</param>
        /// <returns>返回设置内容后的HttpBase对象。</returns>
        public static T SetContent<T>(this T httpBase, HttpContent content) where T : HttpBase
        {
            // 将传入的内容设置到HttpBase对象中
            httpBase.Content = content;
            // 返回处理后的HttpBase对象
            return httpBase;
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="httpBase">HttpBase类型的对象，其内容需要被设置</param>
        /// <param name="content">要设置的内容，以字符串形式传入</param>
        /// <param name="encoding">内容的编码方式，如果未指定，则默认使用UTF-8编码</param>
        /// <returns>返回设置后的内容对象，允许进行方法链调用</returns>
        public static T SetContent<T>(this T httpBase, string content, Encoding encoding = null) where T : HttpBase
        {
            // 将内容转换为字节数组，并调用SetContent方法进行设置
            httpBase.SetContent(Encoding.UTF8.GetBytes(content));
            // 返回处理后的HttpBase对象
            return httpBase;
        }

        /// <summary>
        /// 设置数据体长度
        /// </summary>
        /// <param name="httpBase">要设置数据体长度的HttpBase对象</param>
        /// <param name="value">数据体的长度</param>
        /// <returns>返回修改后的HttpBase对象</returns>
        public static T SetContentLength<T>(this T httpBase, long value) where T : HttpBase
        {
            httpBase.ContentLength = value;
            return httpBase;
        }

        /// <summary>
        /// 从扩展名设置内容类型，必须以“.”开头
        /// </summary>
        /// <param name="httpBase">要设置内容类型的 HttpBase 对象</param>
        /// <param name="extension">文件扩展名，必须以“.”开头，用于确定内容类型</param>
        /// <returns>返回设置后的 HttpBase 对象</returns>
        public static T SetContentTypeByExtension<T>(this T httpBase, string extension) where T : HttpBase
        {
            // 根据扩展名获取对应的内容类型
            var type = HttpTools.GetContentTypeFromExtension(extension);
            // 设置 HttpBase 对象的内容类型
            httpBase.ContentType = type;
            // 返回设置后的 HttpBase 对象
            return httpBase;
        }

        #endregion HttpBase

        #region HttpRequest
        /// <summary>
        /// 添加Query参数
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        /// <returns>返回添加了参数的请求对象</returns>
        public static TRequest AddQuery<TRequest>(this TRequest request, string key, string value) where TRequest : HttpRequest
        {
            // 为请求对象的Query集合添加新的键值对参数
            request.Query.Add(key, value);
            // 返回修改后的请求对象
            return request;
        }

        /// <summary>
        /// 将一个键值对集合按照application/x-www-form-urlencoded格式设置到HttpRequest的内容中
        /// </summary>
        /// <param name="request">待设置内容的HttpRequest对象</param>
        /// <param name="nameValueCollection">包含键值对的集合，将被转换为查询字符串格式</param>
        /// <typeparam name="TRequest">HttpRequest的类型，使用泛型以支持所有HttpRequest的子类</typeparam>
        public static void SetFormUrlEncodedContent<TRequest>(this TRequest request, IEnumerable<KeyValuePair<string, string>> nameValueCollection)
            where TRequest : HttpRequest
        {
            // 将键值对集合转换为查询字符串格式，并设置为请求的内容
            request.SetContent(string.Join("&", nameValueCollection.Select(a => $"{a.Key}={a.Value}")));
            // 设置请求的内容类型为application/x-www-form-urlencoded
            request.ContentType = "application/x-www-form-urlencoded";
        }

        /// <summary>
        /// 异步获取HttpRequest的表单集合
        /// </summary>
        /// <param name="request">HttpRequest对象，用于提取表单数据</param>
        /// <typeparam name="TRequest">泛型参数，限定为HttpRequest类型</typeparam>
        /// <returns>返回一个任务，该任务的结果是IFormCollection类型的表单集合</returns>
        public static async Task<IFormCollection> GetFormCollectionAsync<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            // 检查请求中是否包含分隔符，这是判断是否存在多文件数据的依据。
            // 如果分隔符为空或未指定，则认为请求中没有多文件数据。

            var boundaryString = request.GetBoundary();

            if (boundaryString.IsNullOrEmpty())
            {
                if (request.ContentType == @"application/x-www-form-urlencoded")
                {
                    return new InternalFormCollection(await request.GetContentAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext));
                }
                return new InternalFormCollection();
            }
            else
            {
                var boundary = $"--{boundaryString}".ToUTF8Bytes();

                return new InternalFormCollection(await request.GetContentAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext), boundary);
            }
        }

        /// <summary>
        /// 获取多文件集合。如果不存在，则返回null。
        /// </summary>
        /// <typeparam name="TRequest">请求类型，必须继承自HttpRequest。</typeparam>
        /// <param name="request">请求对象，用于提取多文件集合。</param>
        /// <returns>多文件集合对象，如果请求中不存在多文件数据，则返回null。</returns>
        [Obsolete("此方法已被弃用，请使用GetFormCollectionAsync代替，然后使用获取Files属性", true)]
        public static IMultifileCollection GetMultifileCollection<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 初始化常规的请求头。
        /// <para>包含：</para>
        /// <list type="number">
        /// <item>Connection:keep-alive</item>
        /// <item>Pragma:no-cache</item>
        /// <item>UserAgent:TouchSocket.Http</item>
        /// </list>
        /// </summary>
        /// <param name="request">要初始化请求头的请求对象。</param>
        /// <returns>返回初始化后的请求对象。</returns>
        public static TRequest InitHeaders<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            // 添加保持连接的头，以支持持久连接
            request.Headers.Add(HttpHeaders.Connection, "keep-alive");
            // 添加禁止缓存的头，确保请求最新
            request.Headers.Add(HttpHeaders.Pragma, "no-cache");
            // 设置用户代理头，标识使用的Http库
            request.Headers.Add(HttpHeaders.UserAgent, "TouchSocket.Http");
            return request;
        }

        /// <summary>
        /// 添加Host请求头
        /// </summary>
        /// <param name="request">要添加Host请求头的HttpRequest对象</param>
        /// <param name="host">要设置的Host值</param>
        /// <returns>返回修改后的HttpRequest对象</returns>
        public static TRequest SetHost<TRequest>(this TRequest request, string host) where TRequest : HttpRequest
        {
            request.Headers.Add(HttpHeaders.Host, host);
            return request;
        }

        /// <summary>
        /// 对比不包含参数的Url。其中有任意一方为null，则均返回False。
        /// </summary>
        /// <param name="request">请求对象，用于获取待对比的相对URL。</param>
        /// <param name="url">待对比的目标URL字符串。</param>
        /// <returns>如果两个URL都不为null且在忽略大小写的情况下相等，则返回true；否则返回false。</returns>
        public static bool UrlEquals<TRequest>(this TRequest request, string url) where TRequest : HttpRequest
        {
            // 检查两个URL是否都不为null，并且在文化无关的大小写不敏感的情况下是否相等
            return !string.IsNullOrEmpty(request.RelativeURL) && !string.IsNullOrEmpty(url)
&& request.RelativeURL.Equals(url, StringComparison.CurrentCultureIgnoreCase);
        }

        #region 设置函数

        /// <summary>
        /// 将请求对象的方法设置为Delete。
        /// </summary>
        /// <param name="request">要设置为Delete方法的请求对象。</param>
        /// <returns>返回修改过方法类型的请求对象。</returns>
        public static TRequest AsDelete<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Delete;
            return request;
        }

        /// <summary>
        /// 将请求方法设置为Get
        /// </summary>
        /// <param name="request">要转换为Get方法的请求对象</param>
        /// <returns>修改过方法的请求对象</returns>
        public static TRequest AsGet<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Get;
            return request;
        }

        /// <summary>
        /// 将指定的请求对象设置为指定的HTTP方法。
        /// 该方法扩展了HttpRequest类，允许在调用时动态改变请求的方法类型。
        /// </summary>
        /// <param name="request">要修改的HttpRequest对象。</param>
        /// <param name="method">要设置的HTTP方法，如"Get"、"Post"等。</param>
        /// <returns>修改后的HttpRequest对象。</returns>
        public static TRequest AsMethod<TRequest>(this TRequest request, string method) where TRequest : HttpRequest
        {
            // 设置请求对象的方法类型为传入的HTTP方法。
            request.Method = new HttpMethod(method);
            // 返回修改后的请求对象。
            return request;
        }

        /// <summary>
        /// 将请求对象设置为Post方法
        /// </summary>
        /// <param name="request">要设置为Post方法的请求对象</param>
        /// <returns>修改过方法类型的请求对象</returns>
        public static TRequest AsPost<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Post;
            return request;
        }

        /// <summary>
        /// 将请求对象的方法设置为PUT
        /// </summary>
        /// <param name="request">要修改的请求对象</param>
        /// <returns>修改后的请求对象</returns>
        public static TRequest AsPut<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Put;
            return request;
        }

        #endregion 设置函数

        #region 判断函数

        /// <summary>
        /// 判断当前请求是否为Delete操作
        /// </summary>
        /// <param name="request">请求对象，用于检查请求方法</param>
        /// <returns>如果请求方法为Delete，则返回true；否则返回false</returns>
        public static bool IsDelete<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Delete;
        }

        /// <summary>
        /// 判断当前请求是否为Get请求
        /// </summary>
        /// <param name="request">请求对象，用于检查其请求方法</param>
        /// <returns>如果请求方法是Get，则返回true；否则返回false</returns>
        public static bool IsGet<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Get;
        }

        /// <summary>
        /// 判断指定的请求是否为指定的HTTP方法类型
        /// </summary>
        /// <param name="request">待检查的HTTP请求</param>
        /// <param name="method">要判断的HTTP方法类型，如"Get"、"Post"</param>
        /// <returns>如果请求的方法类型与指定的方法一致，则返回true；否则返回false</returns>
        public static bool IsMethod<TRequest>(this TRequest request, string method) where TRequest : HttpRequest
        {
            return request.Method == new HttpMethod(method);
        }

        /// <summary>
        /// 判断当前请求是否为Post请求
        /// </summary>
        /// <param name="request">请求对象，泛型参数，必须是HttpRequest的子类或实现</param>
        /// <returns>如果当前请求方法是Post，则返回true；否则返回false</returns>
        public static bool IsPost<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            // 直接比较请求对象的Method属性是否为HttpMethod.Post，以判断是否为Post请求
            return request.Method == HttpMethod.Post;
        }

        /// <summary>
        /// 判断请求是否为PUT方法
        /// </summary>
        /// <param name="request">请求对象，类型为HttpRequest的泛型实例</param>
        /// <returns>如果请求方法为PUT，则返回true；否则返回false</returns>
        public static bool IsPut<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Put;
        }

        #endregion 判断函数

        #region 判断属性

        /// <summary>
        /// 判断请求是否接受Gzip压缩。
        /// </summary>
        /// <param name="request">请求对象，用于获取请求的接受编码。</param>
        /// <returns>如果请求接受Gzip压缩，则返回true；否则返回false。</returns>
        public static bool IsAcceptGzip<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            // 获取请求头中的接受编码信息
            var acceptEncoding = request.AcceptEncoding;

            // 检查接受编码是否包含Gzip
            return !acceptEncoding.IsNullOrEmpty() && acceptEncoding.Contains("gzip");
        }

        /// <summary>
        /// 判断请求头中是否包含升级连接
        /// </summary>
        /// <param name="request">请求对象，泛型参数，要求是HttpRequest的子类或实现</param>
        /// <returns>如果请求头中包含升级连接，则返回true；否则返回false</returns>
        public static bool IsUpgrade<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            // 比较请求头中的连接类型是否为升级类型，忽略大小写
            return string.Equals(request.Headers.Get(HttpHeaders.Connection), HttpHeaders.Upgrade.GetDescription(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion 判断属性

        #endregion HttpRequest

        #region HttpResponse

        /// <summary>
        /// 判断返回的状态码是否为成功。
        /// </summary>
        /// <param name="response">要判断的响应对象。</param>
        /// <param name="status">
        /// 可选参数，指定期望的状态码。
        /// 当不指定具体的状态码时，只要状态码在200-299之间则为<see langword="true"/>。
        /// 当指定时，状态码不仅必须要在200-299之间，还必须是指定的状态码才会返回<see langword="true"/>。
        /// </param>
        /// <returns>返回一个布尔值，表示响应状态码是否表示成功。</returns>
        public static bool IsSuccess<TResponse>(this TResponse response, int? status = default) where TResponse : HttpResponse
        {
            // 判断是否指定了特定的状态码
            if (status.HasValue)
            {
                // 检查状态码是否与指定的状态码相匹配，并且是否在200-299之间
                return response.StatusCode == status && response.StatusCode >= 200 && response.StatusCode < 300;
            }
            else
            {
                // 检查状态码是否在200-299之间
                return response.StatusCode >= 200 && response.StatusCode < 300;
            }
        }

        /// <summary>
        /// 设置文件类型。
        /// </summary>
        /// <param name="response">要设置的HTTP响应对象。</param>
        /// <param name="fileName">文件名，用于确定Content-Type和文件下载时的提示名称。</param>
        /// <returns>返回设置后的HTTP响应对象。</returns>
        public static TResponse SetContentTypeFromFileName<TResponse>(this TResponse response, string fileName) where TResponse : HttpResponse
        {
            // 根据文件名设置Content-Disposition头部，以指定文件下载时的处理方式和文件名
            var contentDisposition = "attachment;" + "filename=" + System.Web.HttpUtility.UrlEncode(fileName);
            response.Headers.Add(HttpHeaders.ContentDisposition, contentDisposition);
            return response;
        }

        /// <summary>
        /// 为指定的HttpResponse对象设置Gzip压缩内容
        /// </summary>
        /// <typeparam name="TResponse">HttpResponse的类型</typeparam>
        /// <param name="response">要设置内容的HttpResponse对象</param>
        /// <param name="gzipContent">要设置的Gzip压缩内容</param>
        /// <returns>返回设置了Gzip内容的HttpResponse对象</returns>
        public static TResponse SetGzipContent<TResponse>(this TResponse response, byte[] gzipContent) where TResponse : HttpResponse
        {
            // 设置HttpResponse的内容为Gzip压缩内容
            response.SetContent(gzipContent);
            // 在HttpResponse的头信息中添加ContentEncoding为gzip，标识内容已经被gzip压缩
            response.Headers.Add(HttpHeaders.ContentEncoding, "gzip");
            return response;
        }

        /// <summary>
        /// 设置状态，并且附带时间戳。
        /// </summary>
        /// <param name="response">要设置状态的HttpResponse对象。</param>
        /// <param name="status">HTTP响应状态码。</param>
        /// <param name="msg">状态描述信息。</param>
        /// <returns>返回修改后的HttpResponse对象。</returns>
        public static TResponse SetStatus<TResponse>(this TResponse response, int status, string msg) where TResponse : HttpResponse
        {
            response.StatusCode = status; // 设置HTTP状态码
            response.StatusMessage = msg; // 设置状态描述信息
            response.Headers.Add(HttpHeaders.Server, $"TouchSocket.Http {HttpBase.ServerVersion}"); // 添加服务器版本信息到Header
            response.Headers.Add(HttpHeaders.Date, DateTime.UtcNow.ToGMTString()); // 添加GMT时间到Header
            return response; // 返回修改后的HttpResponse对象
        }

        /// <summary>
        /// 设置默认Success状态，并且附带时间戳。
        /// </summary>
        /// <typeparam name="TResponse">泛型参数，表示HttpResponse的类型。</typeparam>
        /// <param name="response">要设置状态的HttpResponse对象。</param>
        /// <returns>返回设置后的HttpResponse对象。</returns>
        public static TResponse SetStatus<TResponse>(this TResponse response) where TResponse : HttpResponse
        {
            // 调用重载的SetStatus方法，设置状态码为200，状态信息为"Success"。
            return SetStatus(response, 200, "Success");
        }

        /// <summary>
        /// 设置HTTP响应为404 Not Found
        /// 此方法用于处理路径文件未找到的情况，它将HTTP响应的状态码设置为404，并在响应体中返回一个简单的HTML页面，指示文件未找到。
        /// </summary>
        /// <param name="response">要设置的HTTP响应对象</param>
        /// <returns>设置后的HTTP响应对象</returns>
        public static TResponse UrlNotFind<TResponse>(this TResponse response) where TResponse : HttpResponse
        {
            response.SetContent("<html><body><h1>404 -Not Found</h1></body></html>");
            response.SetStatus(404, "Not Found");
            response.ContentType = "text/html;charset=utf-8";
            return response;
        }


        ///<summary>
        /// 设置重定向响应。
        /// <para>
        /// 默认状态码为302，状态信息为"Found"。如果需要自定义状态码和状态信息，请使用<see cref="SetStatus{TResponse}(TResponse, int, string)"/>方法。
        /// </para>
        /// </summary>
        /// <typeparam name="TResponse">响应类型，必须继承自HttpResponse。</typeparam>
        /// <param name="response">响应对象，用于设置重定向。</param>
        /// <param name="url">重定向的目标URL。</param>
        /// <returns>返回设置了重定向的响应对象。</returns>
        public static TResponse SetRedirect<TResponse>(this TResponse response, string url) where TResponse : HttpResponse
        {
            response.Headers.Add(HttpHeaders.Location, url);
            response.SetStatus(302, "Found");
            return response;
        }
        #endregion HttpResponse
    }
}