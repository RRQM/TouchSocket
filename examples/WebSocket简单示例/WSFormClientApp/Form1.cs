using System;
using System.Text;
using System.Windows.Forms;
using TouchSocket.Core;
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

        private WebSocketClient client;

        private void button1_Click(object sender, EventArgs e)
        {
            client.SafeDispose();

            client = new WebSocketClient();
            client.Received = this.MyWSClient_Received;
            client.Handshaked = this.MyWSClient_Handshaked;

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(this.textBox3.Text)
                .UsePlugin()
                .ConfigureContainer(a =>
                {
                    a.AddFileLogger();
                    a.AddEasyLogger(this.ShowMsg);
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocketHeartbeat()//使用心跳插件
                    .Tick(TimeSpan.FromSeconds(5));//每5秒ping一次。

                    a.Add<MyWSClientPlugin>();
                }));
            client.Connect();
            client.CloseWithWS("close");
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
                this.client.SendWithWS(this.textBox2.Text);
            }
            catch (Exception ex)
            {
                this.client.Logger.Exception(ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            try
            {
                this.client.SubSendWithWS(data, 5);
            }
            catch (Exception ex)
            {
                this.client.Logger.Exception(ex);
            }
        }
    }

    class MyWSClientPlugin : WebSocketPluginBase<WebSocketClient>
    {
        protected override void OnHandleWSDataFrame(WebSocketClient client, WSDataFrameEventArgs e)
        {
            switch (e.DataFrame.Opcode)
            {
                case WSDataType.Cont:
                    client.Logger.Info($"收到中间数据，长度为：{e.DataFrame.PayloadLength}");
                    break;

                case WSDataType.Text:
                    client.Logger.Info(e.DataFrame.ToText());
                    break;

                case WSDataType.Binary:
                    if (e.DataFrame.FIN)
                    {
                        client.Logger.Info($"收到二进制数据，长度为：{e.DataFrame.PayloadLength}");
                    }
                    else
                    {
                        client.Logger.Info($"收到未结束的二进制数据，长度为：{e.DataFrame.PayloadLength}");
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

        protected override void OnHandshaking(WebSocketClient client, HttpContextEventArgs e)
        {
            e.Context.Request.Headers["cookie"] = "";
            base.OnHandshaking(client, e);
        }
    }
}