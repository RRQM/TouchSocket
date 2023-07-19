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
        static void Main(string[] args)
        {
            TcpService service = new TcpService();
            service.Received = (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                client.Logger.Info($"已从{client.ID}接收到信息：{mes}");
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost("tcp://127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                    a.RegisterSingleton<IAccessRestrictions, AccessRestrictions>();//注册访问限制实例，AccessRestrictions可自行实现，例如连接数据库做持久化等。
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<AccessRestrictionsPlugin>();//添加访问限制插件
                }))
                .Start();//启动

            service.Logger.Info("服务器成功启动");
            Console.ReadKey();
        }
    }

    public class AccessRestrictionsPlugin : TcpPluginBase
    {
        private readonly IAccessRestrictions accessRestrictions;

        public AccessRestrictionsPlugin(IAccessRestrictions accessRestrictions)
        {
            this.accessRestrictions = accessRestrictions ?? throw new ArgumentNullException(nameof(accessRestrictions));
        }
        protected override void OnConnecting(ITcpClientBase client, OperationEventArgs e)
        {
            if (client is ITcpClient)
            {
                //此处判断，如果该插件被添加在客户端，则不工作。
                return;
            }
            if (this.accessRestrictions.ExistsWhiteList(client.IP))
            {
                //如果存在于白名单，直接返回，允许连接
                return;
            }
            if (this.accessRestrictions.ExistsBlackList(client.IP))
            {
                //如果存在于黑名单，不允许连接
                e.IsPermitOperation = false;
                e.Handled = true;//表示此处已经处理OnConnecting消息，其他插件不再路由投递。
                return;
            }
            base.OnConnecting(client, e);
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
        readonly List<string> whiteListIP = new List<string>();
        readonly List<string> blackListIP = new List<string>();
        public virtual bool AddBlackList(string ip)
        {
            if (blackListIP.Contains(ip))
            {
                return true;
            }
            blackListIP.Add(ip);
            return true;
        }

        public virtual bool AddWhiteList(string ip)
        {
            if (whiteListIP.Contains(ip))
            {
                return true;
            }
            whiteListIP.Add(ip);
            return true;
        }

        public virtual bool ExistsBlackList(string ip)
        {
            //实际上此处也可以用正则表达式
            return this.blackListIP.Contains(ip);
        }

        public virtual bool ExistsWhiteList(string ip)
        {
            //实际上此处也可以用正则表达式
            return this.whiteListIP.Contains(ip);
        }

        public virtual bool RemoveBlackList(string ip)
        {
            return this.blackListIP.Remove(ip);
        }

        public virtual bool RemoveWhiteList(string ip)
        {
            return this.whiteListIP.Remove(ip);
        }
    }
}