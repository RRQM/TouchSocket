using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IDChangedEventArgs
    /// </summary>
    public class IDChangedEventArgs:TouchSocketEventArgs
    {
        /// <summary>
        /// IDChangedEventArgs
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        public IDChangedEventArgs(string oldID, string newID)
        {
            this.OldID = oldID;
            this.NewID = newID;
        }

        /// <summary>
        /// 旧ID
        /// </summary>
        public string OldID { get;private set; }

        /// <summary>
        /// 新ID
        /// </summary>
        public string NewID { get; private set; }
    }
}
