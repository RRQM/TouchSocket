
/// 本项目 基于 RRQM TCP
/// 开发环境  window11   Vs2022 WinFrom Core6 
///  开发者 陈峰
///  微信：ChenFeng_My
///  

using RRQMCore;
using RRQMCore.ByteManager;
using RRQMSocket;
using System.Text;
using WinForms_Core6_Server_Dome.Models;

namespace WinForms_Core6_Server_Dome
{
    public partial class Form1 : Form
    {
        TcpService Service = new TcpService();    //  全局 TCP
        List<Client_Mode> client_Modes = new List<Client_Mode>(); //client Mode
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            Button bt = sender as Button;
            bt.Enabled = false;
            button3.Enabled = true;    
            Server_star();  //服务器启动

        }



        /// <summary>
        /// 服务器启动
        /// </summary>
        void Server_star() 
        {

            textBox3.AppendText("正在启动。。。。");
           

            string serverIP1 = textBox1.Text;
            string serverIP2 = textBox1.Text;

            string serverPort1 = numericUpDown1.Value.ToString();           
            string serverPort2 = numericUpDown2.Value.ToString();

            string Host1 = $"{serverIP1}:{serverPort1}";
            string Host2 = $"{serverIP2}:{serverPort2}";


            
            string errMsg = string.Empty;   //msg 记录错误信息
            if (serverIP1 == String.Empty) errMsg="请填写IP1";
            if (serverIP2 == String.Empty) Host2 = serverPort2;
            if (Convert.ToInt32( serverPort1)==0 ) errMsg = "端口1不可以为0";
            if (Convert.ToInt32(serverPort2) == 0) errMsg = "端口2不可以为0";

            // 如果有错误则返回
            if (errMsg != string.Empty)
            {
                MessageBox.Show(errMsg);
                textBox3.AppendText(errMsg);
                return;

            }

            Service.Connecting += (client, e) => Connecting(client, e);             //有客户端正在连接
            Service.Connected += (client, e) => Connected(client, e);              //有客户端连接
            Service.Disconnected += (client, e) => Disconnected(client, e);      //有客户端断开连接           
            Service.Received += (client, byteBlock, requestInfo) => Received(client, byteBlock, requestInfo); // 接收数据
            Service.Setup(new RRQMConfig()      //载入配置               
                .SetListenIPHosts(new IPHost[] { new IPHost($"{Host1}"), new IPHost($"{Host2}") })//同时监听两个地址
                .SetMaxCount(1000)  // 最大可连接数，默认为10000
                .SetThreadCount(100) // 线程数
                .SetClearInterval(300000) // 清理无数据交互的SocketClient，默认60 * 1000 毫秒。如果不想清除，可使用-1。   这里设为  300000 检查一次。
                )

                .Start();//启动



            Invoke((EventHandler)delegate

            {
                textBox3.AppendText("正在启动成功！");
                textBox3.AppendText("\r\n");

            });



        }






        /// <summary>
        /// client 正在连接事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Connecting(SocketClient client, ClientOperationEventArgs e)
        {

            string c = $"{client.ID}->{client.IP}->{client.Port}->";
            string msg = $"  {c}   在即将完成连接.";

            Invoke((EventHandler)delegate
            {
                textBox3.AppendText(msg);
                textBox3.AppendText("\r\n");

            });



        }

        /// <summary>
        ///  连接完成事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Connected(SocketClient client, RRQMEventArgs e)
        {

            string msg = DateTime.Now + "客户端连接成功";
            dataGridView1.DataSource = null;
            Client_Mode mode = new Client_Mode();    // new 一个 mode
            mode.ID = client.ID;
            mode.Port = client.Port;
            mode.IP = client.IP;
            mode.CanSend = client.CanSend;
            mode.Online = client.Online;
            client_Modes.Add(mode);
            dataGridView1.DataSource = client_Modes;    //填入 dataGridView1


            Invoke((EventHandler)delegate

            {
                textBox3.AppendText(client.IP + "|" + msg);
                textBox3.AppendText("\r\n");

            });



        }

        /// <summary>
        /// 断开事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Disconnected(SocketClient client, MesEventArgs e)
        {
            dataGridView1.DataSource = null;
            string msg = DateTime.Now + "会话断开";
            Invoke((EventHandler)delegate

            {
                textBox3.AppendText(client.ID + "->" + msg);
                textBox3.AppendText("\r\n");

            });




            var c = client_Modes.Where(x => x.ID == client.ID).FirstOrDefault();
            if (c != null) client_Modes.Remove(c);
            if (client_Modes.Count > 0) dataGridView1.DataSource = client_Modes;

        }



        /// <summary>
        ///   //从客户端收到信息 事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        void Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {


            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

            Invoke((EventHandler)delegate

            {
                if (textBox3.Lines.Length == 100) textBox3.Text = String.Empty;
                textBox3.AppendText(DateTime.Now + "\r\n");

                if(mes!= "心跳") textBox3.AppendText($"已从{client.ID}接收到信息：{mes}");  //将收到的信息 放到 textbox  ,心跳 不显示


                textBox3.AppendText("\r\n");

            });

            //client.Send(mes);//将收到的信息直接返回给发送方

            //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

            //var clients = Service.GetClients();
            //foreach (var targetClient in clients)//将收到的信息返回给在线的所有客户端。
            //{
            //    if (targetClient.ID != client.ID)
            //    {

            //        targetClient.Send(mes);
            //        textBox3.AppendText($"To:{client.ID}->发送到信息：{mes}");
            //        textBox3.AppendText("\r\n");
            //    }
            //}

        }


        /// <summary>
        /// 关闭服务品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Button bt = sender as Button;
            bt.Enabled = false;
            button1.Enabled = true;
            Service.Clear();
            Service.Stop();


        }

        /// <summary>
        ///  向指定的Client 发送信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            int i = dataGridView1.CurrentRow.Index;   // 当前选中行 行号
            string sendText = textBox4.Text;
            if (sendText.Length < 0) return;
            string? id = dataGridView1.Rows[i].Cells["ID"].Value.ToString();   // 读取  dataGridView1 当前行 ID 值           

            if (id != string.Empty) Service.Send(id, sendText);
        }




        //-----------------------------------------------------------------------------------

    }
}