using System.IO;
using System.Linq;
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
            Assert.That(doc.Root.Elements.First().Name, Is.EqualTo("includes"));
        }
        [Test]
        public void y()
        {
            var doc = GypDocument.Load(new FileInfo(@"F:\src\sdk\rebtel_sdk\trunk\rebrtc.gyp"));
            Assert.That(doc.Root.Elements.First().Elements.First().Value, Is.EqualTo("gyp/common.gypi"));
        }
    }
}
