using System;
using System.Windows.Forms;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace WSClientApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Load += this.Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myWSClient.Received += this.MyWSClient_Received;
            myWSClient.Handshaked += this.MyWSClient_Handshaked;
        }

        private void MyWSClient_Handshaked(WebSocketClientBase client, HttpContextEventArgs e)
        {
            client.Logger.Message("成功连接");
        }

        private WebSocketClient myWSClient = new WebSocketClient();

        private void button1_Click(object sender, EventArgs e)
        {
            myWSClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:7789/ws")
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger()));
                }));
            myWSClient.Connect();

            myWSClient.Logger.Message("连接成功");
        }

        private void MyWSClient_Received(WebSocketClient client, WSDataFrame dataFrame)
        {
            switch (dataFrame.Opcode)
            {
                case WSDataType.Cont:
                    client.Logger.Message($"收到中间数据，长度为：{dataFrame.PayloadLength}");
                    break;

                case WSDataType.Text:
                    client.Logger.Message(dataFrame.ToText());
                    break;

                case WSDataType.Binary:
                    if (dataFrame.FIN)
                    {
                        client.Logger.Message($"收到二进制数据，长度为：{dataFrame.PayloadLength}");
                    }
                    else
                    {
                        client.Logger.Message($"收到未结束的二进制数据，长度为：{dataFrame.PayloadLength}");
                    }
                    break;

                case WSDataType.Close:
                    {
                        client.Logger.Message("远程请求断开");
                        client.Close("断开");
                    }

                    break;

                case WSDataType.Ping:
                    break;

                case WSDataType.Pong:
                    break;

                default:
                    break;
            }
        }

        private void ShowMsg(string msg)
        {
            this.textBox1.AppendText(msg);
            this.textBox1.AppendText("\r\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this.myWSClient.SendWithWS(this.textBox2.Text);
            }
            catch (Exception ex)
            {
                this.myWSClient.Logger.Exception(ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1024 * 1024 * 10];
            try
            {
                this.myWSClient.SubSendWithWS(data, 1024 * 1024);
            }
            catch (Exception ex)
            {
                this.myWSClient.Logger.Exception(ex);
            }
        }
    }
}