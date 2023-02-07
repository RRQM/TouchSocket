using System;
using System.Text;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UdpDemoApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private UdpSession m_udpSession = new UdpSession();

        private void button1_Click(object sender, EventArgs e)
        {
            m_udpSession.Received = (remote, byteBlock, requestInfo) =>
            {
                if (byteBlock.Len > 1024)
                {
                    m_udpSession.Logger.Info($"收到：{byteBlock.Len}长度的数据。");
                }
                else
                {
                    m_udpSession.Logger.Info($"收到：{Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len)}");
                }
            };

            m_udpSession.Setup(new TouchSocketConfig()
                 .SetBindIPHost(new IPHost(this.textBox2.Text))
                 .UseBroadcast()
                 .SetUdpDataHandlingAdapter(() =>
                 {
                     if (checkBox1.Checked)
                     {
                         return new UdpPackageAdapter();
                     }
                     else
                     {
                         return new NormalUdpDataHandlingAdapter();
                     }
                 })
                 .ConfigureContainer(a =>
                 {
                     a.SetSingletonLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger()));
                 }))
                 .Start();
            m_udpSession.Logger.Info("等待接收");
        }

        private void ShowMsg(string msg)
        {
            this.textBox1.AppendText(msg);
            this.textBox1.AppendText("\r\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_udpSession.Send(new IPHost(this.textBox3.Text).EndPoint, Encoding.UTF8.GetBytes(this.textBox4.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                m_udpSession.Logger.Warning("发送大数据时，请使用UdpPackageAdapter适配器");
            }

            try
            {
                m_udpSession.Send(new IPHost(this.textBox3.Text).EndPoint, new byte[1024 * 1024]);
            }
            catch (Exception ex)
            {
                m_udpSession.Logger.Exception(ex);
            }
        }
    }

    class MyUdpPluginClass:UdpSessionPluginBase
    {

    }
}