
using NAudio.Dsp;
using NAudio.Flac;
using NAudio.Wave;
using ProcessingLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Signals
{
    public class ProcessImage
    {
        private List<float[]> _fftBuffer;
        private int _fftLength = 4096;
        private int _fftLengthBytes ;
        private int _bytesPerSameple;

        private int _bitmapWidth = 4096;
        private int _bitmapHeight = 1024;


        public ProcessImage(string filename)
        {
            _fftBuffer = new List<float[]>();

            using (var audioFile = new AudioFileReader(filename))
            {
                var data = audioFile.HasData(_fftLength);
                var format = audioFile.WaveFormat;
                _bytesPerSameple = format.BitsPerSample / 8;
                var bytesPerSecond = format.AverageBytesPerSecond;
                
                //here we double the buffer for stereo as we are going to skip half the data
                _fftLengthBytes = _fftLength * _bytesPerSameple * format.Channels;
                int readSegment = Math.Min(_fftLengthBytes, bytesPerSecond * format.Channels / 1000);
                

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
                            
                            reading = BitConverter.ToSingle(buffer, (i * 2 )* _bytesPerSameple);
                            reading += BitConverter.ToSingle(buffer, (i * 2 +1) * _bytesPerSameple);
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


                    FastFourierTransform.FFT(true, 12, complex);
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
                /*
                 * Bin size = sample rate / fft size. 
                 * 
                 */
                //using (var outputDevice = new WaveOutEvent())
                //{
                //    outputDevice.Init(audioFile);
                //    outputDevice.Play();
                //    while (outputDevice.PlaybackState == PlaybackState.Playing)
                //    {
                //        Thread.Sleep(1000);
                //    }
                //}
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

            bitmap.Save(@"C:\Users\Home\Documents\Audacity\codexparts\Test.bmp");

            var pixels = new float[_bitmapHeight * _bitmapWidth];

            for (int i = 0; i < _bitmapWidth; i++)
            {
                for (int y = 0; y < _bitmapHeight; y++)
                {
                    var colour = bitmap.GetPixel(i, y);
                    if (colour.R >200)
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
            NormaliseArrayRange(divider, _fftLength/2, 0.03f);
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
            float min = 0;
            float max = 0;
            foreach (var buffer in _fftBuffer)
            {
                for (int i = bottom; i < top; i++)
                {
                    float value = buffer[i];
                    if (value < min)
                    {
                        min = value;
                    }
                    else if (value > max)
                    {
                        max = value;
                    }
                }
            }

           // min *= -1;

            foreach (var buffer in _fftBuffer)
            {
                

                for (int i = bottom; i < top; i++)
                {
                    float value = buffer[i];
                    if (value< 0)
                    {
                        value =  (value/ min)  ;
                    }
                    else
                    {
                        value =  (value / max );
                    }

                   
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
