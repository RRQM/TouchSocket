using RRQMCore.ByteManager;
using RRQMCore.Log;
using RRQMSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Load += this.Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs args)
        {
            m_tcpClient.Connected += (client, e) => { };//成功连接到服务器
            m_tcpClient.Disconnected += (client, e) => { };//从服务器断开连接，当连接不成功时不会触发。
            m_tcpClient.Received += this.TcpClient_Received;
        }

        private void ShowMsg(string msg)
        {
            this.textBox1.AppendText(msg);
            this.textBox1.AppendText("\r\n");
        }

        private void TcpClient_Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            client.Logger.Message($"从服务器收到消息：{Encoding.UTF8.GetString(byteBlock.ToArray())}");//utf8解码。
        }

        TcpClient m_tcpClient = new TcpClient();

        private void button1_Click(object sender, EventArgs args)
        {
            //声明配置
            RRQMConfig config = new RRQMConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .UsePlugin()
                .SetBufferLength(1024 * 10)
                .SetSingletonLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger()));

            //载入配置
            m_tcpClient.Setup(config);
            m_tcpClient.Connect();
            m_tcpClient.Logger.Message("成功连接");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_tcpClient.Send(this.textBox2.Text);
            }
            catch (Exception ex)
            {
                this.m_tcpClient.Logger.Exception(ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //调用GetWaitingClient获取到IWaitingClient的对象。该对象会复用。
                //然后使用SendThenReturn。
                //同时，如果适配器收到数据后，返回的并不是字节，而是IRequestInfo对象时，可以使用SendThenResponse

                byte[] returnData = m_tcpClient.GetWaitingClient(WaitingOptions.AllAdapter).SendThenReturn(Encoding.UTF8.GetBytes(textBox2.Text));
                this.m_tcpClient.Logger.Message($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");
            }
            catch (TimeoutException)
            {
                this.m_tcpClient.Logger.Error("等待超时");
            }
            catch (Exception ex)
            {
                this.m_tcpClient.Logger.Exception(ex);
            }
        }
    }
}
