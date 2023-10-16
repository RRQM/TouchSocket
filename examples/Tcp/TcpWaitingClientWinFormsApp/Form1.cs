using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpWaitingClientWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();

            var client = new TcpClient();
            client.Connect("tcp://127.0.0.1:7789");

            //调用CreateWaitingClient获取到IWaitingClient的对象。
            var waitClient = client.CreateWaitingClient(new WaitingOptions()
            {
                BreakTrigger = true,//表示当连接断开时，会立即触发
                ThrowBreakException = true,//表示当连接断开时，是否触发异常
                FilterFunc = response => //设置用于筛选的fun委托，当返回为true时，才会响应返回
                {
                    if (response.Data.Length == 1)
                    {
                        return true;
                    }
                    return false;
                }
            });

            //然后使用SendThenReturn。
            byte[] returnData = waitClient.SendThenReturn(Encoding.UTF8.GetBytes("RRQM"));
            m_tcpClient.Logger.Info($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");

            //同时，如果适配器收到数据后，返回的并不是字节，而是IRequestInfo对象时，可以使用SendThenResponse.
            ResponsedData responsedData = waitClient.SendThenResponse(Encoding.UTF8.GetBytes("RRQM"));
            IRequestInfo requestInfo = responsedData.RequestInfo;//同步收到的RequestInfo
        }

        TcpClient m_tcpClient;

        private void IsConnected()
        {
            try
            {
                if (this.m_tcpClient?.Online == true)
                {
                    return;
                }
                this.m_tcpClient.SafeDispose();
                this.m_tcpClient = new TcpClient();

                //载入配置
                this.m_tcpClient.Setup(new TouchSocketConfig()
                    .SetRemoteIPHost(this.textBox1.Text));

                this.m_tcpClient.Connect();//调用连接，当连接不成功时，会抛出异常。
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this.IsConnected();
                var waitingClient = this.m_tcpClient.CreateWaitingClient(new WaitingOptions()
                {
                    BreakTrigger = true,
                    ThrowBreakException = true
                });

                var bytes = waitingClient.SendThenReturn(this.textBox2.Text.ToUTF8Bytes());
                if (bytes != null)
                {
                    MessageBox.Show($"收到等待数据：{Encoding.UTF8.GetString(bytes)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.IsConnected();
                var waitingClient = this.m_tcpClient.CreateWaitingClient(new WaitingOptions()
                {
                    BreakTrigger = true,
                    ThrowBreakException = true,
                    FilterFunc = (response) =>
                    {
                        if (response.Data != null)
                        {
                            var str = Encoding.UTF8.GetString(response.Data);
                            if (str.Contains(this.textBox4.Text))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                });

                var bytes = waitingClient.SendThenReturn(this.textBox3.Text.ToUTF8Bytes());
                if (bytes != null)
                {
                    MessageBox.Show($"收到等待数据：{Encoding.UTF8.GetString(bytes)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}