using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.ProtocolHouse;

#region XCom29sPlugin插件

public class XCom29sPlugin : PluginBase, ITcpConnectedPlugin<ITcpClientBase>, ITcpDisconnectedPlugin<ITcpClientBase>, ITcpSendingPlugin<ITcpClientBase>, ITcpReceivedPlugin<ITcpClientBase>
{
    #region 私有字段

    private readonly int timeTick;

    private readonly ILogger<XCom29sPlugin> logger;

    private object _lock = new();

    private bool needConfirm = true;

    #endregion 私有字段

    #region 构造函数
    
    public XCom29sPlugin(ILogger<XCom29sPlugin> logger,int timeTick = 1000 * 5)
    {
        this.timeTick = timeTick;
        this.logger = logger;
    }

    #endregion 构造函数

    #region ITcpConnectedPlugin接口

    public async Task OnTcpConnected(ITcpClientBase client, ConnectedEventArgs e)
    {
        if (client is TcpClient tcp && tcp != null) //客户端
        {
            var commclient = tcp as ICommClientSocket;
            needConfirm = commclient != null ? commclient.NeedConfirm : true;//设置是否需要底层应答

            //设置协议
            if (tcp.CanSetDataHandlingAdapter && tcp.DataHandlingAdapter != null)
            {
                tcp.SetDataHandlingAdapter(new XCom29sFixedHeaderDataHandlingAdapter());
            }

            //终止计时器
            if (client.GetValue(DependencyExtensions.ResendTimerProperty) is Timer timer)
            {
                timer.Dispose();
            }

            //清除队列
            if (client.GetValue(DependencyExtensions.ResendQueueProperty) is Dictionary<string, ResendQueueModel> models)
            {
                lock (_lock)
                {
                    foreach (var item in models)
                    {
                        if (item.Value is ResendQueueModel model && model != null)
                        {
                            model.Dispose();
                            model = null;
                        }
                    }
                    models.Clear();
                    models = null;
                }
                client.SetValue(DependencyExtensions.ResendQueueProperty, null);
            }

            //创建重发队列
            client.SetValue(DependencyExtensions.ResendQueueProperty, new());

            //开启计时器
            client.SetValue(DependencyExtensions.ResendTimerProperty, new Timer((o) =>
            {
                //队列检测
                var tcp1 = client as TcpClient;
                //this.logger.LogInformation($"[{tcp1.RemoteIPHost.EndPoint.ToString()}][{this.GetHashCode()}][客户端{tcp1.IP}:{tcp1.Port}]检测队列中......");
                lock (_lock)
                {
                    if (needConfirm && client.GetValue(DependencyExtensions.ResendQueueProperty) is Dictionary<string, ResendQueueModel> models && models != null && models.Count > 0)
                    {
                        foreach (var item in models)
                        {
                            if (item.Value is ResendQueueModel model && model != null)
                            {
                                if (DateTime.Now > model.TimeOut && model.State != 1)   //超时并且未确认或超时
                                {
                                    if (model.Request is XCom29sRequestInfo info && info != null)
                                    {
                                        this.logger.LogInformation($"[客户端{tcp1.IP}:{tcp1.Port}]正在重发指令[{info.MessageNo}]......");
                                    }
                                    client.Send(model.Request);                         //重新发送
                                    model.State = 2;                                    //设置超时
                                    model.SendTime = DateTime.Now;                      //记录当前时间
                                    model.TimeOut = DateTime.Now.AddSeconds(3);         //设置3秒后超时
                                    Task.Delay(10).GetAwaiter().GetResult();            //等待10毫秒
                                }
                            }
                        }
                    }
                }
            }, client, 3000, this.timeTick));

            this.logger.LogInformation($"[MES服务器{tcp.RemoteIPHost.EndPoint.ToString()}]成功连接......");
        }
        if (client is ISocketClient socket && socket != null)   //服务端
        {
            if (socket.CanSetDataHandlingAdapter && socket.DataHandlingAdapter != null)
            {
                socket.SetDataHandlingAdapter(new XCom29sFixedHeaderDataHandlingAdapter());
            }
            this.logger.LogInformation($"[用户{socket.Id}][{socket.IP}:{socket.Port}]已经成功连接......");
        }
        await e.InvokeNext();
    }

