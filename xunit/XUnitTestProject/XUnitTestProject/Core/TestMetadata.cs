using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestMetadata
    {
        [Fact]
        public void AddShouldBeOk()
        {
            Metadata metadata = new Metadata();
            metadata.Add("1", "1");

            Assert.Equal(1, metadata.Count);
            Assert.Equal("1", metadata["1"]);

            metadata.Add("1", "2");

            Assert.Equal(1, metadata.Count);

            Assert.Equal("2", metadata["1"]);
        }
    }
}
