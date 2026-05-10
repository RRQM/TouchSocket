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

using System.Net.Http;

namespace TouchSocket.SocketIo;

internal class Eio3HttpSockeIoTransport : ISocketIoTransport
{
    private const char Separator = '\u001E';
    private readonly Func<HttpMethod, HttpRequestMessage> m_func;
    private readonly HttpClient m_httpClient;
    private readonly Func<ISocketIoMessage, Task> m_receivedSocketIoMessage;
    private readonly SocketIoCore m_socketIo;
    private ISocketIoMessage m_socketIOMessage;

    public Eio3HttpSockeIoTransport(SocketIoCore socketIo, HttpClient httpClient, Func<HttpMethod, HttpRequestMessage> func, Func<ISocketIoMessage, Task> receivedSocketIoMessage)
    {
        this.m_socketIo = socketIo;
        this.m_httpClient = httpClient;
        this.m_func = func;
        this.m_receivedSocketIoMessage = receivedSocketIoMessage;
    }

    public async Task BeginPolling()
    {
        while (true)
        {
            try
            {
                var request = this.m_func.Invoke(HttpMethod.Get);

                var response = await this.m_httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();

                    var items = SocketIoUtility.SplitEIO3(body);

                    foreach (var item in items)
                    {
                        await this.ReceivedText(item).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public async Task PingAsync()
    {
        var request = this.m_func.Invoke(HttpMethod.Post);
        request.Content = new StringContent("3");
        await this.m_httpClient.SendAsync(request);
    }


    public async Task SendAsync(List<DataItem> dataItems)
    {
        if (dataItems[0].IsText)
        {
            var request = this.m_func.Invoke(HttpMethod.Post);
            request.Content = new StringContent(dataItems[0].Text);
            var vv = await this.m_httpClient.SendAsync(request);
        }

        var binary = dataItems.Where(x => !x.IsText).Select(x => x.Bytes).ToArray();
        if (binary.Any())
        {
            var builder = new StringBuilder();
            for (var i = 0; i < binary.Length; i++)
            {
                builder.Append('b').Append(Convert.ToBase64String(binary[i]));
                if (i != binary.Length - 1)
                {
                    builder.Append(Separator);
                }
            }
            if (builder.Length == 0)
            {
                return;
            }
            var text = builder.ToString();
            var request = this.m_func.Invoke(HttpMethod.Post);
            request.Content = new StringContent(text);
            await this.m_httpClient.SendAsync(request);
        }
    }

    private async Task ReceivedEngineIoMessage(EngineIoMessage engineIOMessage)
    {
        Console.WriteLine(engineIOMessage.MessageType);
        switch (engineIOMessage.MessageType)
        {
            case EngineIoMessageType.Open:
                break;

            case EngineIoMessageType.Close:
                //this.m_client.Close();
                break;

            case EngineIoMessageType.Ping:
                {
                    await this.PingAsync();
                }
                break;

            case EngineIoMessageType.Pong:
                break;

            case EngineIoMessageType.Message:
                {
                    var socketIOMessage = this.m_socketIo.Decode(engineIOMessage);
                    if (socketIOMessage.BytesIndexs.Length > 0)
                    {
                        socketIOMessage.Bytes = new List<byte[]>();
                        this.m_socketIOMessage = socketIOMessage;
                    }
                    else
                    {
                        await this.m_receivedSocketIoMessage.Invoke(socketIOMessage);
                    }
                }
                break;

            case EngineIoMessageType.Upgrade:
                break;

            case EngineIoMessageType.Noop:
                break;

            default:
                break;
        }
    }

    private async Task ReceivedText(string text)
    {
        await this.ReceivedEngineIoMessage(this.m_socketIo.Decode(text));
    }
}