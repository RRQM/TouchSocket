using Microsoft.AspNetCore.Mvc;
using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class NamedPipeController : ControllerBase
    {

        private readonly ILogger<NamedPipeController> _logger;
        private readonly INamedPipeService m_namedPipeService;

        public NamedPipeController(ILogger<NamedPipeController> logger, INamedPipeService namedPipeService)
        {
            _logger = logger;
            this.m_namedPipeService = namedPipeService;
        }

        [HttpGet]
        public IEnumerable<string> GetIds()
        {
            return m_namedPipeService.GetIds();
        }

        [HttpPost]
        public async Task<string> SendMsgThenWait(string id, string msg)
        {
            if (!m_namedPipeService.TryGetClient(id, out var namedPipeSessionClient))
            {
                return "Id��Ч";
            }

            //��������
            await namedPipeSessionClient.SendAsync(msg);
            this._logger.LogInformation("���ͳɹ�");

            //�����߼���Ҫ��ʵ���ڵ�ǰ�����������У�ֱ�ӵ���Ӧ����
            //��ϸʹ���뿴 https://touchsocket.net/docs/current/namedpipeservice

            using (var  receiver=namedPipeSessionClient.CreateReceiver())
            {
                //�趨��ʱʱ��Ϊ10��
                using (var tokenSource=new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    using (var receiverResult =await receiver.ReadAsync(tokenSource.Token))
                    {
                        //�յ������ݣ��˴������ݻ����������Ͷ�ݲ�ͬ�����ݡ�
                        var byteBlock = receiverResult.ByteBlock;
                        var requestInfo = receiverResult.RequestInfo;

                        if (receiverResult.IsCompleted)
                        {
                            //�Ͽ�������
                            this._logger.LogInformation($"�Ͽ���Ϣ��{receiverResult.Message}");
                            return "�ѶϿ�";
                        }
                        var str = byteBlock.Span.ToString(Encoding.UTF8);
                        this._logger.LogInformation(str);

                        return str;
                    }
                }
                
            }
        }
    }
}
