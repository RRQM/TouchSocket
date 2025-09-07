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

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Sockets;

namespace RemoteAccessApp;

public partial class Form1 : Form
{
    public Form1()
    {
        this.InitializeComponent();
        Control.CheckForIllegalCrossThreadCalls = false;
        this.Load += this.Form1_Load;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        this.m_client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .SetDmtpOption(options=>
            {
                options.VerifyToken = "Dmtp";
            })
            .ConfigureContainer(a =>
            {
                a.AddEasyLogger((msg =>
                {
                    this.listBox1.Items.Insert(0, msg);
                }));
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRemoteAccess();
            }));
        this.m_client.ConnectAsync();

        this.m_client.Logger.Info("成功连接");
    }

    private readonly TcpDmtpClient m_client = new TcpDmtpClient();

    private async void button1_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.textBox1.Text.IsNullOrEmpty())
            {
                this.m_client.Logger.Warning("路径不能为空。");
                return;
            }
            var result = await this.m_client.GetRemoteAccessActor().CreateDirectoryAsync(this.textBox1.Text, millisecondsTimeout: 30 * 1000);
            this.m_client.Logger.Info(result.ToString());
        }
        catch (Exception ex)
        {
            this.m_client?.Logger.Exception(ex);
        }
    }

    private async void button2_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.textBox1.Text.IsNullOrEmpty())
            {
                this.m_client.Logger.Warning("路径不能为空。");
                return;
            }
            var result = await this.m_client.GetRemoteAccessActor().DeleteDirectoryAsync(this.textBox1.Text, millisecondsTimeout: 30 * 1000);
            this.m_client.Logger.Info(result.ToString());
        }
        catch (Exception ex)
        {
            this.m_client?.Logger.Exception(ex);
        }
    }

    private async void button3_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.textBox1.Text.IsNullOrEmpty())
            {
                this.m_client.Logger.Warning("路径不能为空。");
                return;
            }
            var result = await this.m_client.GetRemoteAccessActor().GetDirectoryInfoAsync(this.textBox1.Text, millisecondsTimeout: 30 * 1000);
            this.m_client.Logger.Info($"结果：{result.ResultCode}，信息：{result.Message}，详细信息请在对话框获取。");
        }
        catch (Exception ex)
        {
            this.m_client?.Logger.Exception(ex);
        }
    }

    private async void button5_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.textBox1.Text.IsNullOrEmpty())
            {
                this.m_client.Logger.Warning("路径不能为空。");
                return;
            }
            var result = await this.m_client.GetRemoteAccessActor().DeleteFileAsync(this.textBox1.Text, millisecondsTimeout: 30 * 1000);
            this.m_client.Logger.Info(result.ToString());
        }
        catch (Exception ex)
        {
            this.m_client?.Logger.Exception(ex);
        }
    }

    private async void button4_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.textBox1.Text.IsNullOrEmpty())
            {
                this.m_client.Logger.Warning("路径不能为空。");
                return;
            }
            var result = await this.m_client.GetRemoteAccessActor().GetFileInfoAsync(this.textBox1.Text, millisecondsTimeout: 30 * 1000);
            this.m_client.Logger.Info($"结果：{result.ResultCode}，信息：{result.Message}，详细信息请在对话框获取。");
        }
        catch (Exception ex)
        {
            this.m_client?.Logger.Exception(ex);
        }
    }
}