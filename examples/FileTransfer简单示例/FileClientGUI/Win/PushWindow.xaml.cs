//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using Microsoft.Win32;
using System.Windows;

namespace FileClientGUI.Win
{
    /// <summary>
    /// PushWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PushWindow : Window
    {
        public PushWindow()
        {
            InitializeComponent();
        }

        public bool SelectRequest(out string path, out string savePath,out string targetId)
        {
            this.ShowDialog();
            path = this.path;
            savePath = this.savePath;
            targetId = this.targetId;
            return this.go;
        }

        private string savePath;
        private string path;
        private string targetId;
        private bool go;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.path = this.tb1.Text;
            this.savePath = this.tb2.Text;
            this.targetId = this.tb3.Text;
            this.go = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                this.tb1.Text = dialog.FileName;
                this.tb2.Text = System.IO.Path.GetFileName(dialog.FileName);
            }
        }
    }
}