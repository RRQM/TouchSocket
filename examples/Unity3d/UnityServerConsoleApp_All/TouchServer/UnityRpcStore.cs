using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using static UnityServerConsoleApp_All.TouchServer.Touch_HttpDmtp;

namespace UnityServerConsoleApp_All.TouchServer;

public partial class UnityRpcStore : RpcServer
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

        return new LoginModelResult() { ResultCode = ResultCode.Fail, Message = "账号或密码错误" };
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
