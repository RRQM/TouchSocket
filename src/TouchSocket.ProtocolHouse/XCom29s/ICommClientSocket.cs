using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.ProtocolHouse;

/// <summary>
/// 通用Socket客户端接口
/// </summary>
public interface ICommClientSocket : ITcpClient
{
    #region 接口属性

    /// <summary>
    /// 服务器地址
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 心跳包频率(秒),0表示不开启心跳
    /// </summary>
    public int HeartBeatTime { get; set; }

    /// <summary>
    /// 心跳包计数器
    /// </summary>
    public int HeartBeatTimeCount { get; set; }

    /// <summary>
    /// 心跳处理委托
    /// </summary>
    public Action<TcpClientBase>? SendHeartBeat { get; set; }

    /// <summary>
    /// 心跳信息
    /// </summary>
    public IRequestInfo HeartBeatInfo { get; set; }

    /// <summary>
    /// 是否暂停连接自动连接
    /// </summary>
    public bool IsStoppable { get; set; }

    /// <summary>
    /// 需要确认底层应答
    /// </summary>
    public bool NeedConfirm { get; set; }

    #endregion 接口属性

    #region 接口方法

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="host">连接服务器地址</param>
    /// <param name="heartBeatTime">心跳时间(秒),-1表示不心跳</param>
    /// <param name="usePollingTime">轮询重连时间(秒),-1表示不轮询重连</param>
    /// <param name="checkClearTime">活性检测时间(秒),-1表示不活性检测</param>
    /// <returns></returns>
    public bool StartConnect(string host = "", int heartBeatTime = -1, int usePollingTime = -1, int checkClearTime = -1);

    /// <summary>
    /// 断开连接
    /// </summary>
    public void StopConnect();

    /// <summary>
    /// 发送数据函数
    /// </summary>
    /// <param name="requestInfo"></param>
    public void SendData(IRequestInfo requestInfo);

    #endregion 接口方法
}