    #endregion ITcpConnectedPlugin接口

    #region ITcpDisconnectedPlugin接口

    public async Task OnTcpDisconnected(ITcpClientBase client, DisconnectEventArgs e)
    {
        if (client is TcpClient tcp && tcp != null) //客户端
        {
            //终止计时器
            if (client.GetValue(DependencyExtensions.ResendTimerProperty) is Timer timer)
            {
                timer.Dispose();
                client.SetValue(DependencyExtensions.ResendTimerProperty, null);
            }

            //清除队列
            if (client.GetValue(DependencyExtensions.ResendQueueProperty) is Dictionary<string, ResendQueueModel> models)
            {
                lock (_lock)
                {
                    foreach (var item in models)
                    {
                        if (item.Value is ResendQueueModel model && model != null)
                        {
                            model.Dispose();
                            model = null;
                        }
                    }
                    models.Clear();
                    models = null;
                }
                client.SetValue(DependencyExtensions.ResendQueueProperty, null);
            }

            //清除协议
            if (tcp.DataHandlingAdapter?.Owner == client)
            {
                tcp.DataHandlingAdapter?.Dispose();
            }
            this.logger.LogInformation($"[MES服务器{tcp.RemoteIPHost.EndPoint.ToString()}]已经断开连接......");
        }
        if (client is ISocketClient socket && socket != null)   //服务端
        {
            if (socket.CanSetDataHandlingAdapter && socket.DataHandlingAdapter != null)
            {
                socket.DataHandlingAdapter?.Dispose();
            }
            this.logger.LogInformation($"[用户{socket.Id}][{socket.IP}:{socket.Port}]已经断开连接......");
        }
        await e.InvokeNext();
    }

    #endregion ITcpDisconnectedPlugin接口

    #region ITcpReceivedPlugin接口

    public async Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
    {
        if (client is ISocketClient socket)  //服务端
        {
            if (e.RequestInfo is XCom29sRequestInfo info && info != null)
            {
                if (info.FunctionCode.ToUpper() == "C")//收到客户端发送的心跳包不处理
                {
                    this.logger.LogInformation($"[用户{socket.Id}][{client.IP}:{client.Port}]发送心跳消息过来......");
                }
                else if (info.FunctionCode.ToUpper() == "D" && socket.Service is ICommServiceSocket service && service != null)
                {
                    //处理FunctionCode为"C"或"D"数据包，收到"D"回复"A"或"B"

                    int result = (int)info.ResponseType;
                    var value = client.CheckCommandVerifications(info);//0表示指令校验成功,1表示指令不支持,2表示长度不相符
                    switch (value)
                    {
                        case 1:
                            //不支持的指令
                            result = (int)XCom29sResponseType.NAck;
                            info.ResponseType = XCom29sResponseType.NAck;
                            break;

                        case 2:
                            //长度不相符
                            result = (int)XCom29sResponseType.LengthMismatch;
                            info.ResponseType = XCom29sResponseType.LengthMismatch;
                            break;

                        default:
                            //校验成功
                            break;
                    }

                    if (result == (int)XCom29sResponseType.Ack)  //正确
                    {
                        client.Send(new XCom29sRequestInfo(info.MessageNo, info.ReceiveCode, info.SendCode, "A", $""));
                    }
                    else if (result == (int)XCom29sResponseType.LengthMismatch)  //不正确(非法的长度)
                    {
                        client.Send(new XCom29sRequestInfo(info.MessageNo, info.ReceiveCode, info.SendCode, "B", $"message length error"));
                    }
                    else if (result == (int)XCom29sResponseType.MissingTail)  //缺少结尾标识符
                    {
                        client.Send(new XCom29sRequestInfo(info.MessageNo, info.ReceiveCode, info.SendCode, "B", $"message no end character"));
                    }
                    else //if (result == (int)XCom29sResponseType.NAck)  //不支持的电文
                    {
                        client.Send(new XCom29sRequestInfo(info.MessageNo, info.ReceiveCode, info.SendCode, "B", $"messageId[{info.MessageNo}] is invalid"));
                    }

                    //返回给用户委托处理
                    service.DataReceived?.Invoke(client, e.ByteBlock, e.RequestInfo);//根据result值返回
                }
            }
        }
        if (client is TcpClient tcp)  //客户端
        {
            if (e.RequestInfo is XCom29sRequestInfo info && info != null)
            {
                //处理FunctionCode为"A"或"B"数据包(并检查发送队列)
                this.logger.LogInformation($"[MES服务器{tcp.RemoteIPHost.EndPoint.ToString()}]发送消息过来......");

                lock (_lock)
                {
                    if (client.GetValue(DependencyExtensions.ResendQueueProperty) is Dictionary<string, ResendQueueModel> models && models != null && models.Count > 0)
                    {
                        var key = info.MessageNo + info.ReceiveCode + info.SendCode;    //生成key
                        var flag = models.TryGetValue(key, out ResendQueueModel model);
                        if (flag)
                        {
                            model.Dispose();        //释放数据包
                            model = null;           //释放重发对象
                            models.Remove(key);     //从队列删除

                            //写日志
                            this.logger.LogInformation($"数据包[{key}]已经发送，已经从重发队列删除[队列数量：{models.Count}]，发送结果为：{info.FunctionCode}");
                        }
                    }
                }
            }
        }
        await e.InvokeNext();
    }

