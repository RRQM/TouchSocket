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
using System.Numerics;
using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using UnityRpcProxy;

namespace UnityServerConsoleApp_2D.TouchServer;

internal class UnityRpcStore : SingletonRpcServer
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
    public async Task UnitMovement(ICallContext callContext, Vector3 vector3)
    {
        if (callContext.Caller is JsonHttpSessionClient jsonsession)
        {
            jsonsession.Postion = vector3;
            foreach (JsonHttpSessionClient clientItem in jsonsession.Service.GetClients())
            {
                //通知除开玩家的其他所有客户端
                if (jsonsession != clientItem)
                {
                    await clientItem.GetJsonRpcActionClient().UpdatePositionAsync(jsonsession.ID, jsonsession.Postion, ToTimestamp(DateTime.Now));
                }

            }

            this.m_logger.Info($"玩家{jsonsession.ID}移动到{vector3}");
        }
    }

}
