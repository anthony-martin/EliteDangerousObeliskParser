
using NAudio.Dsp;
using NAudio.Flac;
using NAudio.Wave;
using ProcessingLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Signals
{
    public class ProcessImage
    {
        private List<float[]> _fftBuffer;
        private int _fftLength = 32784;
        private int _fftCompexity = 15;
        private int _fftLengthBytes ;
        private int _bytesPerSameple;

        private int _bitmapWidth = 4096;
        private int _bitmapHeight = 1024;

        public ProcessImage()
        {
            _bitmapHeight = _fftLength / 2;

        }

        public ProcessImage(string filename)
        {
            _fftBuffer = new List<float[]>();

            using (var audioFile = new AudioFileReader(filename))
            {
                var format = audioFile.WaveFormat;
                _bytesPerSameple = format.BitsPerSample / 8;
                var bytesPerSecond = format.AverageBytesPerSecond;
                audioFile.Volume = 1.0f;
                //here we double the buffer for stereo as we are going to skip half the data
                _fftLengthBytes = _fftLength * _bytesPerSameple * format.Channels;
                int readSegment = Math.Min(_fftLengthBytes, bytesPerSecond / 1000);
                

                var buffer = new byte[_fftLengthBytes];
                var read = audioFile.Read(buffer, 0, _fftLengthBytes);
                while (read >= readSegment)
                {
                    Complex[] complex = new Complex[_fftLength];
                    for (int i = 0; i < _fftLength ; i++)
                    {
                        float reading = 0;
                        if (format.Channels == 2)
                        {
                            //todo add config for this to select channel
                            
                            reading = BitConverter.ToSingle(buffer, (i * format.Channels) * _bytesPerSameple);
                            reading += BitConverter.ToSingle(buffer, (i * format.Channels + 1) * _bytesPerSameple);
                        }
                        else
                        {
                            reading = BitConverter.ToSingle(buffer, i * _bytesPerSameple);
                        }
                         complex[i] = new Complex
                        {
                           
                            X = Convert.ToSingle(reading * FastFourierTransform.HammingWindow(i, _fftLength)),
                            Y = 0

                        };
                        //  Y = Convert.ToSingle(BitConverter.ToSingle(buffer, i * bytesPerSameple +2 )) } ;
                    }


                    FastFourierTransform.FFT(true, _fftCompexity, complex);
                    var array = new float[_fftLength/2];
                    for (int i = 0; i < _fftLength / 2; i++)
                    {
                        array[i] = complex[i].X;
                    }
                    _fftBuffer.Add(array);
                    var readBuffer = new byte[readSegment];

                    read = audioFile.Read(readBuffer, 0, readSegment);
                    //shift the buffer 
                    //Copy all the values left 1 read segment length. 
                    Array.Copy(buffer, readSegment , buffer, 0, _fftLengthBytes - readSegment);
                    // add the new segment add the read segment to the end of the buffer. 
                    Array.Copy(readBuffer, 0, buffer, _fftLengthBytes - readSegment , readSegment);
                }

                
            }

        }

        public void ProcessAndSave(string filename, string outputDirectory = null)
        {

            var bytes = ImageProcessAndConvertToBytes();

            var detector = new SegmentDetector();

            var startIndex = detector.FindStart(bytes);
            if (startIndex != -1)
            {
                startIndex = Math.Max(0, startIndex-20);
                var endIndex = detector.FindEnd(bytes, startIndex);
                if (endIndex != -1)
                {
                    endIndex = Math.Min(bytes.Length, endIndex + 20);
                    var segments = detector.FindSeperators(bytes, startIndex, endIndex);

                    var imageCreator = new ImageCreator();

                    //main image
                    if (string.IsNullOrWhiteSpace(outputDirectory))
                    {
                        outputDirectory = Path.Combine(Path.GetDirectoryName(filename),"Processed");
                    }
                    var baseName = Path.GetFileNameWithoutExtension(filename);
                    var basePath = outputDirectory;

                    var mainImage = Path.Combine(basePath, $"{endIndex - startIndex}"+baseName + ".png");
                    imageCreator.CreateImage(_fftBuffer, startIndex, endIndex, mainImage);
                    var start = startIndex;
                    for(int i =0; i<= segments.Count; i++)
                    {
                        var end = endIndex-330;
                        if (i < segments.Count)
                        {
                            end = segments[i] + 5;
                        }
                        var segmentName = Path.Combine(basePath, baseName + $"-{i+1}.png");
                        imageCreator.CreateImage(_fftBuffer, start, end, segmentName);

                        if (i < segments.Count)
                        {
                            start = segments[i];
                        }
                    }

                    var footer = Path.Combine(basePath, baseName + $"-{segments.Count+2}.png");
                    imageCreator.CreateImage(_fftBuffer, endIndex-350, endIndex, footer);
                }
            }
        }

        public float[] ImageProcessAndConvertToBytes()
        {
            Bitmap bitmap = new Bitmap(_bitmapWidth, _bitmapHeight);

            int block = 50;
            var gain = 2500;
            //if (_process.Buffer.Count / _bitmapWidth > 1)
            //{
            //    block = _process.Buffer.Count / _bitmapWidth;
            //}
            for (int x = 0; x < _bitmapWidth && x * block + block < Buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] buffer = new float[_bitmapHeight];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = Buffer[x * block + y];

                    var frequencyBins = blockSegment.Length / _bitmapHeight;

                    for (int z = 0; z < _bitmapHeight; z++)
                    {
                        for (int w = 0; w < frequencyBins; w++)
                        {
                            buffer[z] += (blockSegment[z * frequencyBins + w] / (float)frequencyBins / (float)block);
                        }
                    }
                }
                for (int i = 0; i < _bitmapHeight; i++)
                {

                    float powerInverse = 0;
                    int red = 0;
                    int blue = 0;
                    int green = 0;
                    //for (int y = 0; y < 12; y++)

                    var value = buffer[i];

                    powerInverse += Math.Abs(value);


                    //if (powerInverse < 0.5)
                    {
                        blue = Convert.ToInt32(Math.Min(255.0f, gain*powerInverse));
                    }
                    //if (powerInverse >= 0.5f && powerInverse < 0.75f)
                    {
                        green = Convert.ToInt32(Math.Min(255.0f, gain*powerInverse));
                    }
                    //if (powerInverse > 0.75f)
                    {
                        red = Convert.ToInt32(Math.Min(255.0f, gain* powerInverse));
                    }


                    {
                        bitmap.SetPixel(x,  i, System.Drawing.Color.FromArgb(255,
                                    red,
                                    green,
                                    blue));
                    }
                }

            }
            bitmap = new ShapeDetection().GetShapeDetectionImage(bitmap);

            //bitmap.Save(@"C:\Users\Home\Documents\Audacity\codexparts\Test.bmp");

        

            return BitmapToByte(bitmap);
        }

        public float[] BitmapToByte(Bitmap bitmap)
        {
            var pixels = new float[_bitmapHeight * _bitmapWidth];

            for (int i = 0; i < _bitmapWidth; i++)
            {
                for (int y = 0; y < _bitmapHeight; y++)
                {
                    var colour = bitmap.GetPixel(i, y);
                    if (colour.R > 200)
                    {
                        pixels[i * _bitmapHeight + y] = 1;
                    }
                }
            }


            return pixels;
        }


        public void NormaliseArray(int lowerCutoff, int divider)
        {
           // NormaliseArrayRange(0, lowerCutoff, 0.009f);
            NormaliseArrayRange(lowerCutoff, divider, 0.009f);
            NormaliseArrayRange(divider, _fftLength/2, 0.009f);
            foreach (var buffer in _fftBuffer)
            {
                for (int i = 0; i < lowerCutoff; i++)
                {
                    buffer[i] = 0;
                }
            }
        }


        public void NormaliseArrayRange(int bottom, int top, float cutoff)
        {
            float min = float.MaxValue;
            float max = 0;

            //make it a logarithmic value
            foreach (var buffer in _fftBuffer)
            {
                for (int i = bottom; i < top; i++)
                {
                    var boostedValue = Math.Abs(buffer[i] * 900000.0f);
                    if (boostedValue >= 1)
                    {
                        var temp = (float)Math.Abs(Math.Log10(boostedValue));
                        if (float.IsInfinity(temp) || temp > 10)
                        {
                            buffer[i] = 0;
                        }
                        if (float.IsNaN(temp))
                        {
                            buffer[i] = 0;
                        }
                        else
                        {
                            buffer[i] = temp;
                        }
                    }
                    else 
                    {
                        buffer[i] = 0;
                    }
                }
            }

            //find the max and min
            foreach (var buffer in _fftBuffer)
            {
                for (int i = bottom; i < top; i++)
                {
                    float value = buffer[i] ;
                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max && !float.IsInfinity(value))
                    {
                        max = value;
                    }
                }
            }

            var divisor = max - min;
            //normalise that shit
            foreach (var buffer in _fftBuffer)
            {
                for (int i = bottom; i < top; i++)
                {
                    float value = buffer[i];
                    
                    value = Math.Abs((value - min)/ divisor);        
                    //if (value > cutoff)
                    //{
                    //    value *= .75f;
                    //    value = value + 0.25f;
                    //}

                    //if (value < -cutoff)
                    //{
                    //    value *= .75f;
                    //    value = value - 0.25f;
                    //}

                    buffer[i] = value;
                }
            }
        }

        public List<float[]> Buffer
        {
            get { return _fftBuffer; }
        }

    }
}
