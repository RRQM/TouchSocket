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

using System;
using System.Drawing;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ScreenUdpReceiver;

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

            this.udpSession.Received = (c, e) =>
            {
                this.pictureBox1.Image = Image.FromStream(e.ByteBlock.AsStream(false));
                return EasyTask.CompletedTask;
            };
            this.udpSession.SetupAsync(new TouchSocketConfig()
           .SetBindIPHost(new IPHost("127.0.0.1:7790"))
           .SetUdpDataHandlingAdapter(() => { return new UdpPackageAdapter() { MaxPackageSize = 1024 * 1024, MTU = 1024 * 10 }; }));
            this.udpSession.StartAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"错误：{ex.Message},程序将退出");
            Environment.Exit(0);
        }
    }
}