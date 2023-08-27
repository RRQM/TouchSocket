using System;
using System.Drawing;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ScreenUdpReceiver
{
    /// <summary>
    /// 本程序源码由网友“木南白水”提供。
    /// </summary>
    public partial class Form1 : Form
    {
        private UdpSession udpSession;

        public Form1()
        {
            this.InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.udpSession = new UdpSession();

                this.udpSession.Received = (endpoint, byteBlock, requestInfo) =>
                {
                    this.pictureBox1.Image = Image.FromStream(byteBlock);
                };
                this.udpSession.Setup(new TouchSocketConfig()
               .SetBindIPHost(new IPHost("127.0.0.1:7790"))
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