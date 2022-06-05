using RRQMCore.Log;
using RRQMSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UdpDemoApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        UdpSession m_udpSession = new UdpSession();
        private void button1_Click(object sender, EventArgs e)
        {
            m_udpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                m_udpSession.Logger.Message($"收到：{Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len)}");
            };
            m_udpSession.Setup(new RRQMConfig()
                 .SetBindIPHost(new IPHost(this.textBox2.Text))
                 .SetSingletonLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger())))
                 .Start();
            m_udpSession.Logger.Message("等待接收");
        }

        private void ShowMsg(string msg)
        {
            this.textBox1.AppendText(msg);
            this.textBox1.AppendText("\r\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_udpSession.Send(new IPHost(this.textBox3.Text).EndPoint,Encoding.UTF8.GetBytes(this.textBox4.Text));
        }
    }
}
