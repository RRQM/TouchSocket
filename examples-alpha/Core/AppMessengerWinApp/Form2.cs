using TouchSocket.Core;

namespace AppMessengerWinApp
{
    public partial class Form2 : Form, IMessageObject
    {
        public Form2()
        {
            this.InitializeComponent();
        }

        [AppMessage]
        public async Task Say(string msg)
        {
            await Task.CompletedTask;
            this.listBox1.Items.Add(msg);
        }
    }
}