using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Hosting;

namespace TouchSocket.Core
{
    /// <summary>
    /// AspnetcoreLoggerExtension
    /// </summary>
    public static class AspnetcoreLoggerExtension
    {
        public static void AddAspnetcoreLogger(this IRegistrator registrator)
        {
            registrator.RegisterSingleton<ILog,AspnetcoreLogger>();
        }
    }
}
