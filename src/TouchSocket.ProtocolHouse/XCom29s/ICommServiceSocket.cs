using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.ProtocolHouse;

public delegate int ReceivedEventValueHandler<TClient>(TClient client, ByteBlock byteBlock, IRequestInfo requestInfo);

/// <summary>
/// 通用Socket服务端接口
/// </summary>
public interface ICommServiceSocket : ITcpService
{
    #region 接口属性

    /// <summary>
    /// 监听地址端口
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 活性检测时间(秒),0表示不进行活性检测
    /// </summary>
    public int CheckClearTime { get; set; }

    /// <summary>
    /// 指令校验(KEY要求全部大写)
    /// </summary>
    public Dictionary<string, TelegramListDto> CommandVerifications { get; set; }

    /// <summary>
    /// 接收数据处理(先处理后回复)
    /// </summary>
    public ReceivedEventValueHandler<ITcpClientBase> DataReceived { get; set; }

    #endregion 接口属性

    #region 接口方法

    /// <summary>
    /// 开始监听端口
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public bool StartService(string host = "");

    /// <summary>
    /// 停止监听端口
    /// </summary>
    /// <returns></returns>
    public bool StopService();

    #endregion 接口方法
}