    #endregion ITcpReceivedPlugin接口

    #region ITcpSendingPlugin接口

    public async Task OnTcpSending(ITcpClientBase client, SendingEventArgs e)
    {
        if (client is TcpClient tcp)  //客户端
        {
            var info = new XCom29sRequestInfo(e.Buffer);
            if (info.ResponseType == XCom29sResponseType.Ack && info.FunctionCode.ToUpper() == "D")
            {
                if (needConfirm && client.GetValue(DependencyExtensions.ResendQueueProperty) is Dictionary<string, ResendQueueModel> models)
                {
                    lock (_lock)
                    {
                        var key = info.MessageNo + info.SendCode + info.ReceiveCode;    //生成key
                        ResendQueueModel model = null;
                        var flag = models.TryGetValue(key, out model);
                        if (flag)
                        {
                            //model.
                        }
                        else
                        {
                            model = new ResendQueueModel() { Request = info, SendTime = DateTime.Now, TimeOut = DateTime.Now.AddSeconds(3), State = 0 };
                            models.Add(key, model);
                        }
                    }
                    this.logger.LogInformation($"[用户{tcp.IP}:{tcp.Port}]将数据包放到重发队列中[队列数量：{models.Count}]，[电文号：{info.MessageNo}，长度：{info.PacketLength}，时间：{info.MessageDate}{info.MessageTime}]......");
                }
            }
        }
        await e.InvokeNext();
    }

    #endregion ITcpSendingPlugin接口
}

#endregion XCom29sPlugin插件

#region 数据解释

/// <summary>
/// iXCom29s协议适配器(MES协议)
/// </summary>
public class XCom29sFixedHeaderDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<XCom29sRequestInfo>
{
    #region CustomFixedHeaderDataHandlingAdapter接口

    /// <summary>
    /// 固定包头长度
    /// </summary>
    public override int HeaderLength => 29;

    /// <summary>
    /// 允许以对象发送
    /// </summary>
    public override bool CanSendRequestInfo => true;

    /// <summary>
    /// 获取新实例
    /// </summary>
    /// <returns></returns>
    protected override XCom29sRequestInfo GetInstance()
    {
        return new XCom29sRequestInfo();
    }

    /// <summary>
    /// 将对象转成字节
    /// </summary>
    /// <param name="requestInfo"></param>
    protected override void PreviewSend(IRequestInfo requestInfo)
    {
        if (requestInfo is XCom29sRequestInfo info && info != null)
        {
            byte[] data = info.BuildAsBytes();
            GoSend(data, 0, data.Length);
        }
    }

    #endregion CustomFixedHeaderDataHandlingAdapter接口
}

