using System.ComponentModel;
using System.Numerics;
using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using UnityRpcProxy;

namespace UnityServerConsoleApp_2D.TouchServer;

internal class UnityRpcStore : RpcServer
{
    private readonly ILog m_logger;
    public UnityRpcStore(ILog logger)
    {
        this.m_logger = logger;
    }

    /// <summary>
    /// 将 DateTime 转换为时间戳（毫秒）
    /// </summary>
    /// <param name="dateTime">要转换的 DateTime</param>
    /// <returns>时间戳（毫秒）</returns>
    public static long ToTimestamp(DateTime dateTime)
    {
        // Unix 纪元时间
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 计算时间差并转换为毫秒
        var timeSpan = dateTime.ToUniversalTime() - unixEpoch;
        return (long)timeSpan.TotalMilliseconds;
    }

    [Description("单位移动")]
    [JsonRpc(MethodInvoke = true, MethodName = "JsonRpc_{0}")]
    public void UnitMovement(ICallContext callContext, Vector3 vector3)
    {
        if (callContext.Caller is JsonHttpSessionClient jsonsession)
        {
            jsonsession.Postion = vector3;
            foreach (JsonHttpSessionClient clientItem in jsonsession.Service.GetClients())
            {

                //通知除开玩家的其他所有客户端
                if (jsonsession != clientItem)
                {
                    clientItem.GetJsonRpcActionClient().UpdatePositionAsync(jsonsession.ID, jsonsession.Postion, ToTimestamp(DateTime.Now));
                }

            }

        }
    }

}
