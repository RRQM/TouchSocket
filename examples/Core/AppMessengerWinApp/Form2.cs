using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TouchSocket.Core;

namespace AppMessengerWinApp
{
    public partial class Form2 : Form, IMessageObject
    {
        public Form2()
        {
            InitializeComponent();
        }

        [AppMessage]
        public async Task Say(string msg)
        {
            await Task.CompletedTask;
            this.listBox1.Items.Add(msg);
        }
    }
}
