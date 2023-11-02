using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AccessRestrictionsConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// 实现黑白名单功能，博客<see href="https://blog.csdn.net/qq_40374647/article/details/128640132"/>
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var service = new TcpService();
            service.Received = (client, e) =>
            {
                //从客户端收到信息
                var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
                return Task.CompletedTask;
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）

                    //注册访问限制实例，AccessRestrictions可自行实现，例如连接数据库做持久化等。
                    a.RegisterSingleton<IAccessRestrictions, AccessRestrictions>();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<AccessRestrictionsPlugin>();//添加访问限制插件
                }));

            service.Start();//启动

            service.Logger.Info("服务器成功启动");
            Console.ReadKey();
        }
    }

    public class AccessRestrictionsPlugin : PluginBase, ITcpConnectingPlugin<ITcpClientBase>
    {
        private readonly IAccessRestrictions accessRestrictions;

        public AccessRestrictionsPlugin(IAccessRestrictions accessRestrictions)
        {
            this.accessRestrictions = accessRestrictions ?? throw new ArgumentNullException(nameof(accessRestrictions));
        }

        public Task OnTcpConnecting(ITcpClientBase client, ConnectingEventArgs e)
        {
            if (client.IsClient)
            {
                //此处判断，如果该插件被添加在客户端，则不工作。
                return e.InvokeNext();
            }
            if (this.accessRestrictions.ExistsWhiteList(client.IP))
            {
                //如果存在于白名单，直接返回，允许连接
                return e.InvokeNext();
            }
            if (this.accessRestrictions.ExistsBlackList(client.IP))
            {
                //如果存在于黑名单，不允许连接
                e.IsPermitOperation = false;
                e.Handled = true;//表示此处已经处理OnConnecting消息，其他插件不再路由投递。
                return Task.CompletedTask;
            }

            return e.InvokeNext();
        }
    }

    public interface IAccessRestrictions
    {
        bool AddWhiteList(string ip);

        bool AddBlackList(string ip);

        bool RemoveWhiteList(string ip);

        bool RemoveBlackList(string ip);

        bool ExistsWhiteList(string ip);

        bool ExistsBlackList(string ip);
    }

    public class AccessRestrictions : IAccessRestrictions
    {
        private readonly List<string> m_whiteListIP = new List<string>();
        private readonly List<string> m_blackListIP = new List<string>();

        public virtual bool AddBlackList(string ip)
        {
            if (this.m_blackListIP.Contains(ip))
            {
                return true;
            }
            this.m_blackListIP.Add(ip);
            return true;
        }

        public virtual bool AddWhiteList(string ip)
        {
            if (this.m_whiteListIP.Contains(ip))
            {
                return true;
            }
            this.m_whiteListIP.Add(ip);
            return true;
        }

        public virtual bool ExistsBlackList(string ip)
        {
            //实际上此处也可以用正则表达式
            return this.m_blackListIP.Contains(ip);
        }

        public virtual bool ExistsWhiteList(string ip)
        {
            //实际上此处也可以用正则表达式
            return this.m_whiteListIP.Contains(ip);
        }

        public virtual bool RemoveBlackList(string ip)
        {
            return this.m_blackListIP.Remove(ip);
        }

        public virtual bool RemoveWhiteList(string ip)
        {
            return this.m_whiteListIP.Remove(ip);
        }
    }
}