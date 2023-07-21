using RpcProxy;
using System;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchRpcClientApp
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
            this.client.SafeDispose();//client是长连接，可以复用，但在此处使用短连接。
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var myRpcServer = new RpcProxy.MyRpcServer(this.client.GetDmtpRpcActor());//MyRpcServer类是由代码工具生成的类。

            //代理调用时，基本和本地调用一样。只是会多一个调用配置参数。
            var result = myRpcServer.Login(this.textBox1.Text, this.textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //扩展调用时，首先要保证本地已有代理文件，然后调用和和本地调用一样。只是会多一个调用配置参数。
            var result = this.client.GetDmtpRpcActor().Login(this.textBox1.Text, this.textBox2.Text, InvokeOption.WaitInvoke);
            MessageBox.Show(result.ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetVerifyToken("Rpc"));
            this.client.Connect();
        }
    }
}