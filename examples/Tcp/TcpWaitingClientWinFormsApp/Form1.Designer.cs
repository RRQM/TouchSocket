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
            this.label1 = new Label();
            this.textBox1 = new TextBox();
            this.textBox2 = new TextBox();
            this.button2 = new Button();
            this.button3 = new Button();
            this.textBox3 = new TextBox();
            this.textBox4 = new TextBox();
            this.groupBox1 = new GroupBox();
            this.button1 = new Button();
            this.button5 = new Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new Point(20, 35);
            this.label1.Margin = new Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new Size(62, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "地址";
            // 
            // textBox1
            // 
            this.textBox1.Location = new Point(88, 31);
            this.textBox1.Margin = new Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new Size(484, 38);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "tcp://127.0.0.1:7789";
            // 
            // textBox2
            // 
            this.textBox2.Location = new Point(24, 42);
            this.textBox2.Margin = new Padding(4);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new Size(358, 38);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "TouchSocket";
            // 
            // button2
            // 
            this.button2.Location = new Point(410, 36);
            this.button2.Margin = new Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new Size(184, 46);
            this.button2.TabIndex = 4;
            this.button2.Text = "发送并等待";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += this.button2_Click;
            // 
            // button3
            // 
            this.button3.Location = new Point(608, 119);
            this.button3.Margin = new Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new Size(328, 46);
            this.button3.TabIndex = 6;
            this.button3.Text = "发送并筛选等待";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += this.button3_Click;
            // 
            // textBox3
            // 
            this.textBox3.Location = new Point(24, 122);
            this.textBox3.Margin = new Padding(4);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new Size(358, 38);
            this.textBox3.TabIndex = 5;
            this.textBox3.Text = "TouchSocket";
            // 
            // textBox4
            // 
            this.textBox4.Location = new Point(410, 122);
            this.textBox4.Margin = new Padding(4);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new Size(180, 38);
            this.textBox4.TabIndex = 7;
            this.textBox4.Text = "Socket";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Location = new Point(12, 102);
            this.groupBox1.Margin = new Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new Padding(4);
            this.groupBox1.Size = new Size(1412, 182);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "等待请求";
            // 
            // button1
            // 
            this.button1.Location = new Point(599, 27);
            this.button1.Name = "button1";
            this.button1.Size = new Size(150, 46);
            this.button1.TabIndex = 9;
            this.button1.Text = "断开";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += this.button1_Click;
            // 
            // button5
            // 
            this.button5.Location = new Point(1112, 35);
            this.button5.Name = "button5";
            this.button5.Size = new Size(150, 46);
            this.button5.TabIndex = 9;
            this.button5.Text = "取消请求";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += this.button5_Click;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(14F, 31F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1437, 325);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Margin = new Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private Button button5;
    }
}