public class XCom29sRequestInfo : IFixedHeaderRequestInfo, IPacketContent, IXCom29sHeader
{
    #region 构造函数

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public XCom29sRequestInfo()
    {
        MessageNo = "999999";
        MessageDate = DateTime.Now.ToString("yyyyMMdd");
        MessageTime = DateTime.Now.ToString("HHmmss");
        SendCode = "";
        ReceiveCode = "";
        FunctionCode = "C";
        Header = null!;
        Content = null!;
        EndMark = 0x0a;
    }

    /// <summary>
    /// 心跳电文
    /// </summary>
    /// <param name="sendDescription"></param>
    /// <param name="receiveDescription"></param>
    /// <param name="functionCode"></param>
    public XCom29sRequestInfo(
        string sendDescription,
        string receiveDescription,
        string functionCode = "C"
    )
        : this()
    {
        SendCode = sendDescription;
        ReceiveCode = receiveDescription;
        FunctionCode = functionCode;
    }

    /// <summary>
    /// 应答电文
    /// </summary>
    /// <param name="messageNo"></param>
    /// <param name="sendDescription"></param>
    /// <param name="receiveDescription"></param>
    /// <param name="functionCode"></param>
    /// <param name="content"></param>
    public XCom29sRequestInfo(
        string messageNo,
        string sendDescription,
        string receiveDescription,
        string functionCode = "A",
        string content = ""
    )
        : this(sendDescription, receiveDescription, functionCode)
    {
        string s;
        MessageNo = messageNo;
        s = content.PadRight(80, ' ');
        if (s.Length > 0)
            s = s.Substring(0, 80);
        Content = Encoding.UTF8.GetBytes(s);
    }

    /// <summary>
    /// 普通电文
    /// </summary>
    /// <param name="messageNo"></param>
    /// <param name="sendDescription"></param>
    /// <param name="receiveDescription"></param>
    /// <param name="functionCode"></param>
    /// <param name="content"></param>
    public XCom29sRequestInfo(
        string messageNo,
        string sendDescription,
        string receiveDescription,
        string functionCode = "D",
        byte[] content = null!
    )
        : this(sendDescription, receiveDescription, functionCode)
    {
        MessageNo = messageNo;
        Content = content;
        EndMark = 0x0a;
    }

    /// <summary>
    /// 字节转换对象
    /// </summary>
    /// <param name="buffer"></param>
    public XCom29sRequestInfo(byte[] buffer)
        : this()
    {
        ConvertFromBytes(buffer);
    }

    #endregion 构造函数

    #region 常量字段

    /// <summary>
    /// 包最大长度
    /// </summary>
    private const int MaxPackLength = 4030;

    #endregion 常量字段

    #region 属性

    /// <summary>
    /// 报文状态
    /// </summary>
    public XCom29sResponseType ResponseType { get; set; } = XCom29sResponseType.Ack;

    #endregion 属性

    #region IPacketContent接口

    public int PacketLength { get; set; } = 0;

    public int HeaderLength { get; set; } = 0;

    public byte[] Header { get; set; } = null!;

    public int ContentLength { get; set; } = 0;

    public byte[] Content { get; set; } = null!;

    public byte EndMark { get; set; } = 0x0a;

    #endregion IPacketContent接口

    #region IXCom29sHeader接口

    public string MessageLength { get; set; } = string.Empty;

    public string MessageNo { get; set; } = string.Empty;

    public string MessageDate { get; set; } = string.Empty;

    public string MessageTime { get; set; } = string.Empty;

    public string SendCode { get; set; } = string.Empty;

    public string ReceiveCode { get; set; } = string.Empty;

    public string FunctionCode { get; set; } = string.Empty;

    #endregion IXCom29sHeader接口

    #region IFixedHeaderRequestInfo接口

    public int BodyLength { get; private set; } = 0;

