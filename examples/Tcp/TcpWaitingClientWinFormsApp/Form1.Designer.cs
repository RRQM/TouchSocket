//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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
            button1 = new Button();
            button4 = new Button();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 35);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(62, 31);
            label1.TabIndex = 0;
            label1.Text = "地址";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(88, 31);
            textBox1.Margin = new Padding(4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(484, 38);
            textBox1.TabIndex = 1;
            textBox1.Text = "tcp://127.0.0.1:7789";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(24, 42);
            textBox2.Margin = new Padding(4);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(358, 38);
            textBox2.TabIndex = 3;
            textBox2.Text = "TouchSocket";
            // 
            // button2
            // 
            button2.Location = new Point(410, 36);
            button2.Margin = new Padding(4);
            button2.Name = "button2";
            button2.Size = new Size(184, 46);
            button2.TabIndex = 4;
            button2.Text = "发送并等待";
            button2.UseVisualStyleBackColor = true;
            button2.Click += this.button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(608, 119);
            button3.Margin = new Padding(4);
            button3.Name = "button3";
            button3.Size = new Size(328, 46);
            button3.TabIndex = 6;
            button3.Text = "发送并筛选等待";
            button3.UseVisualStyleBackColor = true;
            button3.Click += this.button3_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(24, 122);
            textBox3.Margin = new Padding(4);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(358, 38);
            textBox3.TabIndex = 5;
            textBox3.Text = "TouchSocket";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(410, 122);
            textBox4.Margin = new Padding(4);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(180, 38);
            textBox4.TabIndex = 7;
            textBox4.Text = "Socket";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button4);
            groupBox1.Controls.Add(button3);
            groupBox1.Controls.Add(textBox4);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Location = new Point(12, 102);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new Size(1412, 182);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "异步同步请求";
            // 
            // button1
            // 
            button1.Location = new Point(599, 27);
            button1.Name = "button1";
            button1.Size = new Size(150, 46);
            button1.TabIndex = 9;
            button1.Text = "断开";
            button1.UseVisualStyleBackColor = true;
            button1.Click += this.button1_Click;
            // 
            // button4
            // 
            button4.Location = new Point(944, 119);
            button4.Margin = new Padding(4);
            button4.Name = "button4";
            button4.Size = new Size(328, 46);
            button4.TabIndex = 8;
            button4.Text = "发送并筛选异步等待";
            button4.UseVisualStyleBackColor = true;
            button4.Click += this.button4_Click;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(14F, 31F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1437, 325);
            this.Controls.Add(button1);
            this.Controls.Add(groupBox1);
            this.Controls.Add(textBox1);
            this.Controls.Add(label1);
            this.Margin = new Padding(4);
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
        private Button button1;
        private Button button4;
    }
}