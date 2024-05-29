using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpServiceForWebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TcpServiceController : ControllerBase
    {
        private readonly ILogger<TcpServiceController> _logger;
        private readonly ITcpService m_tcpService;

        public TcpServiceController(ILogger<TcpServiceController> logger, ITcpService tcpService)
        {
            this._logger = logger;
            this.m_tcpService = tcpService;
        }

        [HttpGet]
        public IEnumerable<string> GetAllIds()
        {
            return this.m_tcpService.GetIds();
        }

        [HttpGet]
        public ActionResult<TcpResult> SendMsgTo(string id, string msg)
        {
            try
            {
                if (this.m_tcpService.Clients.TryGetClient(id, out var client))
                {
                    client.Send(msg);
                    return new TcpResult(ResultCode.Success, "success");
                }
                else
                {
                    return new TcpResult(ResultCode.Error, "没有这个ID");
                }
            }
            catch (Exception ex)
            {
                return new TcpResult(ResultCode.Error, ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<Result> SendMsgThenWait(string id, string msg)
        {
            try
            {
                if (this.m_tcpService.Clients.TryGetClient(id, out var client))
                {
                    var result = client.CreateWaitingClient(new WaitingOptions()
                    {
                        FilterFunc = data =>
                        {
                            return true;//此处可以筛选返回数据。
                        }
                    }).SendThenReturn(Encoding.UTF8.GetBytes(msg));
                    return new Result(ResultCode.Success, Encoding.UTF8.GetString(result));
                }
                else
                {
                    return new Result(ResultCode.Error, "没有这个ID");
                }
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Error, ex.Message);
            }
        }
    }
}