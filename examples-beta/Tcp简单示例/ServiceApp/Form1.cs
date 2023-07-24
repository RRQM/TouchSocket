using System;
using System.Text;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ServiceApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Load += this.Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs args)
        {
            this.m_service.Connecting = (client, e) =>
            {
                e.Id = $"{client.IP}:{client.Port}";
            };//有客户端正在连接
            this.m_service.Connected = this.M_service_Connected;//有客户端连接
            this.m_service.Disconnected = this.M_service_Disconnected; ;//有客户端断开连接
            this.m_service.Received = this.Service_Received;
        }

        private void ShowMsg(string msg)
        {
            this.textBox1.AppendText(msg);
            this.textBox1.AppendText("\r\n");
        }

        private TcpService m_service = new TcpService();

        private void button1_Click(object sender, EventArgs ergs)
        {
            this.m_service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetMaxCount(1000)
                .ConfigureContainer(a =>
                {
                    a.AddFileLogger();
                    a.AddEasyLogger(this.ShowMsg);
                })
                .ConfigurePlugins(a =>
                {
                    a.UseCheckClear()
                    .SetCheckClearType(CheckClearType.All)
                    .SetTick(TimeSpan.FromSeconds(60));
                }))
                .Start();//启动
            this.m_service.Logger.Info("服务器成功启动");
        }

        private void M_service_Disconnected(SocketClient client, DisconnectEventArgs e)
        {
            this.listBox1.Items.Remove(client.Id);
        }

        private void M_service_Connected(SocketClient client, TouchSocketEventArgs e)
        {
            this.listBox1.Items.Add(client.Id);
        }

        private void Service_Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            client.Logger.Info($"从客户端id={client.Id}，ip={client.IP}，port={client.Port}收到消息：{Encoding.UTF8.GetString(byteBlock.ToArray())}");//utf8解码。

            client.Send("Accepted");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem is string id)
            {
                this.m_service.Send(id, this.textBox2.Text);
            }
            else
            {
                this.m_service.Logger.Warning("请先选择一个客户端。");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //先找到与远程TcpClient对应的SocketClient
            if (this.listBox1.SelectedItem is string id && this.m_service.TryGetSocketClient(id, out var client))
            {
                try
                {
                    //然后调用GetWaitingClient获取到IWaitingClient的对象。
                    var returnData = client.GetWaitingClient(new WaitingOptions()
                    {
                        AdapterFilter = AdapterFilter.AllAdapter,//表示数据发送和接收时都会经过适配器
                        BreakTrigger = true,//当Client为Tcp系时。是否在断开连接时立即触发结果。默认会返回null。当ThrowBreakException为true时，会触发异常。
                        ThrowBreakException = true//是否触发异常
                    }).SendThenReturn(Encoding.UTF8.GetBytes(this.textBox2.Text));
                    this.m_service.Logger.Info($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");
                }
                catch (TimeoutException)
                {
                    this.m_service.Logger.Error("等待超时");
                }
                catch (Exception ex)
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
            if (this.listBox1.SelectedItem is string id && this.m_service.TryGetSocketClient(id, out var client))
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