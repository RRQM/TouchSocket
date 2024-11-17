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

        private async Task IsConnected()
        {
            try
            {
                if (this.m_tcpClient?.Online == true)
                {
                    return;
                }
                this.m_tcpClient.SafeDispose();
                this.m_tcpClient = new TcpClient();

                this.m_tcpClient.Received = async (client, e) =>
                {
                    //此处不能await，否则也会导致死锁
                    _ = Task.Run(async () =>
                    {
                        var waitingClient = client.CreateWaitingClient(new WaitingOptions());

                        var bytes = await waitingClient.SendThenReturnAsync("hello");
                    });
                };

                await this.m_tcpClient.SetupAsync(new TouchSocketConfig()
                    .ConfigurePlugins(a =>
                    {
                        a.Add(typeof(ITcpReceivedPlugin), (ReceivedDataEventArgs e) =>
                        {
                            Console.WriteLine($"PluginReceivedData:{e.ByteBlock.Span.ToString(Encoding.UTF8)}");
                        });
                    })
                     .SetRemoteIPHost(this.textBox1.Text));

                await this.m_tcpClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                await this.IsConnected();
                var waitingClient = this.m_tcpClient.CreateWaitingClient(new WaitingOptions());

                cts = new CancellationTokenSource(5000);
                var bytes = await waitingClient.SendThenReturnAsync(this.textBox2.Text.ToUTF8Bytes(), cts.Token);
                if (bytes != null)
                {
                    MessageBox.Show($"message:{Encoding.UTF8.GetString(bytes)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                await this.IsConnected();
                var waitingClient = this.m_tcpClient.CreateWaitingClient(new WaitingOptions()
                {
                    FilterFuncAsync = async (response) =>
                    {
                        var byteBlock = response.ByteBlock;
                        var requestInfo = response.RequestInfo;

                        if (byteBlock != null)
                        {
                            var str = byteBlock.Span.ToString(Encoding.UTF8);
                            if (str.Contains(this.textBox4.Text))
                            {
                                return true;
                            }
                            else
                            {
                                //数据不符合要求，waitingClient继续等待

                                //如果需要在插件中继续处理，在此处触发插件

                                await this.m_tcpClient.PluginManager.RaiseAsync(typeof(ITcpReceivedPlugin), this.m_tcpClient, new ReceivedDataEventArgs(byteBlock, requestInfo)).ConfigureAwait(false);
                            }
                        }
                        return false;
                    }
                });

                cts = new CancellationTokenSource(500000);
                var bytes = await waitingClient.SendThenReturnAsync(this.textBox3.Text.ToUTF8Bytes(), cts.Token);

                if (bytes != null)
                {
                    MessageBox.Show($"message:{Encoding.UTF8.GetString(bytes)}");
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

        CancellationTokenSource cts;
        private void button5_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}