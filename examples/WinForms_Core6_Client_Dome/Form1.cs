
/// 本项目 基于 RRQM TCP
/// 开发环境  window11   Vs2022 WinFrom Core6 
///  开发者 陈峰
///  微信：ChenFeng_My




using RRQMCore.ByteManager;
using RRQMSocket;
using System.Text;

namespace WinForms_Core6_Client_Dome
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient = new TcpClient();
        public Form1()
        {
            InitializeComponent();
          
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string server_IP = textBox1.Text;
            string server_Port =numericUpDown1.Value.ToString();
            if (server_IP == string.Empty || server_Port == string.Empty) return;  //如果 IP 和端口  为空 则返回

            tcpClient.Connected += (client, e) => Connected(client, e);//成功连接到服务器
            tcpClient.Disconnected += (client, e) => Disconnected(client, e);//从服务器断开连接，当连接不成功时不会触发。
            tcpClient.Received += (client, byteBlock, requestInfo) => Received(client, byteBlock, requestInfo); //从服务器收到信息


            //声明配置
            RRQMConfig config = new RRQMConfig();
            config.SetRemoteIPHost(new IPHost($"{server_IP}:{server_Port}"))
                .UsePlugin()
                .SetBufferLength(1024 * 10);


            //载入配置
            tcpClient.Setup(config);
            tcpClient.Connect();
            tcpClient.Send("   -------------启动成功--------------     ");

            timer1.Enabled = true; //启动定时器
            timer1.Interval = 270000;// 设置定时器 发送心跳 字符 ，保持与服务器连  ,必须小于服务器的 SetClearInterval （清理无数据交互的SocketClient） 值 
            button1.Enabled = false;
            button3.Enabled = true;

        }



        /// <summary>
        /// 连接服务器成功事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Connected(ITcpClientBase client, MesEventArgs e)
        {

            textBox2.AppendText("连接服务器成功！");
        }


        /// <summary>
        /// 断开连接 事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Disconnected(ITcpClientBase client, MesEventArgs e)
        {

            textBox2.AppendText("断开连接 ！！");
        }


        /// <summary>
        /// 接收信息事件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        void Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {


            //从服务器收到信息
            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

            Invoke((EventHandler)delegate
            {
                textBox2.AppendText($"接收到{client.IP}  的信息：{mes}");
                textBox2.AppendText("\r\n");
            });



        }


        /// <summary>
        ///  发送 心跳
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (tcpClient.Online) tcpClient.Send("心跳");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sendMes = textBox3.Text;
            if (sendMes == string.Empty) return;
            tcpClient.Send(sendMes);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tcpClient.Close();
            button3.Enabled = false;
            button1.Enabled = true;    
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //-----------------------------------------------------------
    }
}