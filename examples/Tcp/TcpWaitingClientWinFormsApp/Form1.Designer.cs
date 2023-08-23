namespace TcpWaitingClientWinFormsApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            button2 = new Button();
            button3 = new Button();
            textBox3 = new TextBox();
            textBox4 = new TextBox();
            groupBox1 = new GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 19);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(32, 17);
            label1.TabIndex = 0;
            label1.Text = "地址";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(44, 17);
            textBox1.Margin = new Padding(2, 2, 2, 2);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(244, 23);
            textBox1.TabIndex = 1;
            textBox1.Text = "tcp://127.0.0.1:7789";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 23);
            textBox2.Margin = new Padding(2, 2, 2, 2);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(181, 23);
            textBox2.TabIndex = 3;
            textBox2.Text = "TouchSocket";
            // 
            // button2
            // 
            button2.Location = new Point(205, 20);
            button2.Margin = new Padding(2, 2, 2, 2);
            button2.Name = "button2";
            button2.Size = new Size(92, 25);
            button2.TabIndex = 4;
            button2.Text = "发送并等待";
            button2.UseVisualStyleBackColor = true;
            button2.Click += this.button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(304, 65);
            button3.Margin = new Padding(2, 2, 2, 2);
            button3.Name = "button3";
            button3.Size = new Size(164, 25);
            button3.TabIndex = 6;
            button3.Text = "发送并筛选等待";
            button3.UseVisualStyleBackColor = true;
            button3.Click += this.button3_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(12, 67);
            textBox3.Margin = new Padding(2, 2, 2, 2);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(181, 23);
            textBox3.TabIndex = 5;
            textBox3.Text = "TouchSocket";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(205, 67);
            textBox4.Margin = new Padding(2, 2, 2, 2);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(92, 23);
            textBox4.TabIndex = 7;
            textBox4.Text = "Socket";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button3);
            groupBox1.Controls.Add(textBox4);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Location = new Point(6, 56);
            groupBox1.Margin = new Padding(2, 2, 2, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(2, 2, 2, 2);
            groupBox1.Size = new Size(474, 100);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "异步同步请求";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(7F, 17F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(488, 178);
            this.Controls.Add(groupBox1);
            this.Controls.Add(textBox1);
            this.Controls.Add(label1);
            this.Margin = new Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button button2;
        private Button button3;
        private TextBox textBox3;
        private TextBox textBox4;
        private GroupBox groupBox1;
    }
}