
/// ����Ŀ ���� RRQM TCP
/// ��������  window11   Vs2022 WinFrom Core6 
///  ������ �·�
///  ΢�ţ�ChenFeng_My
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
        TcpService Service = new TcpService();    //  ȫ�� TCP
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
            Server_star();  //����������

        }



        /// <summary>
        /// ����������
        /// </summary>
        void Server_star() 
        {

            textBox3.AppendText("����������������");
           

            string serverIP1 = textBox1.Text;
            string serverIP2 = textBox1.Text;

            string serverPort1 = numericUpDown1.Value.ToString();           
            string serverPort2 = numericUpDown2.Value.ToString();

            string Host1 = $"{serverIP1}:{serverPort1}";
            string Host2 = $"{serverIP2}:{serverPort2}";


            
            string errMsg = string.Empty;   //msg ��¼������Ϣ
            if (serverIP1 == String.Empty) errMsg="����дIP1";
            if (serverIP2 == String.Empty) Host2 = serverPort2;
            if (Convert.ToInt32( serverPort1)==0 ) errMsg = "�˿�1������Ϊ0";
            if (Convert.ToInt32(serverPort2) == 0) errMsg = "�˿�2������Ϊ0";

            // ����д����򷵻�
            if (errMsg != string.Empty)
            {
                MessageBox.Show(errMsg);
                textBox3.AppendText(errMsg);
                return;

            }

            Service.Connecting += (client, e) => Connecting(client, e);             //�пͻ�����������
            Service.Connected += (client, e) => Connected(client, e);              //�пͻ�������
            Service.Disconnected += (client, e) => Disconnected(client, e);      //�пͻ��˶Ͽ�����           
            Service.Received += (client, byteBlock, requestInfo) => Received(client, byteBlock, requestInfo); // ��������
            Service.Setup(new RRQMConfig()      //��������               
                .SetListenIPHosts(new IPHost[] { new IPHost($"{Host1}"), new IPHost($"{Host2}") })//ͬʱ����������ַ
                .SetMaxCount(1000)  // ������������Ĭ��Ϊ10000
                .SetThreadCount(100) // �߳���
                .SetClearInterval(300000) // ���������ݽ�����SocketClient��Ĭ��60 * 1000 ���롣��������������ʹ��-1��   ������Ϊ  300000 ���һ�Ρ�
                )

                .Start();//����



            Invoke((EventHandler)delegate

            {
                textBox3.AppendText("���������ɹ���");
                textBox3.AppendText("\r\n");

            });



        }






        /// <summary>
        /// client ���������¼�
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Connecting(SocketClient client, ClientOperationEventArgs e)
        {

            string c = $"{client.ID}->{client.IP}->{client.Port}->";
            string msg = $"  {c}   �ڼ����������.";

            Invoke((EventHandler)delegate
            {
                textBox3.AppendText(msg);
                textBox3.AppendText("\r\n");

            });



        }

        /// <summary>
        ///  ��������¼�
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Connected(SocketClient client, RRQMEventArgs e)
        {

            string msg = DateTime.Now + "�ͻ������ӳɹ�";
            dataGridView1.DataSource = null;
            Client_Mode mode = new Client_Mode();    // new һ�� mode
            mode.ID = client.ID;
            mode.Port = client.Port;
            mode.IP = client.IP;
            mode.CanSend = client.CanSend;
            mode.Online = client.Online;
            client_Modes.Add(mode);
            dataGridView1.DataSource = client_Modes;    //���� dataGridView1


            Invoke((EventHandler)delegate

            {
                textBox3.AppendText(client.IP + "|" + msg);
                textBox3.AppendText("\r\n");

            });



        }

        /// <summary>
        /// �Ͽ��¼�
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void Disconnected(SocketClient client, MesEventArgs e)
        {
            dataGridView1.DataSource = null;
            string msg = DateTime.Now + "�Ự�Ͽ�";
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
        ///   //�ӿͻ����յ���Ϣ �¼�
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

                if(mes!= "����") textBox3.AppendText($"�Ѵ�{client.ID}���յ���Ϣ��{mes}");  //���յ�����Ϣ �ŵ� textbox  ,���� ����ʾ


                textBox3.AppendText("\r\n");

            });

            //client.Send(mes);//���յ�����Ϣֱ�ӷ��ظ����ͷ�

            //client.Send("id",mes);//���յ�����Ϣ���ظ��ض�ID�Ŀͻ���

            //var clients = Service.GetClients();
            //foreach (var targetClient in clients)//���յ�����Ϣ���ظ����ߵ����пͻ��ˡ�
            //{
            //    if (targetClient.ID != client.ID)
            //    {

            //        targetClient.Send(mes);
            //        textBox3.AppendText($"To:{client.ID}->���͵���Ϣ��{mes}");
            //        textBox3.AppendText("\r\n");
            //    }
            //}

        }


        /// <summary>
        /// �رշ���Ʒ
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
        ///  ��ָ����Client ������Ϣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            int i = dataGridView1.CurrentRow.Index;   // ��ǰѡ���� �к�
            string sendText = textBox4.Text;
            if (sendText.Length < 0) return;
            string? id = dataGridView1.Rows[i].Cells["ID"].Value.ToString();   // ��ȡ  dataGridView1 ��ǰ�� ID ֵ           

            if (id != string.Empty) Service.Send(id, sendText);
        }




        //-----------------------------------------------------------------------------------

    }
}