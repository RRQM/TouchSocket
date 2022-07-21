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
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TouchSocket.Core;
using TouchSocket.Core.IO;
using TouchSocket.Core.Run;
using TouchSocket.Rpc.TouchRpc;

namespace FileServiceGUI.Models
{
    public class TransferModel : RRQMSkin.MVVM.ObservableObject
    {
        public string FileName => string.IsNullOrEmpty(this.FilePath) ? null : Path.GetFileName(this.FilePath);
        public string FilePath { get; set; }
        public string FileLength { get; set; }

        private string speed;

        public string Speed
        {
            get => this.speed;
            set => this.SetProperty(ref this.speed, value);
        }

        private float progress;

        public float Progress
        {
            get => this.progress;
            set => this.SetProperty(ref this.progress, value);
        }

        private ImageSource status;

        public ImageSource Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        public FileOperator FileOperator { get; set; }

        public TransferType TransferType { get; set; }

        private string mes;

        public string Mes
        {
            get => this.mes;
            set => this.SetProperty(ref this.mes, value);
        }

        public void Start()
        {
            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
              {
                  if (this.FileOperator.Result.ResultCode == ResultCode.Default)
                  {
                      this.Progress = this.FileOperator.Progress;
                      this.Speed = FileUtility.ToFileLengthString(this.FileOperator.Speed());
                  }
                  else if (this.FileOperator.Result.ResultCode == ResultCode.Success)
                  {
                      App.Current.Dispatcher.Invoke(() =>
                      {
                          this.Status = new BitmapImage(new Uri("Resources/Images/完成.png", UriKind.RelativeOrAbsolute));
                      });
                      this.Progress = 1;
                      loop.Dispose();
                  }
                  else
                  {
                      App.Current.Dispatcher.Invoke(() =>
                      {
                          this.Status = new BitmapImage(new Uri("Resources/Images/未完成.png", UriKind.RelativeOrAbsolute));
                      });
                      this.Mes = this.FileOperator.Result.ToString();
                      loop.Dispose();
                  }
              });

            loopAction.RunAsync();
        }
    }
}