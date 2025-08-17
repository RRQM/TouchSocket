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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// Http客户端基类
/// </summary>
internal abstract class HttpClientBase2 : HttpClientBase, IHttpSession
{
    public HttpClientBase2()
    {
        this.adapter = new MyHttpClientDataHandlingAdapter(this);
    }

    private readonly MyHttpClientDataHandlingAdapter adapter;

    protected override async ValueTask<bool> OnTcpReceiving(IBytesReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            if (!this.adapter.TryParseRequest(ref reader, out var response))
            {
                break;
            }
            await EasyTask.CompletedTask;
            //response.InternalInputAsync();
        }
        return true;
    }
}

internal class MyHttpClientDataHandlingAdapter : CustomDataHandlingAdapter<HttpResponse>
{
    public MyHttpClientDataHandlingAdapter(HttpClientBase httpClientBase)
    {
        this.m_httpResponse = new HttpResponse(httpClientBase);
    }

    protected override bool ParseRequestCore<TReader>(ref TReader reader, out HttpResponse request)
    {
        if (this.m_httpResponse.ParsingHeader(ref reader))
        {
            request = this.m_httpResponse;
            return true;
        }
        request = null;
        return false;
    }

    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref HttpResponse request)
    {
        throw new NotImplementedException();
    }

    private readonly HttpResponse m_httpResponse;
}