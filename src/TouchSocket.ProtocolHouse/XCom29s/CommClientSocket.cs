using Microsoft.Extensions.Logging;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.ProtocolHouse;

public class CommClientSocket<T> : TcpClient, ICommClientSocket where T : class, IPlugin
{
    #region 私有字段

    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<CommClientSocket<T>> logger;

    /// <summary>
    /// 是否初始化
    /// </summary>
    private bool IsInitable = false;

    #endregion 私有字段

    #region 构造函数

    public CommClientSocket(ILogger<CommClientSocket<T>> logger)
    {
        this.logger = logger;

        #region 关联网络事件

        //成功连接到服务器
        Connected += (client, e) =>
        {
            HeartBeatTimeCount = 0;
            return EasyTask.CompletedTask;
        };

        //从服务器断开连接，当连接不成功时不会触发。
        Disconnected += (client, e) =>
        {
            return EasyTask.CompletedTask;
        };

        //从服务器收到信息
        Received += (client, e) =>
        {
            return EasyTask.CompletedTask;
        };

        #endregion 关联网络事件
    }

    #endregion 构造函数

    #region ICommClientSocket接口属性

    public string Host { get; set; } = "127.0.0.1:8087";

    public int HeartBeatTime { get; set; } = 0;

    public int HeartBeatTimeCount { get; set; } = 0;

    public Action<TcpClientBase>? SendHeartBeat { get; set; } = null;

    public IRequestInfo HeartBeatInfo { get; set; } = null!;

    public bool IsStoppable { get; set; } = false;

    public bool NeedConfirm { get; set; } = true;

    #endregion ICommClientSocket接口属性

    #region ICommClientSocket接口方法

    public bool StartConnect(string host = "", int heartBeatTime = 30, int usePollingTime = 5, int checkClearTime = 60 * 3)
    {
        bool result = false;

        this.Host = !string.IsNullOrEmpty(host) ? host : this.Host;
        this.HeartBeatTimeCount = 0;
        this.HeartBeatTime = heartBeatTime;
        if (IsInitable)
        {
            IsStoppable = false; //设置为不暂停连接
            return result;
        }
        try
        {
            var config = new TouchSocketConfig()
            .SetRemoteIPHost(Host)
            .ConfigurePlugins(a =>
            {
                //检查连接客户端活性插件
                if (checkClearTime > 0)
                    a.UseCheckClear().SetTick(TimeSpan.FromSeconds(checkClearTime));

                //使用轮询插件
                if (usePollingTime > 0)
                {
                    a.UseReconnection(TimeSpan.FromSeconds(usePollingTime), (client, n, ex) =>
                    {
                        this.logger.LogTrace($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}]连接服务器[{RemoteIPHost.EndPoint.ToString()}]第 {n} 次失败，错误信息[{ex.Message}]......");
                        return !IsStoppable;//返回false时会退出重连循环
                    }, null!)
                    .UsePolling()
                    .SetActionForCheck(async (client, failCount) =>
                    {
                        bool? result = false;
                        result = IsStoppable || (bool)(client?.Online);//返回true不会再次连接
                        if (IsStoppable) await Task.Delay(10);
                        if ((bool)client?.Online && HeartBeatTime > 0)
                        {
                            HeartBeatTimeCount++;
                            if (HeartBeatTimeCount >= HeartBeatTime)
                            {
                                HeartBeatTimeCount = 0;

                                //委托方式发心跳
                                SendHeartBeat?.Invoke(this);

                                //扩展方式发送心跳
                                if (HeartBeatInfo != null)
                                    client.Ping(HeartBeatInfo);
                            }
                        }
                        return await Task.FromResult(result);
                    });

                    //a.UseSocketReconnection(this, TimeSpan.FromSeconds(usePollingTime), (client, n, ex) =>
                    //{
                    //    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}]连接服务器[{RemoteIPHost.EndPoint.ToString()}]第 {n} 次失败，错误信息[{ex.Message}]......");
                    //    return !IsStoppable;//返回false时会退出重连循环
                    //}, null!);
                }
                //添加MES处理插件
                a.Add<T>();
            })
            .ConfigureContainer(a =>
            {
                //a.AddLogger(new MyNLogLogger());//注册NLog日志
            });

            this.Setup(config);

            HeartBeatTimeCount = 0;
            var res = this.ConnectAsync();   //会触发异常
            IsStoppable = false; //设置为不暂停连接
            IsInitable = true;  //设置为初始化
            result = true;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}]连接服务器失败，请重新设置[{ex.Message}]......");
            this.TryShutdown();
            Close();
            HeartBeatTimeCount = 0;
        }

        return result;
    }

    public void StopConnect()
    {
        IsStoppable = true; //设置为暂停连接

        //断开连接
        if (this.Online)
        {
            try
            {
                this.TryShutdown();
                Close();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}]断开服务器[{RemoteIPHost.EndPoint.ToString()}]时出错[{ex.Message}]......");
            }
        }
        else
            this.logger.LogInformation($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}]已经断开服务器[{RemoteIPHost.EndPoint.ToString()}]......");
        HeartBeatTimeCount = 0;
    }

    public void SendData(IRequestInfo requestInfo)
    {
        this.Send(requestInfo);
    }

    #endregion ICommClientSocket接口方法
}