    public bool OnParsingBody(byte[] body)
    {
        if (body?.Length >= BodyLength && HeaderLength > 0)
        {
            using (var byteBlock = new ByteBlock(body))
            {
                if (BodyLength - 1 > 0)
                {
                    Content = new byte[BodyLength - 1];
                    byteBlock.Read(Content, 0, BodyLength - 1);
                }

                if (body[^1] == 0x0a)
                {
                    EndMark = (byte)byteBlock.ReadByte();
                    BodyLength--; //去掉最后的字符长度
                    ResponseType = XCom29sResponseType.Ack;
                }
                else
                {
                    ResponseType = XCom29sResponseType.MissingTail;
                }

                if (PacketLength > MaxPackLength)
                {
                    ResponseType = XCom29sResponseType.LengthMismatch;
                }
            }

            return true;
        }

        return false;
    }

    public bool OnParsingHeader(byte[] header)
    {
        if (header?.Length >= 29)
        {
            using (var byteBlock = new ByteBlock(header))
            {
                int length = 0;
                byte[] data = new byte[header.Length];

                //保存包头
                Header = header;
                HeaderLength = 29;

                byteBlock.Position = 0; //重置到开始位置

                //电文长度C4
                byteBlock.Read(data, 0, 4);
                MessageLength = Encoding.UTF8.GetString(data, 0, 4);
                int.TryParse(MessageLength, NumberStyles.Integer, CultureInfo.InvariantCulture, out length);
                BodyLength = length - 29; //包体长度(设置接收剩下长度)
                PacketLength = length; //设置总包长度
                ContentLength = length - 30; //包体长度

                if (BodyLength < 0 || BodyLength > MaxPackLength)
                {
                    BodyLength = 0;     //标识结束接收本数据包
                    HeaderLength = 0;   //非法数据确认接收完成
                    ResponseType = XCom29sResponseType.NAck;
                    return true;
                }

                //电文号C6
                byteBlock.Read(data, 0, 6);
                MessageNo = Encoding.UTF8.GetString(data, 0, 6);

                //电文发送日期C8
                byteBlock.Read(data, 0, 8);
                MessageDate = Encoding.UTF8.GetString(data, 0, 8);

                //电文发送时间C6
                byteBlock.Read(data, 0, 6);
                MessageTime = Encoding.UTF8.GetString(data, 0, 6);

                //电文发送端描述码C2
                byteBlock.Read(data, 0, 2);
                SendCode = Encoding.UTF8.GetString(data, 0, 2);

                //电文接收端描述码C2
                byteBlock.Read(data, 0, 2);
                ReceiveCode = Encoding.UTF8.GetString(data, 0, 2);

                //电文功能描述码C1
                byteBlock.Read(data, 0, 1);
                FunctionCode = Encoding.UTF8.GetString(data, 0, 1);

                return true;
            }
        }

        return false;
    }

    #endregion IFixedHeaderRequestInfo接口

    #region 字节转换

    public void Build(ByteBlock byteBlock)
    {
        int length = 0;
        string s = "";

        //1.电文长度C4
        length = 29 + (Content == null ? 0 : Content.Length) + 1;
        s = string.Format("{0:D4}", length);
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //2.电文号C6
        s = MessageNo;
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //3.电文发送日期C8
        s = MessageDate;
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //4.电文发送时间C6
        s = MessageTime;
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //5.电文发送端描述码C2
        s = SendCode;
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //6.电文接收端描述码C2
        s = ReceiveCode;
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //7.电文功能描述码C1
        s = FunctionCode;
        byteBlock.Write(Encoding.UTF8.GetBytes(s));

        //8.电文内容
        if (Content != null)
            byteBlock.Write(Content);

        //9.电文结束符C1(0x0a)
        byteBlock.Write(EndMark);
    }

    public byte[] BuildAsBytes()
    {
        using (ByteBlock byteBlock = new ByteBlock())
        {
            Build(byteBlock);
            return byteBlock.ToArray();
        }
    }

