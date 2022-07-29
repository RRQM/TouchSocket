﻿using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;

namespace ScreenUdpSender
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

        Thread m_thread;

        private void Tick()
        {
            while (true)
            {
                byte[] byteArray = ImageToByte(getScreen());
                ByteBlock bb = new ByteBlock(byteArray);
                udpSession.Send(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7790), bb);
                Thread.Sleep((int)(1000.0/(int)this.numericUpDown1.Value));
            }
        }


        #region 屏幕和光标获取
        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        private const Int32 CURSOR_SHOWING = 0x00000001;
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }
      
        public Image getScreen(int x = 0, int y = 0, int width = -1, int height = -1, String savePath = "", bool haveCursor = true)
        {
            if (width == -1) width = SystemInformation.VirtualScreen.Width;
            if (height == -1) height = SystemInformation.VirtualScreen.Height;

            Bitmap tmp = new Bitmap(width, height);                 //按指定大小创建位图
            Graphics g = Graphics.FromImage(tmp);                   //从位图创建Graphics对象
            g.CopyFromScreen(x, y, 0, 0, new Size(width, height));  //绘制

            // 绘制鼠标
            if (haveCursor)
            {
                try
                {
                    CURSORINFO pci;
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    GetCursorInfo(out pci);
                    System.Windows.Forms.Cursor cur = new System.Windows.Forms.Cursor(pci.hCursor);
                    cur.Draw(g, new Rectangle(pci.ptScreenPos.x, pci.ptScreenPos.y, cur.Size.Width, cur.Size.Height));
                }
                catch (Exception ex) { }    // 若获取鼠标异常则不显示
            }

            //Size halfSize = new Size((int)(tmp.Size.Width * 0.8), (int)(tmp.Size.Height * 0.8));  // 按一半尺寸存储图像
            //if (!savePath.Equals("")) saveImage(tmp, tmp.Size, savePath);       // 保存到指定的路径下

            return tmp;     //返回构建的新图像
        }
        #endregion

        #region 格式转换
        private byte[] ImageToByte(Image Picture)
        {
            MemoryStream ms = new MemoryStream();
            if (Picture == null)
                return new byte[ms.Length];
            Picture.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] BPicture = new byte[ms.Length];
            BPicture = ms.GetBuffer();
            return BPicture;
        }

        private Image ByteToImage(byte[] btImage)
        {
            if (btImage.Length == 0)
                return null;
            System.IO.MemoryStream ms = new System.IO.MemoryStream(btImage);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            return image;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            try
            {
                udpSession = new UdpSession();

                udpSession.Setup(
                new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7789))
                .SetBufferLength(1024 * 64)
                .SetUdpDataHandlingAdapter(() => { return new UdpPackageAdapter() { MaxPackageSize = 1024 * 1024, MTU = 1024 * 10 }; })
                ).Start();
                m_thread = new Thread(Tick);
                m_thread.IsBackground = true;
                m_thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误：{ex.Message},程序将退出");
                Environment.Exit(0);
            }
        }
    }
}
