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

                //��������
                this.m_tcpClient.Setup(new TouchSocketConfig()
                    .SetRemoteIPHost(this.textBox1.Text));

                this.m_tcpClient.Connect();//�������ӣ������Ӳ��ɹ�ʱ�����׳��쳣��
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

                var bytes =waitingClient.SendThenReturn(this.textBox2.Text.ToUTF8Bytes());
                if (bytes != null)
                {
                    MessageBox.Show($"�յ��ȴ����ݣ�{Encoding.UTF8.GetString(bytes)}");
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
                    MessageBox.Show($"�յ��ȴ����ݣ�{Encoding.UTF8.GetString(bytes)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}