using Consul;
using System;
using System.Linq;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private AgentService[] services;

        private async void button1_Click(object sender, EventArgs e)
        {
            var consulClient = new ConsulClient(x => x.Address = new Uri($"http://127.0.0.1:8500"));//请求注册的 Consul 地址
            var ret = await consulClient.Agent.Services();

            services = ret.Response.Values.ToArray();
            this.listBox1.DataSource = services;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem is AgentService agentService)
            {
                try
                {
                    HttpTouchRpcClient client = new HttpTouchRpcClient();
                    client.Setup(new TouchSocketConfig()
                        .SetRemoteIPHost($"{agentService.Address}:{agentService.Port}"));
                    client.Connect();

                    //直接调用时，第一个参数为服务名+方法名（必须全小写）
                    //第二个参数为调用配置参数，可设置调用超时时间，取消调用等功能。
                    //后续参数为调用参数。
                    string result = client.Invoke<string>("myserver/sayhello", InvokeOption.WaitInvoke, textBox1.Text);
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
                    JsonRpcClient client = new JsonRpcClient();
                    client.Setup(new TouchSocketConfig()
                        .SetJRPT(JRPT.Http)
                        .SetRemoteIPHost($"http://{agentService.Address}:{agentService.Port}/jsonrpc"));
                    client.Connect();

                    string result = client.Invoke<string>("myserver/sayhello", InvokeOption.WaitInvoke, textBox1.Text);
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