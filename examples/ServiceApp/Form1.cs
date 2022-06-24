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

namespace ServiceApp
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
            m_service.Connecting += (client, e) => 
            {
                e.ID = $"{client.IP}:{client.Port}";
            };//有客户端正在连接
            m_service.Connected += this.M_service_Connected;//有客户端连接
            m_service.Disconnected += this.M_service_Disconnected; ;//有客户端断开连接
            m_service.Received += this.Service_Received;
        }

        private void ShowMsg(string msg)
        {
            this.textBox1.AppendText(msg);
            this.textBox1.AppendText("\r\n");
        }

        TcpService m_service = new TcpService();

        private void button1_Click(object sender, EventArgs ergs)
        {
            m_service.Setup(new RRQMConfig()//载入配置     
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetMaxCount(10000)
                .SetThreadCount(10)
                .SetSingletonLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger())))
                .Start();//启动
            m_service.Logger.Message("服务器成功启动");
        }

        private void M_service_Disconnected(SocketClient client, ClientDisconnectedEventArgs e)
        {
            this.listBox1.Items.Remove(client.ID);
        }

        private void M_service_Connected(SocketClient client, RRQMCore.RRQMEventArgs e)
        {
            this.listBox1.Items.Add(client.ID);
        }

        private void Service_Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            client.Logger.Message($"从客户端id={client.ID}，ip={client.IP}，port={client.Port}收到消息：{Encoding.UTF8.GetString(byteBlock.ToArray())}");//utf8解码。
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem is string id)
            {
                this.m_service.Send(id, textBox2.Text);
            }
            else
            {
                this.m_service.Logger.Warning("请先选择一个客户端。");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //先找到与远程TcpClient对应的SocketClient
            if (this.listBox1.SelectedItem is string id && this.m_service.TryGetSocketClient(id, out SocketClient client))
            {
                try
                {
                    //然后调用GetWaitingClient获取到IWaitingClient的对象。该对象会复用。
                    byte[] returnData = client.GetWaitingClient(WaitingOptions.AllAdapter).SendThenReturn(Encoding.UTF8.GetBytes(textBox2.Text));
                    this.m_service.Logger.Message($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");
                }
                catch (TimeoutException)
                {
                    this.m_service.Logger.Error("等待超时");
                }
                catch(Exception ex)
                {
                    this.m_service.Logger.Exception(ex);
                }
            }
            else
            {
                this.m_service.Logger.Warning("请先选择一个客户端。");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //先找到与远程TcpClient对应的SocketClient
            if (this.listBox1.SelectedItem is string id && this.m_service.TryGetSocketClient(id, out SocketClient client))
            {
                client.SafeDispose();
            }
            else
            {
                this.m_service.Logger.Warning("请先选择一个客户端。");
            }
        }
    }
}
