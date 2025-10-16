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

namespace TouchSocket.Http;

internal sealed class HttpServerDataHandlingAdapter : SingleStreamDataHandlingAdapter
{
    private ServerHttpRequest m_currentRequest;
    private ServerHttpRequest m_requestRoot;
    private long m_surLen;
    private Task m_task;

    /// <inheritdoc/>
    public override void OnLoaded(object owner)
    {
        if (owner is not HttpSessionClient httpSessionClient)
        {
            throw new Exception($"此适配器必须适用于{nameof(IHttpService)}");
        }

        this.m_requestRoot = new ServerHttpRequest(httpSessionClient);
        base.OnLoaded(owner);
    }

    /// <inheritdoc/>
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.m_currentRequest == null)
            {
                if (this.m_task != null)
                {
                    await this.m_task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_task = null;
                }

                this.m_requestRoot.Reset();
                this.m_currentRequest = this.m_requestRoot;
                if (this.m_currentRequest.ParsingHeader(ref reader))
                {
                    if (this.m_currentRequest.ContentLength > reader.BytesRemaining)
                    {
                        this.m_surLen = this.m_currentRequest.ContentLength;

                        this.m_task = this.GoReceivedAsync(null, this.m_currentRequest);
                    }
                    else
                    {
                        var contentLength = (int)this.m_currentRequest.ContentLength;
                        var content = reader.GetMemory(contentLength);
                        reader.Advance(contentLength);

                        this.m_currentRequest.InternalSetContent(content);
                        //this.m_currentRequest.CompleteInput();
                        this.m_task = this.GoReceivedAsync(null, this.m_currentRequest);

                        this.m_currentRequest = null;
                    }
                }
                else
                {
                    this.m_currentRequest = null;
                    return;
                }
            }

            if (this.m_surLen > 0)
            {
                if (reader.BytesRemaining > 0)
                {
                    var len = (int)Math.Min(this.m_surLen, reader.BytesRemaining);

                    await this.m_currentRequest.InternalInputAsync(reader.GetMemory(len)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_surLen -= len;
                    reader.Advance(len);
                    if (this.m_surLen == 0)
                    {
                        this.m_currentRequest.CompleteInput();
                        this.m_currentRequest = null;
                    }
                }
            }
            else
            {
                this.m_currentRequest = null;
            }
        }
    }
}