using TouchSocket.Rpc;

namespace GenerateProxyFromServerConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var rpcStore = new RpcStore(new TouchSocket.Core.Container());

            rpcStore.RegisterServer<MyRpcClass>();
        }
    }

    internal partial class MyRpcClass : RpcServer
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    internal class MyRpcAttribute : RpcAttribute
    {
        public MyRpcAttribute()
        {
            this.GeneratorFlag = CodeGeneratorFlag.ExtensionAsync | CodeGeneratorFlag.InstanceAsync;
        }

        public override Type[] GetGenericConstraintTypes()
        {
            return new Type[] { typeof(IRpcClient) };
        }

        public override string GetDescription(RpcMethod methodInstance)
        {
            return base.GetDescription(methodInstance);
        }
    }
}