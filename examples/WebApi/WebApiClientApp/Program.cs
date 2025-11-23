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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using WebApiProxy;

namespace WebApiClientApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await TestHttpClient();

        //此处预设一个30秒超时的请求设定。
        var invokeOption_30s = new InvokeOption(30 * 1000)
        {
            FeedbackType = FeedbackType.WaitInvoke
        };

        #region WebApi客户端GET调用
        {
            var client = await CreateWebApiClient();

            var request = new WebApiRequest();
            request.Method = HttpMethodType.Get;
            request.Querys = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("a", "10"), new KeyValuePair<string, string>("b", "20") };

            var sum1 = await client.InvokeTAsync<int>("/ApiServer/Sum", invokeOption_30s, request);
            Console.WriteLine($"Get调用成功，结果：{sum1}");
        }
        #endregion

        #region WebApi客户端POST调用
        {
            var client = await CreateWebApiClient();

            var requestForPost = new WebApiRequest();
            requestForPost.Method = HttpMethodType.Post;
            requestForPost.Body = new MyClass() { A = 10, B = 20 };

            var sum2 = await client.InvokeTAsync<int>("/ApiServer/TestPost", invokeOption_30s, requestForPost);
            Console.WriteLine($"Post调用成功，结果：{sum2}");
        }
        #endregion

        #region WebApi客户端使用代理调用
        {
            var client = await CreateWebApiClient();

            var sum3 = await client.TestPostAsync(new MyClass() { A = 10, B = 20 }, invokeOption_30s);
            Console.WriteLine($"代理调用成功，结果：{sum3}");
        }
        #endregion

        #region WebApi客户端字符串模板调用
        {
            var client = await CreateWebApiClientSlim();

            var sum1 = await client.InvokeTAsync<int>("GET:/ApiServer/Sum?a={0}&b={1}", invokeOption_30s, 10, 20);
            Console.WriteLine($"Get调用成功，结果：{sum1}");

            var sum2 = await client.InvokeTAsync<int>("POST:/ApiServer/TestPost", invokeOption_30s, new MyClass() { A = 10, B = 20 });
            Console.WriteLine($"Post调用成功，结果：{sum2}");

            var sum3 = client.TestPostAsync(new MyClass() { A = 10, B = 20 }, invokeOption_30s);
            Console.WriteLine($"代理调用成功，结果：{sum3}");
        }
        #endregion

        Console.ReadKey();
    }

    #region WebApi客户端使用原生HttpClient
    private static async Task UseNativeHttpClient()
    {
        using var httpClient = new HttpClient();
        var result = await httpClient.GetStringAsync("http://localhost:7789/apiserver/sum?a=10&b=20");
        Console.WriteLine(result); // 输出: 30
    }
    #endregion

    #region WebApi客户端代码生成使用代理
    private static async Task UseGeneratedProxy()
    {
        var client = new WebApiClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789"));
        await client.ConnectAsync();

        // 直接使用生成的扩展方法
        var sum = await client.SumAsync(10, 20);
        Console.WriteLine($"结果: {sum}");
    }
    #endregion

    #region WebApi客户端使用插件
    private static async Task UsePluginExample()
    {
        var client = new WebApiClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .ConfigurePlugins(a =>
            {
                a.Add<MyWebApiPlugin>();
            }));
    }
    #endregion

    #region WebApi客户端异常处理
    private static async Task ExceptionHandlingExample()
    {
        try
        {
            var client = new WebApiClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789"));
            await client.ConnectAsync();

            var request = new WebApiRequest();
            request.Method = HttpMethodType.Get;

            var result = await client.InvokeTAsync<int>("/apiserver/sum?a=10&b=20", null, request);
            Console.WriteLine($"结果: {result}");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("请求超时");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求失败: {ex.Message}");
        }
    }
    #endregion

    #region WebApi客户端设置超时
    private static async Task SetTimeoutExample()
    {
        var client = new WebApiClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789"));
        await client.ConnectAsync();

        // 为单次调用设置超时
        var invokeOption = new InvokeOption(30 * 1000)
        {
            FeedbackType = FeedbackType.WaitInvoke
        };

        var request = new WebApiRequest();
        request.Method = HttpMethodType.Get;
        var result = await client.InvokeTAsync<int>("/apiserver/sum", invokeOption, request);
    }
    #endregion

    #region WebApi客户端使用CancellationToken
    private static async Task UseCancellationTokenExample()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            var httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync("/apiserver/sum?a=10&b=20", cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("操作已取消");
        }
    }
    #endregion

    private static async Task<HttpClient> TestHttpClient()
    {
        using var client = new HttpClient();
        await client.ConnectAsync("127.0.0.1:7789");
        Console.WriteLine("连接成功");

        using var cts = new CancellationTokenSource(1000 * 10);
        var responseString = await client.GetStringAsync("/ApiServer/Sum?a=10&b=20", cts.Token);
        return client;
    }

    #region WebApi客户端创建WebApiClient
    private static async Task<WebApiClient> CreateWebApiClient()
    {
        using var client = new WebApiClient();
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7789")
             .ConfigurePlugins(a =>
             {
                 a.Add<MyWebApiPlugin>();
             }));
        await client.ConnectAsync();
        Console.WriteLine("连接成功");
        return client;
    }
    #endregion

    #region WebApi客户端创建WebApiClientSlim
    private static async Task<WebApiClientSlim> CreateWebApiClientSlim()
    {
        using var client = new WebApiClientSlim(new System.Net.Http.HttpClient());
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("http://127.0.0.1:7789")
             .ConfigurePlugins(a =>
             {
                 a.Add<MyWebApiPlugin>();
             }));

        return client;
    }
    #endregion

    #region WebApi客户端插件拦截
    /// <summary>
    /// 此处可以做WebApi的请求之前和之后的拦截。
    /// </summary>
    private class MyWebApiPlugin : PluginBase, IWebApiRequestPlugin, IWebApiResponsePlugin
    {

        public async Task OnWebApiRequest(IWebApiClientBase client, WebApiEventArgs e)
        {
            if (e.IsHttpMessage)//发送的是System.Net.Http.HttpClient为通讯主体
            {
                //添加一个header
                e.RequestMessage.Headers.Add("Token", "123123");
            }
            else
            {
                //添加一个header
                e.Request.Headers.Add("Token", "123123");
            }

            await e.InvokeNext();
        }

        public async Task OnWebApiResponse(IWebApiClientBase client, WebApiEventArgs e)
        {
            await e.InvokeNext();
        }
    }
    #endregion
}