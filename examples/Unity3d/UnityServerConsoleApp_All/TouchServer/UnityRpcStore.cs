// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using static UnityServerConsoleApp_All.TouchServer.Touch_HttpDmtp;

namespace UnityServerConsoleApp_All.TouchServer;

public partial class UnityRpcStore : SingletonRpcServer
{
    public UnityRpcStore(ILog logger)
    {

        this.m_logger = logger;
    }

    private readonly ILog m_logger;

    [Description("登录")]
    [DmtpRpc(MethodInvoke = true, MethodName = "DmtpRpc_{0}")]
    [JsonRpc(MethodInvoke = true, MethodName = "JsonRpc_{0}")]
    public LoginModelResult Login(ICallContext callContext, LoginModel model)
    {
        if (callContext.Caller is HttpDmtpSessionClient session)
        {
            Console.WriteLine("HttpDmtp:请求登陆：" + model.Account + ",Pwd:" + model.Password);
        }
        if (callContext.Caller is JsonHttpDmtpSessionClient jsonsession)
        {
            Console.WriteLine("Json_webSocket:请求登陆：" + model.Account + ",Pwd:" + model.Password);
        }

        if (model.Account == "123" && model.Password == "abc")
        {
            return new LoginModelResult() { ResultCode = ResultCode.Success, Message = "Success" };
        }

        return new LoginModelResult() { ResultCode = ResultCode.Failure, Message = "账号或密码错误" };
    }

    [Description("性能测试")]
    [DmtpRpc(MethodInvoke = true, MethodName = "DmtpRpc_{0}")]
    [JsonRpc(MethodInvoke = true, MethodName = "JsonRpc_{0}")]
    public int Performance(int i)
    {
        return ++i;
    }
}
public class LoginModel
{
    public string Token { get; set; }
    public string Account { get; set; }
    public string Password { get; set; }
}

public class LoginModelResult
{
    public ResultCode ResultCode { get; set; }
    public string Message { get; set; }
}
