using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public abstract class DependencyPropertyBase
    {
        private static int s_idCount = 0;

        public DependencyPropertyBase()
        {
            this.Id = Interlocked.Increment(ref s_idCount);
        }

        /// <summary>
        /// 标识属性的唯一
        /// </summary>
        public int Id { get; }
    }
}