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

namespace TLVWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void ShowMsg(string msg)
        {
            this.listBox1.Items.Insert(0, msg);
        }

        private readonly TcpService m_tcpService = new TcpService();

        private void button1_Click(object sender, EventArgs e)
        {
            //�����յ���Ϣ�¼�
            this.m_tcpService.Received = (client, e) =>
            {
                if (e.RequestInfo is TLVDataFrame frame)
                {
                    client.Logger.Info($"�������յ�,Tag={frame.Tag},Length={frame.Length},Value={(frame.Value != null ? Encoding.UTF8.GetString(frame.Value) : string.Empty)}");
                }
                return EasyTask.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.AddEasyLogger(this.ShowMsg);
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>()//ʹ�ò�����൱���Զ�����������������������ӦPing��
                       .SetLengthType(FixedHeaderType.Int);//����֧�ֵ�����������ͣ���ֵ����SetMaxPackageSizeӰ�졣
                });

            //��������
            this.m_tcpService.SetupAsync(config);

            //����
            this.m_tcpService.StartAsync();
            this.m_tcpService.Logger.Info("�������ɹ�������");
        }

        private readonly TcpClient m_client = new TcpClient();

        private void button2_Click(object sender, EventArgs e)
        {
            this.m_client.SetupAsync(new TouchSocketConfig()
                  .SetAdapterOption(new AdapterOption()
                  {
                      MaxPackageSize = 1024 * 1024 * 10
                  })
                  .ConfigureContainer(a =>
                  {
                      a.AddEasyLogger(this.ShowMsg);
                  })
                  //.SetDataHandlingAdapter(() => new TLVDataHandlingAdapter(FixedHeaderType.Int, verifyFunc: null))//���ʹ��TLVPlugin������˲����ʡ�ԡ�
                  .ConfigurePlugins(a =>
                  {
                      a.Add<TLVPlugin>()//ʹ�ò�����൱���Զ�����������������������ӦPing��
                      .SetLengthType(FixedHeaderType.Int);//����֧�ֵ�����������ͣ���ֵ����SetMaxPackageSizeӰ�졣
                  })
                  .SetRemoteIPHost(new IPHost("127.0.0.1:7789")));
            this.m_client.ConnectAsync();

            this.m_client.Logger.Info("���ӳɹ�");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_client?.Send(new ValueTLVDataFrame((ushort)this.numericUpDown1.Value, Encoding.UTF8.GetBytes(this.textBox1.Text)));
            }
            catch (Exception ex)
            {
                this.m_client.Logger.Exception(ex);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_client.Logger.Info($"ping={this.m_client?.PingWithTLV()}");
            }
            catch (Exception ex)
            {
                this.m_client.Logger.Exception(ex);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    this.m_client?.Send(new ValueTLVDataFrame((ushort)this.numericUpDown1.Value, Encoding.UTF8.GetBytes(i.ToString())));
                }
                catch (Exception ex)
                {
                    this.m_client?.Logger.Exception(ex);
                }
            }
        }
    }
}