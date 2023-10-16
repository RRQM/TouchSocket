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

            //����CreateWaitingClient��ȡ��IWaitingClient�Ķ���
            var waitClient = client.CreateWaitingClient(new WaitingOptions()
            {
                BreakTrigger = true,//��ʾ�����ӶϿ�ʱ������������
                ThrowBreakException = true,//��ʾ�����ӶϿ�ʱ���Ƿ񴥷��쳣
                FilterFunc = response => //��������ɸѡ��funί�У�������Ϊtrueʱ���Ż���Ӧ����
                {
                    if (response.Data.Length == 1)
                    {
                        return true;
                    }
                    return false;
                }
            });

            //Ȼ��ʹ��SendThenReturn��
            byte[] returnData = waitClient.SendThenReturn(Encoding.UTF8.GetBytes("RRQM"));
            m_tcpClient.Logger.Info($"�յ���Ӧ��Ϣ��{Encoding.UTF8.GetString(returnData)}");

            //ͬʱ������������յ����ݺ󣬷��صĲ������ֽڣ�����IRequestInfo����ʱ������ʹ��SendThenResponse.
            ResponsedData responsedData = waitClient.SendThenResponse(Encoding.UTF8.GetBytes("RRQM"));
            IRequestInfo requestInfo = responsedData.RequestInfo;//ͬ���յ���RequestInfo
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

                var bytes = waitingClient.SendThenReturn(this.textBox2.Text.ToUTF8Bytes());
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