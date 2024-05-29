using System.Reflection;

namespace CustomDmtpActorConsoleApp.SimpleDmtpRpc
{
    internal class MethodModel
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