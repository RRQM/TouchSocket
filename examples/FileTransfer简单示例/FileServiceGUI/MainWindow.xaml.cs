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
using FileServiceGUI.Models;
using FileServiceGUI.Win;
using RRQMSkin.MVVM;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.IO;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace FileServiceGUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.clients = new RRQMList<TcpTouchRpcSocketClient>();
            this.remoteModels = new RRQMList<TransferModel>();
            this.localModels = new RRQMList<TransferModel>();

            this.ListBox_Clients.ItemsSource = this.clients;
            this.ListBox_RemoteTransfer.ItemsSource = this.remoteModels;
            this.ListBox_LocalTransfer.ItemsSource = this.localModels;

            this.Start();
        }

        private RRQMList<TcpTouchRpcSocketClient> clients;
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

        private TcpTouchRpcService fileService;

        private void Start()
        {
            //启动
            if (this.fileService != null)
            {
                return;
            }
            this.fileService = new TcpTouchRpcService();
            this.fileService.Received += this.FileService_Received;
            this.fileService.Connected += this.FileService_Connected;
            this.fileService.Disconnected += this.FileService_Disconnected;
            this.fileService.FileTransfering += this.FileService_FileTransfering;
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .SetVerifyToken("FileService");

            try
            {
                this.fileService.Setup(config);
                this.fileService.Start();
                this.ShowMsg("启动成功");
            }
            catch (Exception ex)
            {
                this.ShowMsg(ex.Message);
            }
        }

        private void FileService_Received(TcpTouchRpcSocketClient socketClient, short protocol, ByteBlock byteBlock)
        {
            this.ShowMsg($"收到数据：协议={protocol},数据长度:{byteBlock.Len - 2}");
            if (protocol == -1)
            {
                socketClient.Send(byteBlock.ToArray(2));
            }
            else
            {
                socketClient.Send(protocol, byteBlock.ToArray(2));
            }
        }

        private void FileService_FileTransfering(TcpTouchRpcSocketClient client, FileOperationEventArgs e)
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

            this.UIInvoke(() =>
            {
                this.remoteModels.Add(model);
            });
        }

        private void FileService_Disconnected(TcpTouchRpcSocketClient client, TouchSocketEventArgs e)
        {
            this.UIInvoke(() =>
            {
                this.clients.Remove(client);
            });
        }

        private void FileService_Connected(TcpTouchRpcSocketClient client, TouchSocketEventArgs e)
        {
            this.UIInvoke(() =>
            {
                this.clients.Add(client);
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
            if (this.transferModel == null || this.transferModel.FileOperator.Result.ResultCode != ResultCode.Default)
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
                this.ShowMsg(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //取消
            if (this.transferModel == null || this.transferModel.FileOperator.Result.ResultCode != ResultCode.Default)
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
                this.ShowMsg(ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Push
            if (this.ListBox_Clients.SelectedItem is TcpTouchRpcSocketClient client)
            {
                PushWindow pushWindow = new PushWindow();
                if (pushWindow.SelectRequest(out FileRequest fileRequest))
                {
                    if (this.cb_resume.IsChecked == true)
                    {
                        fileRequest.Flags = TransferFlags.BreakpointResume;
                    }
                    FileOperator fileOperator = new FileOperator();

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
                    client.PushFileAsync(fileRequest, fileOperator);
                }
            }
            else
            {
                MessageBox.Show("请选择一个客户端");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Pull
            if (this.ListBox_Clients.SelectedItem is TcpTouchRpcSocketClient client)
            {
                PullWindow pullWindow = new PullWindow();
                if (pullWindow.SelectRequest(out FileRequest fileRequest))
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
                    client.PullFileAsync(fileRequest, fileOperator);
                }
            }
            else
            {
                MessageBox.Show("请选择一个客户端");
            }
        }
    }
}