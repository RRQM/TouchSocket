using AppMessengerWinApp;
using TouchSocket.Core;

namespace WinAppForAppMessenger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var form = new Form2();
            AppMessenger.Default.Register(form);
            form.Show();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await AppMessenger.Default.SendAsync("Say", "Hello");
        }
    }
}