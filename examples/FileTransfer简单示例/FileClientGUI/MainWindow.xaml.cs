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
using FileClientGUI.Models;
using FileClientGUI.Win;
using RRQMSkin.MVVM;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace FileClientGUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.remoteModels = new RRQMList<TransferModel>();
            this.localModels = new RRQMList<TransferModel>();

            this.ListBox_RemoteTransfer.ItemsSource = this.remoteModels;
            this.ListBox_LocalTransfer.ItemsSource = this.localModels;
        }

        private RRQMList<TransferModel> remoteModels;
        private RRQMList<TransferModel> localModels;

        private void ShowMsg(string msg)
        {
            this.UIInvoke(() =>
            {
                this.msgBox.AppendText($"{msg}\r\n");
            });
        }

        private void UIInvoke(Action action)
        {
            this.Dispatcher.Invoke(() =>
            {
                action.Invoke();
            });
        }

        private TcpTouchRpcClient fileClient;

        private static int id = 0;

        private void ConButton_Click(object sender, RoutedEventArgs e)
        {
            fileClient = new TcpTouchRpcClient();
            fileClient.Disconnected = this.FileClient_Disconnected;
            fileClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .SetVerifyToken("FileService")
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add<TouchRpcActionPlugin<TcpTouchRpcClient>>()//此处的逻辑可用插件替代完成。
                       .SetFileTransfering(this.FileClient_BeforeFileTransfer)
                       .SetReceivedProtocolData(FileClient_Received);
                }));

            try
            {
                fileClient.Connect();

                ((Button)sender).IsEnabled = false;

                fileClient.ResetID((++id).ToString());
                ShowMsg($"连接成功，ID={this.fileClient.ID}");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void FileClient_Disconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
        }

        private void FileClient_Received(TcpTouchRpcClient client, ProtocolDataEventArgs e)
        {
            ShowMsg($"收到数据：协议={e.Protocol},数据长度:{e.ByteBlock.Len - 2}");
        }

        private void FileClient_BeforeFileTransfer(TcpTouchRpcClient client, FileOperationEventArgs e)
        {
            TransferModel model = new TransferModel();
            model.FileOperator = e.FileOperator;
            model.TransferType = e.TransferType;

            switch (e.TransferType)
            {
                case TransferType.Push:
                    model.FilePath = e.FileRequest.SavePath;
                    model.FileLength = FileUtility.ToFileLengthString(e.FileInfo.FileLength);
                    break;

                case TransferType.Pull:
                    model.FilePath = e.FileRequest.Path;
                    model.FileLength = FileUtility.ToFileLengthString(new FileInfo(e.FileRequest.Path).Length);
                    break;

                default:
                    break;
            }
            model.Start();

            UIInvoke(() =>
            {
                this.remoteModels.Add(model);
            });
        }

        private TransferModel transferModel;

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem is TransferModel transferModel)
            {
                this.transferModel = transferModel;
            }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (transferModel == null || transferModel.FileOperator.Result.ResultCode != ResultCode.Default)
            {
                MessageBox.Show("请选择一个条目，然后控制。");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(((TextBox)sender).Text))
                {
                    return;
                }
                //this.transferModel.FileOperator.SetMaxSpeed(int.Parse(((TextBox)sender).Text));
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PushWindow pushWindow = new PushWindow();
            if (pushWindow.SelectRequest(out FileRequest fileRequest, out string clientID))
            {
                if (this.cb_resume.IsChecked == true)
                {
                    fileRequest.Flags = TransferFlags.BreakpointResume;
                }
                FileOperator fileOperator = new FileOperator();
                //fileOperator.SetMaxSpeed(10240);
                FileInfo fileInfo = new FileInfo(fileRequest.Path);
                TransferModel model = new TransferModel()
                {
                    FileLength = FileUtility.ToFileLengthString(fileInfo.Length),
                    FilePath = fileRequest.Path,
                    FileOperator = fileOperator,
                    TransferType = TransferType.Push
                };
                model.Start();

                this.localModels.Add(model);
                if (string.IsNullOrEmpty(clientID))
                {
                    this.fileClient.PushFileAsync(fileRequest, fileOperator);
                }
                else
                {
                    this.fileClient.PushFileAsync(clientID, fileRequest, fileOperator);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            PullWindow pullWindow = new PullWindow();
            if (pullWindow.SelectRequest(out FileRequest fileRequest, out string clientID))
            {
                if (this.cb_resume.IsChecked == true)
                {
                    fileRequest.Flags = TransferFlags.BreakpointResume;
                }
                FileOperator fileOperator = new FileOperator();

                TransferModel model = new TransferModel()
                {
                    FileLength = "未知",
                    FilePath = fileRequest.Path,
                    FileOperator = fileOperator,
                    TransferType = TransferType.Pull
                };
                model.Start();

                this.localModels.Add(model);

                if (string.IsNullOrEmpty(clientID))
                {
                    this.fileClient.PullFileAsync(fileRequest, fileOperator);
                }
                else
                {
                    this.fileClient.PullFileAsync(clientID, fileRequest, fileOperator);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //取消
            if (transferModel == null || transferModel.FileOperator.Result.ResultCode != ResultCode.Default)
            {
                MessageBox.Show("请选择一个条目，然后控制。");
                return;
            }
            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                this.transferModel.FileOperator.Token = tokenSource.Token;
                tokenSource.Cancel();
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.fileClient.Send(10, new byte[] { 1, 2, 3 });
        }

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this.fileClient.Ping().ToString());
        }
    }
}