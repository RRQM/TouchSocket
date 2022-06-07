
/// ����Ŀ ���� RRQM TCP
/// ��������  window11   Vs2022 WinFrom Core6 
///  ������ �·�
///  ΢�ţ�ChenFeng_My




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
            if (server_IP == string.Empty || server_Port == string.Empty) return;  //��� IP �Ͷ˿�  Ϊ�� �򷵻�

            tcpClient.Connected += (client, e) => Connected(client, e);//�ɹ����ӵ�������
            tcpClient.Disconnected += (client, e) => Disconnected(client, e);//�ӷ������Ͽ����ӣ������Ӳ��ɹ�ʱ���ᴥ����
            tcpClient.Received += (client, byteBlock, requestInfo) => Received(client, byteBlock, requestInfo); //�ӷ������յ���Ϣ


            //��������
            RRQMConfig config = new RRQMConfig();
            config.SetRemoteIPHost(new IPHost($"{server_IP}:{server_Port}"))
                .UsePlugin()
                .SetBufferLength(1024 * 10);


            //��������
            tcpClient.Setup(config);
            tcpClient.Connect();
            tcpClient.Send("   -------------�����ɹ�--------------     ");

            timer1.Enabled = true; //������ʱ��
            timer1.Interval = 270000;// ���ö�ʱ�� �������� �ַ� ���������������  ,����С�ڷ������� SetClearInterval �����������ݽ�����SocketClient�� ֵ 
            button1.Enabled = false;
            button3.Enabled = true;

        }



        /// <summary>
        /// ���ӷ������ɹ��¼�
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Connected(ITcpClientBase client, MesEventArgs e)
        {

            textBox2.AppendText("���ӷ������ɹ���");
        }


        /// <summary>
        /// �Ͽ����� �¼�
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Disconnected(ITcpClientBase client, MesEventArgs e)
        {

            textBox2.AppendText("�Ͽ����� ����");
        }


        /// <summary>
        /// ������Ϣ�¼�
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        void Received(TcpClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {


            //�ӷ������յ���Ϣ
            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

            Invoke((EventHandler)delegate
            {
                textBox2.AppendText($"���յ�{client.IP}  ����Ϣ��{mes}");
                textBox2.AppendText("\r\n");
            });



        }


        /// <summary>
        ///  ���� ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (tcpClient.Online) tcpClient.Send("����");

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