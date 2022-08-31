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
        }

        private void MyWSClient_Handshaked(WebSocketClientBase client, HttpContextEventArgs e)
        {
            client.Logger.Info("成功连接");
        }

        private WebSocketClient myWSClient;

        private void button1_Click(object sender, EventArgs e)
        {
            myWSClient.SafeDispose();

            myWSClient = new WebSocketClient();
            myWSClient.Received += this.MyWSClient_Received;
            myWSClient.Handshaked += this.MyWSClient_Handshaked;

            myWSClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(this.textBox3.Text)
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger()));
                }));
            myWSClient.Connect();
        }

        private void MyWSClient_Received(WebSocketClient client, WSDataFrame dataFrame)
        {
            switch (dataFrame.Opcode)
            {
                case WSDataType.Cont:
                    client.Logger.Info($"收到中间数据，长度为：{dataFrame.PayloadLength}");
                    break;

                case WSDataType.Text:
                    client.Logger.Info(dataFrame.ToText());
                    break;

                case WSDataType.Binary:
                    if (dataFrame.FIN)
                    {
                        client.Logger.Info($"收到二进制数据，长度为：{dataFrame.PayloadLength}");
                    }
                    else
                    {
                        client.Logger.Info($"收到未结束的二进制数据，长度为：{dataFrame.PayloadLength}");
                    }
                    break;

                case WSDataType.Close:
                    {
                        client.Logger.Info("远程请求断开");
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