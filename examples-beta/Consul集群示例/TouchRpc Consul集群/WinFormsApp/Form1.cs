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
                    client.Setup(new TouchSocketConfig()
                        .ConfigurePlugins(a =>
                        {
                            a.UseDmtpRpc();
                        })
                        .SetRemoteIPHost($"{agentService.Address}:{agentService.Port}"));
                    client.Connect();

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
                    var client = new JsonRpcClient();
                    client.Setup(new TouchSocketConfig()
                        .SetJRPT(JRPT.Http)
                        .SetRemoteIPHost($"http://{agentService.Address}:{agentService.Port}/jsonrpc"));
                    client.Connect();

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