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

using RpcProxy;
using System;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace DmtpClientApp;

public partial class Form1 : Form
{
    private TcpDmtpClient m_client;

    public Form1()
    {
        this.InitializeComponent();
    }

    private async void button1_Click(object sender, EventArgs e)
    {
        //直接调用时，第一个参数为调用键，服务类全名+方法名（必须全小写）
        //第二个参数为调用配置参数，可设置调用超时时间，取消调用等功能。
        //后续参数为调用参数。
        var result = await this.m_client.GetDmtpRpcActor().InvokeTAsync<bool>("Login", InvokeOption.WaitInvoke, this.textBox1.Text, this.textBox2.Text);
        MessageBox.Show(result.ToString());
    }

    private void button2_Click(object sender, EventArgs e)
    {
        var myRpcServer = new RpcProxy.MyRpcServer(this.m_client.GetDmtpRpcActor());//MyRpcServer类是由代码工具生成的类。

        //代理调用时，基本和本地调用一样。只是会多一个调用配置参数。
        var result = myRpcServer.Login(this.textBox1.Text, this.textBox2.Text, InvokeOption.WaitInvoke);
    }

    private void button3_Click(object sender, EventArgs e)
    {
        //扩展调用时，首先要保证本地已有代理文件，然后调用和和本地调用一样。只是会多一个调用配置参数。
        var result = this.m_client.GetDmtpRpcActor().Login(this.textBox1.Text, this.textBox2.Text, InvokeOption.WaitInvoke);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private void button4_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.m_client == null || this.m_client.DisposedValue)
            {
                this.m_client = new TcpDmtpClient();
            }
            this.m_client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();

                //使用心跳保活，或者避免异常连接。达到最大失败次数会断开，不会重连。
                a.UseDmtpHeartbeat()
                .SetTick(TimeSpan.FromSeconds(3))
                .SetMaxFailCount(3);

                //使用重连
                a.UseReconnection<TcpDmtpClient>()
                .SetPollingTick(TimeSpan.FromSeconds(3))
                .SetActionForCheck(async (c, i) =>//重新定义检活策略
                {
                    //方法1，直接判断是否在握手状态。使用该方式，最好和心跳插件配合使用
                    //await Task.CompletedTask;//消除Task
                    //return c.IsConnected;//判断是否在握手状态

                    //方法2，直接ping，如果true，则客户端必在线。如果false，则客户端不一定不在线，原因是可能当前传输正在忙
                    if ((await c.PingAsync()).IsSuccess)
                    {
                        return  ConnectionCheckResult.Alive;
                    }
                    //返回false时可以判断，如果最近活动时间不超过3秒，则猜测客户端确实在忙，所以跳过本次重连
                    else if (DateTime.Now - c.GetLastActiveTime() < TimeSpan.FromSeconds(3))
                    {
                        return  ConnectionCheckResult.Skip;
                    }
                    //否则，直接重连。
                    else
                    {
                        return  ConnectionCheckResult.Dead;
                    }
                });
            })
            .SetDmtpOption(options=>
            {
                options.VerifyToken = "Rpc";
                options.Id = "asdasd";
            }));
            this.m_client.ConnectAsync();

            MessageBox.Show("连接成功");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private async void button5_Click(object sender, EventArgs e)
    {
        await this.m_client.CloseAsync();
        this.m_client.SafeDispose();
    }

    private void button6_Click(object sender, EventArgs e)
    {
        this.m_client?.Dispose();
    }

    private void button7_Click(object sender, EventArgs e)
    {
    }
}