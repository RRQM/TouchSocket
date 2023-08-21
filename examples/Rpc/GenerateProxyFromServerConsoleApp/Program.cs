using TouchSocket.Rpc;

namespace GenerateProxyFromServerConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rpcStore = new RpcStore(new TouchSocket.Core.Container());

            rpcStore.RegisterServer<MyRpcClass>();
        }
    }

    class MyRpcClass : RpcServer
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    class MyRpcAttribute : RpcAttribute
    {
        public MyRpcAttribute()
        {
            this.GeneratorFlag = CodeGeneratorFlag.ExtensionAsync | CodeGeneratorFlag.InstanceAsync; 
        }
        public override Type[] GetGenericConstraintTypes()
        {
            return new Type[] { typeof(IRpcClient) };
        }

        public override string GetDescription(MethodInstance methodInstance)
        {
            return base.GetDescription(methodInstance);
        }
    }
}