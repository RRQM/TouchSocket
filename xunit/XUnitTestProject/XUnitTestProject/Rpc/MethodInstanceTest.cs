using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace XUnitTestProject.Rpc
{
    public class MethodInstanceTest:UnitBase
    {
        [Fact]
        public void FilterMutexAccessTypeShouleBeOk()
        {
            var methodInstance1 = new MethodInstance(typeof(IInterface).GetMethod(nameof(IInterface.Test)));

            Assert.NotNull(methodInstance1);
            Assert.True(methodInstance1.Filters.Length == 1);

            var methodInstance2 = new MethodInstance(typeof(IInterface).GetMethod(nameof(IInterface.Test2)));

            Assert.NotNull(methodInstance2);
            Assert.True(methodInstance2.Filters.Length == 2);

            var methodInstance3 = new MethodInstance(typeof(IInterface).GetMethod(nameof(IInterface.Test3)));

            Assert.NotNull(methodInstance3);
            Assert.True(methodInstance3.Filters.Length == 3);
        }

        [Fact]
        public void FilterRunShouleBeOk()
        {
           
        }

        [My]
        interface IInterface
        {
            [DmtpRpc]
            [My]
            [My2]
            void Test();

            [DmtpRpc]
            [My]
            [My2]
            [My3]
            void Test2();

            [DmtpRpc]
            [My4]
            [My3]
            void Test3();
        }

        class MyClass : IInterface
        {
            [My]
            public void Test()
            {
               
            }

            public void Test2()
            {
                
            }

            public void Test3()
            {
                
            }
        }

        class MyBaseAttribute:RpcActionFilterAttribute
        {
            public override Type[] MutexAccessTypes => new Type[] {typeof(MyBaseAttribute) };
        }

        class MyAttribute: MyBaseAttribute
        {

        }

        class My2Attribute : MyBaseAttribute
        {

        }

        class My3Attribute : RpcActionFilterAttribute
        {

        }

        class My4Attribute : RpcActionFilterAttribute
        {

        }
    }
}
