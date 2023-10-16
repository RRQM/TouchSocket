using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace XUnitTestProject
{
    public class UnitBaseOutput:UnitBase
    {
        public UnitBaseOutput(ITestOutputHelper testOutputHelper)
        {
            this.OutputHelper = testOutputHelper;
        }

        public ITestOutputHelper OutputHelper { get; }
    }
}
