using System.Collections.Generic;
using System.IO;
using System.Linq;
using GypiAutoUpdater.Gyp.Linq;
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
            Assert.That(doc.Root.Children.First().Name, Is.EqualTo("includes"));
        }
        [Test]
        public void y()
        {
            var doc = GypDocument.Load(new FileInfo(@"F:\src\sdk\rebtel_sdk\trunk\src\main.gyp"));
            var sources = doc.Root.Element("targets").Children.First(). Element("sources").Children.ToList();
            Assert.That(sources[0].Value, Is.EqualTo("<@(rebrtc_header_files)"));
            Assert.That(sources[1].Value, Is.EqualTo("<@(rebrtc_source_files)"));
        }

        [Test]
        public void z()
        {
            var streamEditor = new GypStreamEditor(@"F:\src\sdk\rebtel_sdk\trunk\src\main.gypi", System.Console.Out);
            streamEditor.AddStringToArray("rebrtc_header_files", new [] {"foo.h"});
            streamEditor.Go();
        }

        [Test]
        public void numbers()
        {
            var streamEditor = new GypStreamEditor(@"F:\src\sdk\rebtel_sdk\trunk\gyp\common.gypi", System.Console.Out);
            streamEditor.Go();
        }
    }
}
