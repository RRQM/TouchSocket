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
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace FileServiceGUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<TcpTouchRpcSocketClient> m_clients;

        private TcpTouchRpcService m_fileService;

        private ObservableCollection<TransferModel> m_localModels;

        private ObservableCollection<TransferModel> m_remoteModels;

        private TransferModel transferModel;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
        }

        private void FileService_Connected(TcpTouchRpcSocketClient client, TouchSocketEventArgs e)
        {
            this.UIInvoke(() =>
            {
                this.m_clients.Add(client);
            });
        }

        private void FileService_Disconnected(TcpTouchRpcSocketClient client, TouchSocketEventArgs e)
        {
            this.UIInvoke(() =>
            {
                this.m_clients.Remove(client);
            });
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.m_clients = new ObservableCollection<TcpTouchRpcSocketClient>();
            this.m_remoteModels = new ObservableCollection<TransferModel>();
            this.m_localModels = new ObservableCollection<TransferModel>();

            this.ListBox_Clients.ItemsSource = this.m_clients;
        }

        private void ShowMsg(string msg)
        {
            this.UIInvoke(() =>
            {
                this.msgBox.Text = this.msgBox.Text.Insert(0, $"{msg}\r\n");
            });
        }

        private void UIInvoke(Action action)
        {
            this.Dispatcher.Invoke(() =>
            {
                action.Invoke();
            });
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase button = (ButtonBase)sender;

            if (button.Content.Equals("启动服务器"))
            {
                this.m_fileService = new TcpTouchRpcService
                {
                    Connected = this.FileService_Connected,
                    Disconnected = this.FileService_Disconnected
                };

                var config = new TouchSocketConfig();
                config.SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                    .SetVerifyToken("FileService")
                    .UsePlugin()
                    .SetBufferLength(1024 * 1024)
                    .ConfigurePlugins(a =>
                    {
                        a.Add<MyPlugin>();
                        //a.Add<TouchRpcActionPlugin<TcpTouchRpcSocketClient>>()
                        //.SetFileTransfering(this.FileService_FileTransfering);
                    });

                try
                {
                    this.m_fileService.Setup(config);
                    this.m_fileService.Start();
                    button.Content = "停止服务器";
                    this.ShowMsg("启动成功");
                }
                catch (Exception ex)
                {
                    this.ShowMsg(ex.Message);
                }
            }
            else
            {
                this.m_fileService.SafeDispose();
                button.Content = "启动服务器";
                this.ShowMsg("服务器已关闭");
            }
        }
    }
}