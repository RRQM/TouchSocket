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
            button1 = new Button();
            textBox2 = new TextBox();
            button2 = new Button();
            button3 = new Button();
            textBox3 = new TextBox();
            textBox4 = new TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(52, 51);
            label1.Name = "label1";
            label1.Size = new Size(62, 31);
            label1.TabIndex = 0;
            label1.Text = "地址";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(120, 48);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(290, 38);
            textBox1.TabIndex = 1;
            textBox1.Text = "tcp://127.0.0.1:7789";
            // 
            // button1
            // 
            button1.Location = new Point(439, 43);
            button1.Name = "button1";
            button1.Size = new Size(150, 46);
            button1.TabIndex = 2;
            button1.Text = "连接";
            button1.UseVisualStyleBackColor = true;
            button1.Click += this.button1_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(52, 128);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(358, 38);
            textBox2.TabIndex = 3;
            textBox2.Text = "TouchSocket";
            // 
            // button2
            // 
            button2.Location = new Point(439, 123);
            button2.Name = "button2";
            button2.Size = new Size(150, 46);
            button2.TabIndex = 4;
            button2.Text = "发送并等待";
            button2.UseVisualStyleBackColor = true;
            button2.Click += this.button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(636, 204);
            button3.Name = "button3";
            button3.Size = new Size(327, 46);
            button3.TabIndex = 6;
            button3.Text = "发送并筛选等待";
            button3.UseVisualStyleBackColor = true;
            button3.Click += this.button3_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(52, 209);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(358, 38);
            textBox3.TabIndex = 5;
            textBox3.Text = "TouchSocket";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(439, 209);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(150, 38);
            textBox4.TabIndex = 7;
            textBox4.Text = "Socket";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(14F, 31F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1017, 556);
            this.Controls.Add(textBox4);
            this.Controls.Add(button3);
            this.Controls.Add(textBox3);
            this.Controls.Add(button2);
            this.Controls.Add(textBox2);
            this.Controls.Add(button1);
            this.Controls.Add(textBox1);
            this.Controls.Add(label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private TextBox textBox2;
        private Button button2;
        private Button button3;
        private TextBox textBox3;
        private TextBox textBox4;
    }
}