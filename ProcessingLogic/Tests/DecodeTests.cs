using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Signals.Tests
{
    [TestFixture]
    internal class DecodeTests
    {
        [Test]
        public void explode()
        {
            var process = new ProcessImage(@"C:\Users\Home\Documents\Audacity\codexparts\guardian_obelisk_08.flac");

            Assert.NotNull(process);
        }

        [Test]
        public void ConfusingNibble()
        {
            byte[] nibble = new byte[]
                            {57,0,176,255};

            var result = BitConverter.ToSingle(nibble, 0);

            Assert.AreNotEqual(float.NaN, result);
        }
    }
}