    public void ConvertFromBytes(byte[] buffer)
    {
        if (buffer != null && buffer.Length >= 29)
        {
            using (var byteBlock = new ByteBlock(buffer))
            {
                int length = 0;
                byte[] data = new byte[buffer.Length];//分配读取空间

                #region 读取电文头部

                //保存包头
                Header = buffer;
                HeaderLength = 29;

                byteBlock.Position = 0; //重置到开始位置

                //电文长度C4
                byteBlock.Read(data, 0, 4);
                MessageLength = Encoding.UTF8.GetString(data, 0, 4);
                int.TryParse(MessageLength, NumberStyles.Integer, CultureInfo.InvariantCulture, out length);
                BodyLength = length - 29; //包体长度(设置接收剩下长度)
                PacketLength = length; //设置总包长度
                ContentLength = length - 30; //包体长度

                if (BodyLength < 0 || BodyLength > MaxPackLength)
                {
                    BodyLength = 0;     //标识结束接收本数据包
                    HeaderLength = 0;   //非法数据确认接收完成
                    ResponseType = XCom29sResponseType.NAck;
                    return;
                }

                //电文号C6
                byteBlock.Read(data, 0, 6);
                MessageNo = Encoding.UTF8.GetString(data, 0, 6);

                //电文发送日期C8
                byteBlock.Read(data, 0, 8);
                MessageDate = Encoding.UTF8.GetString(data, 0, 8);

                //电文发送时间C6
                byteBlock.Read(data, 0, 6);
                MessageTime = Encoding.UTF8.GetString(data, 0, 6);

                //电文发送端描述码C2
                byteBlock.Read(data, 0, 2);
                SendCode = Encoding.UTF8.GetString(data, 0, 2);

                //电文接收端描述码C2
                byteBlock.Read(data, 0, 2);
                ReceiveCode = Encoding.UTF8.GetString(data, 0, 2);

                //电文功能描述码C1
                byteBlock.Read(data, 0, 1);
                FunctionCode = Encoding.UTF8.GetString(data, 0, 1);

                #endregion 读取电文头部

                #region 读取电文体

                if (byteBlock.Length - byteBlock.Position >= BodyLength && HeaderLength > 0)
                {
                    if (BodyLength - 1 > 0)
                    {
                        Content = new byte[BodyLength - 1];
                        byteBlock.Read(Content, 0, BodyLength - 1);
                    }

                    if (buffer[^1] == 0x0a)
                    {
                        EndMark = (byte)byteBlock.ReadByte();
                        BodyLength--; //去掉最后的字符长度
                        ResponseType = XCom29sResponseType.Ack;
                    }
                    else
                    {
                        ResponseType = XCom29sResponseType.MissingTail;
                    }

                    if (PacketLength > MaxPackLength)
                    {
                        ResponseType = XCom29sResponseType.LengthMismatch;
                    }
                }

                #endregion 读取电文体
            }
        }
    }

    #endregion 字节转换
}

/// <summary>
/// 数据包头内容
/// </summary>
public interface IXCom29sHeader
{
    #region 电文包头格式

    /// <summary>
    /// 电文长度C4  (如：00100)
    /// </summary>
    public string MessageLength { get; set; }

    /// <summary>
    /// 电文号C6   (如：M1S101)
    /// </summary>
    public string MessageNo { get; set; }

    /// <summary>
    /// 电文发送日期C8    (如：20220801)
    /// </summary>
    public string MessageDate { get; set; }

    /// <summary>
    /// 电文发送时间C6    (如：121501)
    /// </summary>
    public string MessageTime { get; set; }

    /// <summary>
    /// 发送端描述码C2    (如：M1)
    /// </summary>
    public string SendCode { get; set; }

    /// <summary>
    /// 接收端描述码C2    (如：S1)
    /// </summary>
    public string ReceiveCode { get; set; }

    /// <summary>
    /// 功能码C1           (如：D表示发送数据,A表示电文传送成功，B表示电文传送出错，C表示心跳电文传送)
    /// </summary>
    public string FunctionCode { get; set; }

    #endregion 电文包头格式
}

/// <summary>
/// 数据包整体内容
/// </summary>
public interface IPacketContent
{
    #region 电文内容

    /// <summary>
    /// 电文总长度
    /// </summary>
    public int PacketLength { get; set; }

    /// <summary>
    /// 电文头长度
    /// </summary>
    public int HeaderLength { get; set; }

    /// <summary>
    /// 电文头内容
    /// </summary>
    public byte[] Header { get; set; }

