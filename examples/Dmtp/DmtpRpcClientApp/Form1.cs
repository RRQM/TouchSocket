using RpcProxy;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace DmtpClientApp
{
    public partial class Form1 : Form
    {
        private TcpDmtpClient client = new TcpDmtpClient();

        public Form1()
        {
            this.InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //直接调用时，第一个参数为调用键，服务类全名+方法名（必须全小写）
            //第二个参数为调用配置参数，可设置调用超时时间，取消调用等功能。
            //后续参数为调用参数。
            var result = this.client.GetDmtpRpcActor().InvokeT<bool>("Login", InvokeOption.WaitInvoke, this.textBox1.Text, this.textBox2.Text);
            MessageBox.Show(result.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var myRpcServer = new RpcProxy.MyRpcServer(this.client.GetDmtpRpcActor());//MyRpcServer类是由代码工具生成的类。

            //代理调用时，基本和本地调用一样。只是会多一个调用配置参数。
            var result = myRpcServer.Login(this.textBox1.Text, this.textBox2.Text, InvokeOption.WaitInvoke);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //扩展调用时，首先要保证本地已有代理文件，然后调用和和本地调用一样。只是会多一个调用配置参数。
            var result = this.client.GetDmtpRpcActor().Login(this.textBox1.Text, this.textBox2.Text, InvokeOption.WaitInvoke);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();

                    //使用心跳保活，或者避免异常连接。达到最大失败次数会断开，不会重连。
                    a.UseDmtpHeartbeat()
                    .SetTick(TimeSpan.FromSeconds(3))
                    .SetMaxFailCount(3);

                    //使用重连
                    a.UseReconnection<TcpDmtpClient>()
                    .SetActionForCheck(async (c, i) =>//重新定义检活策略
                    {
                        //方法1，直接判断是否在握手状态。使用该方式，最好和心跳插件配合使用
                        //await Task.CompletedTask;//消除Task
                        //return c.IsHandshaked;//判断是否在握手状态

                        //方法2，直接ping，如果true，则客户端必在线。如果false，则客户端不一定不在线，原因是可能当前传输正在忙
                        if (await c.PingAsync())
                        {
                            return true;
                        }
                        //返回false时可以判断，如果最近活动时间不超过3秒，则猜测客户端确实在忙，所以跳过本次重连
                        else if (DateTime.Now- c.GetLastActiveTime()<TimeSpan.FromSeconds(3))
                        {
                            return null;
                        }
                        //否则，直接重连。
                        else
                        {
                            return false;
                        }
                    })
                    .SetTick(TimeSpan.FromSeconds(3))
                    .UsePolling();

                })
                .SetVerifyToken("Rpc"));
            this.client.Connect();
        }
    }
}