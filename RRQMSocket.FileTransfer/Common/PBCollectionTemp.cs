//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Generic;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 临时序列化
    /// </summary>
    public class PBCollectionTemp
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public UrlFileInfo UrlFileInfo { get; internal set; }

        /// <summary>
        /// 块集合
        /// </summary>
        public List<FileBlock> Blocks { get; internal set; }

        /// <summary>
        /// 从文件块转换
        /// </summary>
        /// <param name="progressBlocks"></param>
        /// <returns></returns>
        public static PBCollectionTemp GetFromProgressBlockCollection(ProgressBlockCollection progressBlocks)
        {
            if (progressBlocks == null)
            {
                return null;
            }
            PBCollectionTemp collectionTemp = new PBCollectionTemp();
            collectionTemp.UrlFileInfo = progressBlocks.UrlFileInfo;
            collectionTemp.Blocks = new List<FileBlock>();
            collectionTemp.Blocks.AddRange(progressBlocks);
            return collectionTemp;
        }

        /// <summary>
        /// 转换为ProgressBlockCollection
        /// </summary>
        /// <returns></returns>
        public ProgressBlockCollection ToPBCollection()
        {
            ProgressBlockCollection progressBlocks = new ProgressBlockCollection();
            progressBlocks.UrlFileInfo = this.UrlFileInfo;
            if (this.Blocks != null)
            {
                progressBlocks.AddRange(this.Blocks);
            }
            return progressBlocks;
        }
    }
}