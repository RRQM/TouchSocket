using System.Text;
using System.Windows.Forms;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace TLVWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void ShowMsg(string msg)
        {
            this.listBox1.Items.Insert(0, msg);
        }

        readonly TcpService m_tcpService = new TcpService();
        private void button1_Click(object sender, EventArgs e)
        {
            //�����յ���Ϣ�¼�
            m_tcpService.Received = (client, byteBlock, requestInfo) =>
            {
                if (requestInfo is TLVDataFrame frame)
                {
                    client.Logger.Info($"�������յ�,Tag={frame.Tag},Len={frame.Length},Value={(frame.Value!=null?Encoding.UTF8.GetString(frame.Value):string.Empty)}") ;
                }
            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .UsePlugin()
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
            m_tcpService.Setup(config);

            //����
            m_tcpService.Start();
            m_tcpService.Logger.Info("�������ɹ�������");
        }

        readonly TcpClient m_client = new TcpClient();
        private void button2_Click(object sender, EventArgs e)
        {
            m_client.Setup(new TouchSocketConfig()
                  .UsePlugin()
                  .SetMaxPackageSize(1024*1024*10)
                  .ConfigureContainer(a =>
                  {
                      a.SetSingletonLogger(new EasyLogger(this.ShowMsg));
                  })
                  //.SetDataHandlingAdapter(() => new TLVDataHandlingAdapter(FixedHeaderType.Int, verifyFunc: null))//���ʹ��TLVPlugin������˲����ʡ�ԡ�
                  .ConfigurePlugins(a =>
                  {
                      a.Add<TLVPlugin>()//ʹ�ò�����൱���Զ�����������������������ӦPing��
                      .SetLengthType(FixedHeaderType.Int);//����֧�ֵ�����������ͣ���ֵ����SetMaxPackageSizeӰ�졣

                      a.Add<PollingKeepAlivePlugin<TcpClient>>()
                      .SetTick(1000)
                      .SetActionForCheck((c) => 
                      {
                          c.Logger.Info($"�Զ�ping�����{c.Ping()}");
                      });
                  })
                  .SetRemoteIPHost(new IPHost("127.0.0.1:7789")));
            m_client.Connect();

            m_client.Logger.Info("���ӳɹ�");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_client?.Send(new ValueTLVDataFrame((ushort)this.numericUpDown1.Value,Encoding.UTF8.GetBytes(this.textBox1.Text)));
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
                m_client.Logger.Info($"ping={this.m_client?.Ping()}");
            }
            catch (Exception ex)
            {
                this.m_client.Logger.Exception(ex);
            }
        }
    }
}