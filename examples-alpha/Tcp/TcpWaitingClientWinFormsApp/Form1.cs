//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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

                //��������
                this.m_tcpClient.SetupAsync(new TouchSocketConfig()
                    .SetRemoteIPHost(this.textBox1.Text));

                this.m_tcpClient.ConnectAsync();//�������ӣ������Ӳ��ɹ�ʱ�����׳��쳣��
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