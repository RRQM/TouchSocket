using System.Collections.Specialized;

namespace TouchSocket.Http
{
    /// <summary>
    /// 表单文件
    /// </summary>
    public interface IFormFile
    {
        /// <summary>
        /// 获取Content-Disposition
        /// </summary>
        string ContentDisposition { get; }

        /// <summary>
        /// 获取Content-Type
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// 实际的数据
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// 数据对
        /// </summary>
        public NameValueCollection DataPair { get; }

        /// <summary>
        /// 获取file name
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// 文件长度。在数据接收完成之前，该值为-1;
        /// </summary>
        long Length { get; }

        /// <summary>
        ///  获取name字段
        /// </summary>
        string Name { get; }

        ///// <summary>
        ///// 读取文件数据  //太麻烦先不实现
        ///// </summary>
        //public int Read(byte[] buffer, int offset, int count);
    }

    internal class InternalFormFile : IFormFile
    {
        public string ContentDisposition => this.DataPair["Content-Disposition"];

        public string ContentType => this.DataPair["Content-Type"];

        public byte[] Data { get; set; }
        public NameValueCollection DataPair { get; set; }
        public string FileName => this.DataPair["filename"];
        public long Length => Data == null ? 0 : Data.Length;

        public string Name => this.DataPair["name"];
        //public int Read(byte[] buffer, int offset, int count)
        //{
        //    return this.ReadAction(buffer, offset, count);
        //}

        //public Func<byte[], int, int, int> ReadAction { get; set; }
    }
}