using Microsoft.Extensions.Logging;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.ProtocolHouse;

public class CommServiceSocket<T> : TcpService, ICommServiceSocket where T : class, IPlugin
{
    #region 私有字段

    private readonly ILogger<CommServiceSocket<T>> logger;

    /// <summary>
    /// 是否初始化
    /// </summary>
    private bool IsInitable = false;

    #endregion 私有字段

    #region 构造函数

    public CommServiceSocket(ILogger<CommServiceSocket<T>> logger)
    {
        this.logger = logger;

        #region 关联网络事件

        //成功连接到服务器
        Connected += (client, e) =>
        {
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

    #region ICommServiceSocket接口属性

    public string Host { get; set; } = "0.0.0.0:8087";

    public int CheckClearTime { get; set; } = 60 * 3;

    public Dictionary<string, TelegramListDto> CommandVerifications { get; set; } = new();

    public ReceivedEventValueHandler<ITcpClientBase> DataReceived { get; set; } = null!;

    #endregion ICommServiceSocket接口属性

    #region ICommServiceSocket接口方法

    public bool StartService(string host = "")
    {
        bool result = false;

        this.Host = !string.IsNullOrEmpty(host) ? host : this.Host;
        if (ServerState != ServerState.Running)
        {
            try
            {
                var config = new TouchSocketConfig()
                    .SetListenIPHosts(new IPHost[] { new IPHost(Host) })
                    .ConfigurePlugins(a =>
                    {
                        //检查连接客户端活性插件
                        if (CheckClearTime > 0)
                            a.UseCheckClear().SetTick(TimeSpan.FromSeconds(CheckClearTime));
                        a.Add<T>();
                    })
                    .ConfigureContainer(a =>
                    {
                        //a.AddLogger(new MyNLogLogger());//注册NLog日志
                    });

                this.Setup(config);
                this.Start();//启动
                this.IsInitable = true;//设置初始化
                this.logger.LogInformation($"服务器监听地址端口[{this.Host}]成功......");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"服务器监听地址端口[{this.Host}]失败[{ex.Message}]......");
            }
        }
        else
        {
            this.logger.LogInformation($"服务器已经启动，已经正在监听地址端口[{this.Host}]......");
        }

        return result;
    }

    public bool StopService()
    {
        bool result = false;
        try
        {
            if (this?.ServerState == ServerState.Running)
            {
                if (SocketClients != null && SocketClients.Count > 0)
                {
                    var cs = SocketClients.GetClients().ToList();
                    for (int i = 0; i < cs.Count; i++)
                    {
                        cs[i].Close();
                        cs[i].Dispose();
                        cs[i] = null;
                    }
                }
                this?.Stop();
                result = true;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError($"服务器已经停止监听地址端口[{Host}]失败[{ex.Message}]......");
            return false;
        }
        return result;
    }

    #endregion ICommServiceSocket接口方法
}