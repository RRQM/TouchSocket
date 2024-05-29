namespace RemoteAccessApp
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
            button4 = new Button();
            button5 = new Button();
            button3 = new Button();
            button2 = new Button();
            textBox1 = new TextBox();
            button1 = new Button();
            listBox1 = new ListBox();
            this.SuspendLayout();
            // 
            // button4
            // 
            button4.Location = new Point(405, 47);
            button4.Margin = new Padding(2);
            button4.Name = "button4";
            button4.Size = new Size(97, 25);
            button4.TabIndex = 15;
            button4.Text = "获取文件信息";
            button4.UseVisualStyleBackColor = true;
            button4.Click += this.button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(299, 47);
            button5.Margin = new Padding(2);
            button5.Name = "button5";
            button5.Size = new Size(102, 25);
            button5.TabIndex = 14;
            button5.Text = "删除文件";
            button5.UseVisualStyleBackColor = true;
            button5.Click += this.button5_Click;
            // 
            // button3
            // 
            button3.Location = new Point(405, 8);
            button3.Margin = new Padding(2);
            button3.Name = "button3";
            button3.Size = new Size(97, 25);
            button3.TabIndex = 13;
            button3.Text = "获取文件夹信息";
            button3.UseVisualStyleBackColor = true;
            button3.Click += this.button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(299, 8);
            button2.Margin = new Padding(2);
            button2.Name = "button2";
            button2.Size = new Size(102, 25);
            button2.TabIndex = 12;
            button2.Text = "删除文件夹";
            button2.UseVisualStyleBackColor = true;
            button2.Click += this.button2_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(11, 11);
            textBox1.Margin = new Padding(2);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(182, 23);
            textBox1.TabIndex = 11;
            // 
            // button1
            // 
            button1.Location = new Point(201, 8);
            button1.Margin = new Padding(2);
            button1.Name = "button1";
            button1.Size = new Size(94, 25);
            button1.TabIndex = 10;
            button1.Text = "创建文件夹";
            button1.UseVisualStyleBackColor = true;
            button1.Click += this.button1_Click;
            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 17;
            listBox1.Location = new Point(3, 84);
            listBox1.Margin = new Padding(2);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(502, 208);
            listBox1.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(7F, 17F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(509, 303);
            this.Controls.Add(button4);
            this.Controls.Add(button5);
            this.Controls.Add(button3);
            this.Controls.Add(button2);
            this.Controls.Add(textBox1);
            this.Controls.Add(button1);
            this.Controls.Add(listBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Button button4;
        private Button button5;
        private Button button3;
        private Button button2;
        private TextBox textBox1;
        private Button button1;
        private ListBox listBox1;
    }
}