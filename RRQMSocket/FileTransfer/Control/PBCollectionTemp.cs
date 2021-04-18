using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public FileInfo FileInfo { get; internal set; }

        /// <summary>
        /// 块集合
        /// </summary>
        public List<FileProgressBlock> Blocks { get; internal set; }

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
            collectionTemp.FileInfo = progressBlocks.FileInfo;
            collectionTemp.Blocks = new List<FileProgressBlock>();
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
            progressBlocks.FileInfo = this.FileInfo;
            if (this.Blocks!=null)
            {
                progressBlocks.AddRange(this.Blocks);
            }
            return progressBlocks;
        }
    }
}
