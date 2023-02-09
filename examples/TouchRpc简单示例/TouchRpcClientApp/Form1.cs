using RpcProxy;
using System;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace TouchRpcClientApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();
            //直接调用时，第一个参数为调用键，服务类全名+方法名（必须全小写）
            //第二个参数为调用配置参数，可设置调用超时时间，取消调用等功能。
            //后续参数为调用参数。
            bool result = client.Invoke<bool>("touchrpcserverapp.myrpcserver.login", InvokeOption.WaitInvoke, textBox1.Text, textBox2.Text);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            MyRpcServer myRpcServer = new MyRpcServer(client);//MyRpcServer类是由代码工具生成的类。

            //代理调用时，基本和本地调用一样。只是会多一个调用配置参数。
            bool result = myRpcServer.Login(textBox1.Text, textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();

            client.TryCanInvoke = (c) =>
            {
                if (client.IsHandshaked)
                {
                    return true;
                }
                else
                {
                    try
                    {
                        client.Connect();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        client.Logger.Exception(ex);
                        return false;
                    }
                }
            };

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            //扩展调用时，首先要保证本地已有代理文件，然后调用和和本地调用一样。只是会多一个调用配置参数。
            bool result = client.Login(textBox1.Text, textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UdpTouchRpc client = new UdpTouchRpc();
            client.Setup(new TouchSocketConfig()
                .SetBindIPHost(7794)
                .SetRemoteIPHost("127.0.0.1:7791"));//设置目标地址。
            client.Start();

            bool result = client.Invoke<bool>("touchrpcserverapp.myrpcserver.login", InvokeOption.WaitInvoke, textBox1.Text, textBox2.Text);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client可以复用，但在此处直接释放。
        }

        private void button6_Click(object sender, EventArgs e)
        {
            UdpTouchRpc client = new UdpTouchRpc();
            client.Setup(new TouchSocketConfig()
                .SetBindIPHost(7794)
                .SetRemoteIPHost("127.0.0.1:7791"));//设置目标地址。
            client.Start();

            MyRpcServer myRpcServer = new MyRpcServer(client);//MyRpcServer类是由代码工具生成的类。

            //代理调用时，基本和本地调用一样。只是会多一个调用配置参数。
            bool result = myRpcServer.Login(textBox1.Text, textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client可以复用，但在此处直接释放。
        }

        private void button5_Click(object sender, EventArgs e)
        {
            UdpTouchRpc client = new UdpTouchRpc();
            client.Setup(new TouchSocketConfig()
                .SetBindIPHost(7794)
                .SetRemoteIPHost("127.0.0.1:7791"));//设置目标地址。
            client.Start();

            //扩展调用时，首先要保证本地已有代理文件，然后调用和和本地调用一样。只是会多一个调用配置参数。
            bool result = client.Login(textBox1.Text, textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client可以复用，但在此处直接释放。
        }

        private void button7_Click(object sender, EventArgs e)
        {
            HttpTouchRpcClient client = new HttpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7790")
               .SetVerifyToken("TouchRpc"));
            client.Connect();
            //直接调用时，第一个参数为调用键，服务类全名+方法名（必须全小写）
            //第二个参数为调用配置参数，可设置调用超时时间，取消调用等功能。
            //后续参数为调用参数。
            bool result = client.Invoke<bool>("touchrpcserverapp.myrpcserver.login", InvokeOption.WaitInvoke, textBox1.Text, textBox2.Text);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button9_Click(object sender, EventArgs e)
        {
            HttpTouchRpcClient client = new HttpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7790")
                .SetVerifyToken("TouchRpc"));
            client.Connect();
            MyRpcServer myRpcServer = new MyRpcServer(client);//MyRpcServer类是由代码工具生成的类。

            //代理调用时，基本和本地调用一样。只是会多一个调用配置参数。
            bool result = myRpcServer.Login(textBox1.Text, textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button8_Click(object sender, EventArgs e)
        {
            HttpTouchRpcClient client = new HttpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7790")
               .SetVerifyToken("TouchRpc"));
            client.Connect();
            //扩展调用时，首先要保证本地已有代理文件，然后调用和和本地调用一样。只是会多一个调用配置参数。
            bool result = client.Login(textBox1.Text, textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button10_Click(object sender, EventArgs e)
        {
            HttpTouchRpcClient client = new HttpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7790")
               .SetVerifyToken("TouchRpc"));
            client.Connect();

            string key = "touchrpcserverapp.myrpcserver.performance";

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    client.Invoke<int>(key, null, i);
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            MessageBox.Show(timeSpan.ToString());

            client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //try
            //{
            //    Enterprise.ForTest();
            //}
            //catch
            //{

              
            //}
        }
    }
}