    /// <summary>
    /// 电文内容长度
    /// </summary>
    public int ContentLength { get; set; }

    /// <summary>
    /// 电文内容
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// 电文结尾标识符
    /// </summary>
    public byte EndMark { get; set; }

    #endregion 电文内容
}

public enum XCom29sResponseType : byte
{
    /// <summary>
    /// 电文正确
    /// </summary>
    Ack = 0x01,

    /// <summary>
    /// 电文不正确
    /// </summary>
    NAck = 0x02,

    /// <summary>
    /// 缺少电文标识符
    /// </summary>
    MissingTail = 0x04,

    /// <summary>
    /// 长度不符
    /// </summary>
    LengthMismatch = 0x08
}

#endregion 数据解释

#region 依赖扩展

public static class DependencyExtensions
{
    public static readonly DependencyProperty<Timer> ResendTimerProperty = DependencyProperty<Timer>.Register("ResendTimer", null);

    public static readonly DependencyProperty<Dictionary<string, ResendQueueModel>> ResendQueueProperty = DependencyProperty<Dictionary<string, ResendQueueModel>>.Register("ResendQueue", null);

    /// <summary>
    /// 扩展方式发送心跳(会将传入对象时间字段刷新)
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="requestInfo"></param>
    /// <returns></returns>
    public static bool Ping<TClient>(this TClient client, IRequestInfo requestInfo) where TClient : ITcpClientBase
    {
        try
        {
            if (requestInfo != null && requestInfo is XCom29sRequestInfo info && info != null)
            {
                //Console.WriteLine($"[{client.IP}:{client.Port}]正在发送心跳包......");
                info.MessageDate = DateTime.Now.ToString("yyyyMMdd");
                info.MessageTime = DateTime.Now.ToString("HHmmss");
                client.Send(info);
                return true;
            }
        }
        catch (Exception ex)
        {
            client.Logger.Exception(ex);
        }
        return false;
    }

    /// <summary>
    /// 扩展方式应答
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="requestInfo"></param>
    /// <returns></returns>
    public static bool Pong<TClient>(this TClient client, IRequestInfo requestInfo) where TClient : ITcpClientBase
    {
        try
        {
            if (requestInfo != null && requestInfo is XCom29sRequestInfo info && info != null)
            {
                //Console.WriteLine($"[服务端{client.IP}:{client.Port}]应答心跳包......");
                client.Send(info);
            }
            return true;
        }
        catch (Exception ex)
        {
            client.Logger.Exception(ex);
        }

        return false;
    }

    /// <summary>
    /// 指令校验函数
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="requestInfo"></param>
    /// <returns></returns>
    public static int CheckCommandVerifications<TClient>(this TClient client, IRequestInfo requestInfo) where TClient : ITcpClientBase
    {
        int result = 1; //0表示指令校验成功,1表示指令不支持,2表示长度不相符
        try
        {
            if (client != null && client is ISocketClient socket && socket != null
                && socket.Service is ICommServiceSocket server && server != null
                && requestInfo != null && requestInfo is XCom29sRequestInfo info && info != null
                && server.CommandVerifications != null && server.CommandVerifications.Count > 0)
            {
                var flag = server.CommandVerifications.TryGetValue(info.MessageNo.ToUpper(), out var command);
                if (flag)
                {
                    result = command != null && command.TelegramLength == info.ContentLength ? 0 : 2;//校验成功或长度不相符
                }
            }
        }
        catch (Exception ex)
        {
            client.Logger.Exception(ex);
        }

        return result;
    }
}

#endregion 依赖扩展

#region 队列对象

/// <summary>
/// 重发对象类
/// </summary>
public class ResendQueueModel : IDisposable
{
    /// <summary>
    /// 数据包对象
    /// </summary>
    public IRequestInfo Request { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    public DateTime TimeOut { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; }

    /// <summary>
    /// 发送状态(0表示未确认,1表示已确认,2表示超时)
    /// </summary>
    public int State { get; set; }

    public void Dispose()
    {
        Request = null;
    }
}

#endregion 队列对象
