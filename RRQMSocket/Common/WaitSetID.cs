using RRQMCore.Run;

namespace RRQMSocket
{
    /// <summary>
    /// 设置ID
    /// </summary>
    public class WaitSetID : WaitResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        public WaitSetID(string oldID, string newID)
        {
            this.OldID = oldID;
            this.NewID = newID;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitSetID()
        {
        }


        /// <summary>
        /// 旧ID
        /// </summary>
        public string OldID { get; set; }

        /// <summary>
        /// 新ID
        /// </summary>
        public string NewID { get; set; }
    }
}