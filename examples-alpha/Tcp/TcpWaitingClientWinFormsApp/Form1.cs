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
        }

        private TcpClient m_tcpClient;

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
                this.m_tcpClient.SetupAsync(new TouchSocketConfig()
                    .SetRemoteIPHost(this.textBox1.Text));

                this.m_tcpClient.ConnectAsync();//调用连接，当连接不成功时，会抛出异常。
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
                var waitingClient = this.m_tcpClient.CreateWaitingClient(new WaitingOptions());

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

        private void button1_Click(object sender, EventArgs e)
        {
            this.m_tcpClient?.Close();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.IsConnected();
                var waitingClient = this.m_tcpClient.CreateWaitingClient(new WaitingOptions()
                {
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

                var bytes = await waitingClient.SendThenReturnAsync(this.textBox3.Text.ToUTF8Bytes());
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