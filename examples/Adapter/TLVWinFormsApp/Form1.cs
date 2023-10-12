using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TLVWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void ShowMsg(string msg)
        {
            this.listBox1.Items.Insert(0, msg);
        }

        private readonly TcpService m_tcpService = new TcpService();

        private void button1_Click(object sender, EventArgs e)
        {
            //订阅收到消息事件
            this.m_tcpService.Received = (client,e) =>
            {
                if (e.RequestInfo is TLVDataFrame frame)
                {
                    client.Logger.Info($"服务器收到,Tag={frame.Tag},Len={frame.Length},Value={(frame.Value != null ? Encoding.UTF8.GetString(frame.Value) : string.Empty)}");
                }
                return EasyTask.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger(new EasyLogger(this.ShowMsg));
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>()//使用插件，相当于自动设置适配器，并且主动回应Ping。
                       .SetLengthType(FixedHeaderType.Int);//设置支持的最大数据类型，该值还受SetMaxPackageSize影响。
                });

            //载入配置
            this.m_tcpService.Setup(config);

            //启动
            this.m_tcpService.Start();
            this.m_tcpService.Logger.Info("服务器成功启动。");
        }

        private readonly TcpClient m_client = new TcpClient();

        private void button2_Click(object sender, EventArgs e)
        {
            this.m_client.Setup(new TouchSocketConfig()
                  .SetMaxPackageSize(1024 * 1024 * 10)
                  .ConfigureContainer(a =>
                  {
                      a.SetSingletonLogger(new EasyLogger(this.ShowMsg));
                  })
                  //.SetDataHandlingAdapter(() => new TLVDataHandlingAdapter(FixedHeaderType.Int, verifyFunc: null))//如果使用TLVPlugin插件，此步骤可省略。
                  .ConfigurePlugins(a =>
                  {
                      a.Add<TLVPlugin>()//使用插件，相当于自动设置适配器，并且主动回应Ping。
                      .SetLengthType(FixedHeaderType.Int);//设置支持的最大数据类型，该值还受SetMaxPackageSize影响。
                  })
                  .SetRemoteIPHost(new IPHost("127.0.0.1:7789")));
            this.m_client.Connect();

            this.m_client.Logger.Info("连接成功");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_client?.Send(new ValueTLVDataFrame((ushort)this.numericUpDown1.Value, Encoding.UTF8.GetBytes(this.textBox1.Text)));
            }
            catch (Exception ex)
            {
                this.m_client.Logger.Exception(ex);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_client.Logger.Info($"ping={this.m_client?.PingWithTLV()}");
            }
            catch (Exception ex)
            {
                this.m_client.Logger.Exception(ex);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    this.m_client?.Send(new ValueTLVDataFrame((ushort)this.numericUpDown1.Value, Encoding.UTF8.GetBytes(i.ToString())));
                }
                catch (Exception ex)
                {
                    this.m_client?.Logger.Exception(ex);
                }
            }
        }
    }
}