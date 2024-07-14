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

using Consul;
using System;
using System.Linq;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
        }

        private AgentService[] services;

        private async void button1_Click(object sender, EventArgs e)
        {
            var consulClient = new ConsulClient(x => x.Address = new Uri($"http://127.0.0.1:8500"));//请求注册的 Consul 地址
            var ret = await consulClient.Agent.Services();

            this.services = ret.Response.Values.ToArray();
            this.listBox1.DataSource = this.services;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem is AgentService agentService)
            {
                try
                {
                    var client = new HttpDmtpClient();
                    client.SetupAsync(new TouchSocketConfig()
                        .ConfigurePlugins(a =>
                        {
                            a.UseDmtpRpc();
                        })
                        .SetRemoteIPHost($"{agentService.Address}:{agentService.Port}"));
                    client.ConnectAsync();

                    //直接调用时，第一个参数为服务名+方法名（必须全小写）
                    //第二个参数为调用配置参数，可设置调用超时时间，取消调用等功能。
                    //后续参数为调用参数。
                    var result = client.GetDmtpRpcActor().InvokeT<string>("myserver/sayhello", InvokeOption.WaitInvoke, this.textBox1.Text);
                    client.SafeDispose();
                    MessageBox.Show(result);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("请先选择一个服务器节点。");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem is AgentService agentService)
            {
                try
                {
                    var client = new HttpJsonRpcClient();
                    client.SetupAsync(new TouchSocketConfig()
                        .SetRemoteIPHost($"http://{agentService.Address}:{agentService.Port}/jsonrpc"));
                    client.ConnectAsync();

                    var result = client.InvokeT<string>("myserver/sayhello", InvokeOption.WaitInvoke, this.textBox1.Text);
                    client.SafeDispose();
                    MessageBox.Show(result);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("请先选择一个服务器节点。");
            }
        }
    }
}