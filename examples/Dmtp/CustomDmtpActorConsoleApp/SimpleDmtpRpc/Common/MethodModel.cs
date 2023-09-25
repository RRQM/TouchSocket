using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    class MethodModel
    {
        public MethodModel(MethodInfo method, object target)
        {
            this.Method = method;
            this.Target = target;
        }

        public MethodInfo Method { get; private set; }
        public object Target { get; private set; }
    }
}
