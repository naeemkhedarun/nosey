using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CSNosey.Tests
{
    [TestFixture]
    public class SemVerReaderTests
    {

        [Test]
        public void ShouldConvertSemVewFileToVersion()
        {
            var content = File.ReadAllText(".semver");
            var semVerReader = new SemVerReader(content);
            var version = semVerReader.GetVersion();

            Assert.That(version, Is.EqualTo(new Version(0,1,0)));
        }

    }
}
