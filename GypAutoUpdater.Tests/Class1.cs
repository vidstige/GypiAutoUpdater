using System.IO;
using GypiAutoUpdater.Model;
using NUnit.Framework;

namespace GypAutoUpdater.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void x()
        {
            var doc = GypDocument.Load(new FileInfo(@"F:\src\sdk\rebtel_sdk\trunk\rebrtc.gyp"));
        }
    }
}
