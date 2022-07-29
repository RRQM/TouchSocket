using System;
using System.Drawing;
using System.Windows.Forms;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;

namespace ScreenUdpReceiver
{
    /// <summary>
    /// 本程序源码由网友“木南白水”提供。
    /// </summary>
    public partial class Form1 : Form
    {
        UdpSession udpSession;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                udpSession = new UdpSession();

                udpSession.Received += (endpoint, byteBlock, requestInfo) =>
                {
                    pictureBox1.Image = Image.FromStream(byteBlock);
                };
                udpSession.Setup(new TouchSocketConfig()
               .SetBindIPHost(new IPHost("127.0.0.1:7790"))
               .SetBufferLength(1024 * 64)
               .SetUdpDataHandlingAdapter(() => { return new UdpPackageAdapter() { MaxPackageSize = 1024 * 1024, MTU = 1024 * 10 }; })
               ).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误：{ex.Message},程序将退出");
                Environment.Exit(0);
            }
        }
    }
}
