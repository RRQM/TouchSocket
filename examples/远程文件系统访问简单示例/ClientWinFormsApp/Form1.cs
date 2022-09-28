using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace ClientWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private TcpTouchRpcClient m_client = new TcpTouchRpcClient();

        private void Form1_Load(object sender, EventArgs e)
        {
            m_client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc")
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger(new EasyLogger(msg =>
                    {
                        this.listBox1.Items.Insert(0, msg);
                    }));
                }));
            m_client.Connect();

            m_client.Logger.Info("成功连接");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.textBox1.Text.IsNullOrEmpty())
                {
                    this.m_client.Logger.Warning("路径不可为空。");
                    return;
                }
                Result result = await this.m_client?.CreateDirectoryAsync(this.textBox1.Text);
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
                    this.m_client.Logger.Warning("路径不可为空。");
                    return;
                }
                Result result = await this.m_client?.DeleteDirectoryAsync(this.textBox1.Text);
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
                    this.m_client.Logger.Warning("路径不可为空。");
                    return;
                }
                RemoteDirectoryInfoResult result = await this.m_client?.GetDirectoryInfoAsync(this.textBox1.Text);
                this.m_client.Logger.Info($"结果：{result.ResultCode}，信息：{result.Message}，更多信息请调试获得。");
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
                    this.m_client.Logger.Warning("路径不可为空。");
                    return;
                }
                Result result = await this.m_client?.DeleteFileAsync(this.textBox1.Text);
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
                    this.m_client.Logger.Warning("路径不可为空。");
                    return;
                }
                RemoteFileInfoResult result = await this.m_client?.GetFileInfoAsync(this.textBox1.Text);
                this.m_client.Logger.Info($"结果：{result.ResultCode}，信息：{result.Message}更多信息请调试获得。");
            }
            catch (Exception ex)
            {
                this.m_client?.Logger.Exception(ex);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Task.Run(() =>
                {
                    try
                    {
                        //RemoteStream是将服务器的流数据映射到此处，然后可以直接当本地Stream一样的读取和写入。
                        using (RemoteStream remoteStream = this.m_client?.LoadRemoteStream(new Metadata()))
                        {
                            long length = 0;
                            byte[] buffer = new byte[1024 * 10];
                            while (true)
                            {
                                int r = remoteStream.Read(buffer, 0, buffer.Length);
                                if (r == 0)
                                {
                                    break;
                                }
                                length += r;
                            }
                            MessageBox.Show(length.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        this.m_client?.Logger.Exception(ex);
                    }
                });
            }
        }
    }
}