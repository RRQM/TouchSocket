////------------------------------------------------------------------------------
////  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
////  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
////  CSDN博客：https://blog.csdn.net/qq_40374647
////  哔哩哔哩视频：https://space.bilibili.com/94253567
////  Gitee源代码仓库：https://gitee.com/RRQM_Home
////  Github源代码仓库：https://github.com/RRQM
////  API首页：https://touchsocket.net/
////  交流QQ群：234762506
////  感谢您的下载和使用
////------------------------------------------------------------------------------

//using System;
//using TouchSocket.Core;

//namespace TouchSocket.Http;

///// <summary>
///// Http客户端数据处理适配器
///// </summary>
//internal sealed class HttpClientDataHandlingAdapter2 : CustomUnfixedHeaderDataHandlingAdapter<UnfixedHeaderHttpResponse>
//{
//    private UnfixedHeaderHttpResponse m_httpResponseRoot;

//    /// <inheritdoc/>
//    public override bool CanSplicingSend => false;

//    /// <inheritdoc/>
//    public override void OnLoaded(object owner)
//    {
//        if (owner is not HttpClientBase clientBase)
//        {
//            throw new Exception($"此适配器必须适用于{nameof(HttpClientBase)}");
//        }
//        this.m_httpResponseRoot = new UnfixedHeaderHttpResponse(clientBase);
//        base.OnLoaded(owner);
//    }

//    protected override UnfixedHeaderHttpResponse GetInstance()
//    {
//        return this.m_httpResponseRoot;
//    }

//    protected override void OnReceivedSuccess(UnfixedHeaderHttpResponse request)
//    {
//        base.OnReceivedSuccess(request);
//        request.ResetHttp();
//    }
//}

//internal class UnfixedHeaderHttpResponse : HttpResponse, IUnfixedHeaderRequestInfo
//{
//    private int m_headerLength;

//    internal UnfixedHeaderHttpResponse(HttpClientBase httpClientBase) : base(httpClientBase)
//    {
//    }

//    public int BodyLength => (int)this.ContentLength;

//    public int HeaderLength => this.m_headerLength;

//    public bool OnParsingBody(ReadOnlySpan<byte> body)
//    {
//        this.InternalSetContent(body.ToArray());
//        return true;
//    }

//    public bool OnParsingHeader<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
//    {
//        var startPos = byteBlock.Position;

//        if (this.ParsingHeader(ref byteBlock))
//        {
//            this.m_headerLength = byteBlock.Position - startPos;
//            return true;
//        }

//        return false;
//    }
//}