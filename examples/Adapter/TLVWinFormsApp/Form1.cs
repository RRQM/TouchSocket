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
            this.m_tcpService.Received = (client, byteBlock, requestInfo) =>
            {
                if (requestInfo is TLVDataFrame frame)
                {
                    client.Logger.Info($"�������յ�,Tag={frame.Tag},Len={frame.Length},Value={(frame.Value != null ? Encoding.UTF8.GetString(frame.Value) : string.Empty)}");
                }
            };

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger(new EasyLogger(this.ShowMsg));
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>()//ʹ�ò�����൱���Զ�����������������������ӦPing��
                       .SetLengthType(FixedHeaderType.Int);//����֧�ֵ�����������ͣ���ֵ����SetMaxPackageSizeӰ�졣
                });

            //��������
            this.m_tcpService.Setup(config);

            //����
            this.m_tcpService.Start();
            this.m_tcpService.Logger.Info("�������ɹ�������");
        }

        private readonly TcpClient m_client = new TcpClient();

        private void button2_Click(object sender, EventArgs e)
        {
            this.m_client.Setup(new TouchSocketConfig()
                  .SetMaxPackageSize(1024 * 1024 * 10)
                  .ConfigureContainer(a =>
                  {
                      a.SetSingletonLogger(new EasyLogger(this.ShowMsg));
                  })
                  //.SetDataHandlingAdapter(() => new TLVDataHandlingAdapter(FixedHeaderType.Int, verifyFunc: null))//���ʹ��TLVPlugin������˲����ʡ�ԡ�
                  .ConfigurePlugins(a =>
                  {
                      a.Add<TLVPlugin>()//ʹ�ò�����൱���Զ�����������������������ӦPing��
                      .SetLengthType(FixedHeaderType.Int);//����֧�ֵ�����������ͣ���ֵ����SetMaxPackageSizeӰ�졣
                  })
                  .SetRemoteIPHost(new IPHost("127.0.0.1:7789")));
            this.m_client.Connect();

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