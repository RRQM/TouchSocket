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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc.AspNetCore;

namespace TouchRpcWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TouchRpcController : ControllerBase
    {
        private readonly ILogger<TouchRpcController> _logger;
        private readonly IWSTouchRpcService m_touchRpcService;

        public TouchRpcController(ILogger<TouchRpcController> logger, IWSTouchRpcService touchRpcService)
        {
            _logger = logger;
            this.m_touchRpcService = touchRpcService;
        }

        [HttpGet]
        public string GetProxy()
        {
            var cellcode = Startup.RpcStore.GetProxyInfo();
            string code = CodeGenerator.ConvertToCode("RRQM", cellcode);
            return code;
        }

        [HttpGet]
        public async void WS()
        {
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            await m_touchRpcService.SwitchClientAsync(webSocket);
        }
    }
}