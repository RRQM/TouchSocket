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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UdpDemoApp;

public partial class Form1 : Form
{
    public Form1()
    {
        this.InitializeComponent();
        Control.CheckForIllegalCrossThreadCalls = false;
    }

    private readonly UdpSession m_udpSession = new UdpSession();

    private async void button1_Click(object sender, EventArgs e)
    {
        this.m_udpSession.Received = async (remote, e) =>
        {
            if (e.Memory.Length > 1024)
            {
                this.m_udpSession.Logger.Info($"收到：{e.Memory.Length}长度的数据。");
                await this.m_udpSession.SendAsync("收到");
            }
            else
            {
                this.m_udpSession.Logger.Info($"收到：{e.Memory.Span.ToString(Encoding.UTF8)}");
            }
            var endPoint = e.EndPoint;
        };

        await this.m_udpSession.SetupAsync(new TouchSocketConfig()
               .SetBindIPHost(new IPHost(this.textBox2.Text))
               .SetRemoteIPHost(new IPHost(this.textBox3.Text))
               .UseBroadcast()
               .SetUdpDataHandlingAdapter(() =>
               {
                   if (this.checkBox1.Checked)
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
                   a.AddLogger(new LoggerGroup(new EasyLogger(this.ShowMsg), new FileLogger()));
               }));
        await this.m_udpSession.StartAsync();
        this.m_udpSession.Logger.Info("等待接收");
    }

    private void ShowMsg(string msg)
    {
        this.textBox1.AppendText(msg);
        this.textBox1.AppendText("\r\n");
    }

    private async void button2_Click(object sender, EventArgs e)
    {
        await this.m_udpSession.SendAsync(new IPHost(this.textBox3.Text).EndPoint, Encoding.UTF8.GetBytes(this.textBox4.Text));
    }

    private async void button3_Click(object sender, EventArgs e)
    {
        if (!this.checkBox1.Checked)
        {
            this.m_udpSession.Logger.Warning("发送大数据时，请使用UdpPackageAdapter适配器");
        }

        try
        {
            await this.m_udpSession.SendAsync(new IPHost(this.textBox3.Text).EndPoint, new byte[1024 * 1024]);
        }
        catch (Exception ex)
        {
            this.m_udpSession.Logger.Exception(ex);
        }
    }

    private async void button4_Click(object sender, EventArgs e)
    {
        //调用CreateWaitingClient获取到IWaitingClient的对象。
        var waitClient = this.m_udpSession.CreateWaitingClient(new WaitingOptions()
        {
            RemoteIPHost = new IPHost(this.textBox3.Text)//表示目的地址
        });

        //然后使用SendThenReturn。
        var returnData = await waitClient.SendThenReturnAsync(Encoding.UTF8.GetBytes("RRQM"));
        this.ShowMsg($"收到回应消息：{Encoding.UTF8.GetString(returnData)}");

        ////同时，如果适配器收到数据后，返回的并不是字节，而是IRequestInfo对象时，可以使用SendThenResponse.
        //ResponsedData responsedData = waitClient.SendThenResponse(Encoding.UTF8.GetBytes("RRQM"));
        //IRequestInfo requestInfo = responsedData.RequestInfo;//同步收到的RequestInfo
    }
}