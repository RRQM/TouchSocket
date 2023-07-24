using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Sockets;

namespace RemoteAccessApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Load += this.Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            this.m_client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("Dmtp")
                .ConfigureContainer(a =>
                {
                    a.AddEasyLogger((msg =>
                    {
                        this.listBox1.Items.Insert(0, msg);
                    }));
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRemoteAccess();
                }));
            this.m_client.Connect();

            this.m_client.Logger.Info("�ɹ�����");
        }

        private TcpDmtpClient m_client = new TcpDmtpClient();

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text.IsNullOrEmpty())
                {
                    this.m_client.Logger.Warning("·������Ϊ�ա�");
                    return;
                }
                var result = await this.m_client.GetRemoteAccessActor().CreateDirectoryAsync(this.textBox1.Text, timeout: 30 * 1000);
                this.m_client.Logger.Info(result.ToString());
            }
            catch (Exception ex)
            {
                this.m_client?.Logger.Exception(ex);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text.IsNullOrEmpty())
                {
                    this.m_client.Logger.Warning("·������Ϊ�ա�");
                    return;
                }
                var result = await this.m_client.GetRemoteAccessActor().DeleteDirectoryAsync(this.textBox1.Text, timeout: 30 * 1000);
                this.m_client.Logger.Info(result.ToString());
            }
            catch (Exception ex)
            {
                this.m_client?.Logger.Exception(ex);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text.IsNullOrEmpty())
                {
                    this.m_client.Logger.Warning("·������Ϊ�ա�");
                    return;
                }
                var result = await this.m_client.GetRemoteAccessActor().GetDirectoryInfoAsync(this.textBox1.Text, timeout: 30 * 1000);
                this.m_client.Logger.Info($"�����{result.ResultCode}����Ϣ��{result.Message}��������Ϣ����Ի�á�");
            }
            catch (Exception ex)
            {
                this.m_client?.Logger.Exception(ex);
            }
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text.IsNullOrEmpty())
                {
                    this.m_client.Logger.Warning("·������Ϊ�ա�");
                    return;
                }
                var result = await this.m_client.GetRemoteAccessActor().DeleteFileAsync(this.textBox1.Text, timeout: 30 * 1000);
                this.m_client.Logger.Info(result.ToString());
            }
            catch (Exception ex)
            {
                this.m_client?.Logger.Exception(ex);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text.IsNullOrEmpty())
                {
                    this.m_client.Logger.Warning("·������Ϊ�ա�");
                    return;
                }
                var result = await this.m_client.GetRemoteAccessActor().GetFileInfoAsync(this.textBox1.Text, timeout: 30 * 1000);
                this.m_client.Logger.Info($"�����{result.ResultCode}����Ϣ��{result.Message}������Ϣ����Ի�á�");
            }
            catch (Exception ex)
            {
                this.m_client?.Logger.Exception(ex);
            }
        }
    }
}