using Microsoft.AspNetCore.Mvc;
using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeWebApplication.Controllers;

[ApiController]
[Route("[controller]/[Action]")]
public class NamedPipeController : ControllerBase
{

    private readonly ILogger<NamedPipeController> _logger;
    private readonly INamedPipeService m_namedPipeService;

    public NamedPipeController(ILogger<NamedPipeController> logger, INamedPipeService namedPipeService)
    {
        this._logger = logger;
        this.m_namedPipeService = namedPipeService;
    }

    [HttpGet]
    public IEnumerable<string> GetIds()
    {
        return this.m_namedPipeService.GetIds();
    }

    [HttpPost]
    public async Task<string> SendMsgThenWait(string id, string msg)
    {
        if (!this.m_namedPipeService.TryGetClient(id, out var namedPipeSessionClient))
        {
            return "Id无效";
        }

        //发送数据
        await namedPipeSessionClient.SendAsync(msg);
        this._logger.LogInformation("发送成功");

        //下列逻辑主要是实现在当前代码上下文中，直接等响应数据
        //详细使用请看 https://touchsocket.net/docs/current/namedpipeservice

        using (var receiver = namedPipeSessionClient.CreateReceiver())
        {
            //设定超时时间为10秒
            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                using (var receiverResult = await receiver.ReadAsync(tokenSource.Token))
                {
                    //收到的数据，此处的数据会根据适配器投递不同的数据。
                    var byteBlock = receiverResult.Memory;
                    var requestInfo = receiverResult.RequestInfo;

                    if (receiverResult.IsCompleted)
                    {
                        //断开连接了
                        this._logger.LogInformation($"断开信息：{receiverResult.Message}");
                        return "已断开";
                    }
                    var str = byteBlock.Span.ToString(Encoding.UTF8);
                    this._logger.LogInformation(str);

                    return str;
                }
            }

        }
    }
}
