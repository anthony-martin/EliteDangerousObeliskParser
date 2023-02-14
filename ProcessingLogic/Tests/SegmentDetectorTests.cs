using NUnit.Framework;
using Signals;


namespace ProcessingLogic.Tests
{
    [TestFixture]
    internal class SegmentDetectorTests
    {
        private ProcessImage _process;
        private SegmentDetector _segmentDetector;

        [SetUp]
        public void Setup()
        {
            var _filePath = @"C:\Users\Home\Documents\Audacity\codexparts\guardian_obelisk_08.flac";
            _process = new ProcessImage(_filePath);
            var OverlayAboveSement = 660;
            var StartOverlayIndex = 191;
            _process.NormaliseArray(StartOverlayIndex*2, OverlayAboveSement*2);

            _segmentDetector = new SegmentDetector();
        }

        [Test]
        public void FindStart()
        {
            var floatarray = _process.ImageProcessAndConvertToBytes();

            var start = _segmentDetector.FindStart(floatarray);

            Assert.AreEqual( 34, start);
        }

        [Test]
        public void FindEnd()
        {
            var buffer = _process.Buffer;

            var end = _segmentDetector.FindEnd(buffer, 1750);

            Assert.Greater(end, 5000);
        }
    }
}
