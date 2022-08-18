//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace EERPCClientDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void ShowMsg(string msg)
        {
            this.Invoke((Action)(delegate () { this.textBox1.AppendText(msg + "\r\n"); }));
        }

        private TcpTouchRpcClient tcpRpcClient;
        private void button3_Click(object sender, EventArgs e)
        {
            this.tcpRpcClient = new TcpTouchRpcClient();
            this.tcpRpcClient.Disconnected += TcpRpcClient_Disconnected;
            tcpRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigureRpcStore(a=> 
                {
                    a.RegisterServer<ThisRpcServer>();
                }))
                .Connect(1000 * 100);
            this.button3.Enabled = false;
            this.Text = this.tcpRpcClient.ID;
            ShowMsg("连接成功");
        }

        private void TcpRpcClient_Disconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            ShowMsg("已断开连接");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                AccessType accessType = AccessType.Owner;
                if (this.checkBox1.Checked)
                {
                    accessType = accessType | AccessType.Owner;
                }
                if (this.checkBox2.Checked)
                {
                    accessType = accessType | AccessType.Service;
                }
                if (this.checkBox3.Checked)
                {
                    accessType = accessType | AccessType.Everyone;
                }
                this.tcpRpcClient.PublishEvent(this.textBox2.Text, accessType);
                ShowMsg("发布成功");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] events = this.tcpRpcClient.GetAllEvents();

            this.listBox1.Items.Clear();
            this.listBox1.Items.AddRange(events);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                this.tcpRpcClient.SubscribeEvent<string>(this.textBox3.Text, SubscribeEvent);
                this.ShowMsg($"订阅成功");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void SubscribeEvent(EventSender eventSender, string arg)
        {
            this.ShowMsg($"从{eventSender.RaiseSourceType}收到通知事件{eventSender.EventName}，信息：{arg}");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.SelectedItem is string eventName)
                {
                    this.tcpRpcClient.RaiseEvent(eventName, this.textBox4.Text);
                    ShowMsg("触发成功");
                }
                else
                {
                    ShowMsg("请先选择事件");
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                this.tcpRpcClient.UnpublishEvent(this.textBox2.Text);
                ShowMsg("取消发布成功");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                this.tcpRpcClient.UnsubscribeEvent(this.textBox3.Text);
                ShowMsg("取消订阅成功");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                this.ShowMsg("正在试用企业版功能，1小时后失效。");
            }
        }
    }

    internal class ThisRpcServer : RpcServer
    {
        [TouchRpc(true)]
        public DateTime GetDataTime()
        {
            return DateTime.Now;
        }
    }
}