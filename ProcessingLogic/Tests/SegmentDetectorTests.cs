using NUnit.Framework;
using Signals;
using System.Drawing;
using System.Resources;

namespace ProcessingLogic.Tests
{
    [TestFixture]
    internal class SegmentDetectorTests8
    {
        private ProcessImage _process;
        private SegmentDetector _segmentDetector;

        private float[] _buffer;

        [SetUp]
        public void Setup()
        {
            System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();

            var resources = thisExe.GetManifestResourceNames();
            System.IO.Stream file =
                thisExe.GetManifestResourceStream("ProcessingLogic.Tests.resources.Test8.bmp");
            var bitmap = new Bitmap(file);

            _process = new ProcessImage();

            _buffer = _process.BitmapToByte(bitmap);

            _segmentDetector = new SegmentDetector();
        }

        [Test]
        public void FindStart()
        {
            var start = _segmentDetector.FindStart(_buffer);

            Assert.AreEqual( 34, start);
        }

        [Test]
        public void FindEnd()
        {

            var end = _segmentDetector.FindEnd(_buffer, 0);

            Assert.AreEqual(end, 1454);
        }

        [Test]
        public void FindSegmemts()
        {
            var end = _segmentDetector.FindSeperators(_buffer, 33, 1454);

            Assert.AreEqual(7, end.Count);
        }
    }

    [TestFixture]
    internal class SegmentDetectorTests1
    {
        private ProcessImage _process;
        private SegmentDetector _segmentDetector;

        private float[] _buffer;

        [SetUp]
        public void Setup()
        {
            System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();

            var resources = thisExe.GetManifestResourceNames();
            System.IO.Stream file =
                thisExe.GetManifestResourceStream("ProcessingLogic.Tests.resources.Test1.bmp");
            var bitmap = new Bitmap(file);

            _process = new ProcessImage();

            _buffer = _process.BitmapToByte(bitmap);

            _segmentDetector = new SegmentDetector();
        }

        [Test]
        public void FindStart()
        {
            var start = _segmentDetector.FindStart(_buffer);

            Assert.AreEqual(44, start);
        }

        [Test]
        public void FindEnd()
        {

            var end = _segmentDetector.FindEnd(_buffer, 0);

            Assert.AreEqual(end, 1162);
        }

        [Test]
        public void FindSegmemts()
        {
            var end = _segmentDetector.FindSeperators(_buffer, 0, 842);

            Assert.AreEqual(1, end.Count);
        }
    }


    [TestFixture]
    internal class SegmentDetectorTests2
    {
        private ProcessImage _process;
        private SegmentDetector _segmentDetector;

        private float[] _buffer;

        [SetUp]
        public void Setup()
        {
            System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();

            var resources = thisExe.GetManifestResourceNames();
            System.IO.Stream file =
                thisExe.GetManifestResourceStream("ProcessingLogic.Tests.resources.Test2.bmp");
            var bitmap = new Bitmap(file);

            _process = new ProcessImage();

            _buffer = _process.BitmapToByte(bitmap);

            _segmentDetector = new SegmentDetector();
        }

        [Test]
        public void FindStart()
        {
            var start = _segmentDetector.FindStart(_buffer);

            Assert.AreEqual(34, start);
        }

        [Test]
        public void FindEnd()
        {

            var end = _segmentDetector.FindEnd(_buffer, 0);

            Assert.AreEqual(end, 1454);
        }

        [Test]
        public void FindSegmemts()
        {
            var end = _segmentDetector.FindSeperators(_buffer, 33, 1454);

            Assert.AreEqual(7, end.Count);
        }
    }


    [TestFixture]
    internal class SegmentDetectorTests27
    {
        private ProcessImage _process;
        private SegmentDetector _segmentDetector;

        private float[] _buffer;

        [SetUp]
        public void Setup()
        {
            System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();

            var resources = thisExe.GetManifestResourceNames();
            System.IO.Stream file =
                thisExe.GetManifestResourceStream("ProcessingLogic.Tests.resources.Test27.bmp");
            var bitmap = new Bitmap(file);

            _process = new ProcessImage();

            _buffer = _process.BitmapToByte(bitmap);

            _segmentDetector = new SegmentDetector();
        }

        [Test]
        public void FindStart()
        {
            var start = _segmentDetector.FindStart(_buffer);

            Assert.AreEqual(55, start);
        }

        [Test]
        public void FindEnd()
        {

            var end = _segmentDetector.FindEnd(_buffer, 0);

            Assert.AreEqual(end, 2270);
        }

        [Test]
        public void FindSegmemts()
        {
            var end = _segmentDetector.FindSeperators(_buffer, 33, 2270);

            Assert.AreEqual(8, end.Count);
        }
    }
}
