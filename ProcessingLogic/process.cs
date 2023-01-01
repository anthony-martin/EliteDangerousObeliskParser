
using NAudio.Dsp;
using NAudio.Flac;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Signals
{
    public class ProcessImage
    {
        private List<Complex[]> _fftBuffer;
        private int _fftLength = 2048;
        private int _fftLengthBytes ;
        private int _bytesPerSameple;
        public ProcessImage(string filename)
        {
            _fftBuffer = new List<Complex[]>();

            using (var audioFile = new AudioFileReader(filename))
            {
                var data = audioFile.HasData(1024);
                var format = audioFile.WaveFormat;
                _bytesPerSameple = format.BitsPerSample / 8;
                var bytesPerSecond = format.AverageBytesPerSecond;
                _fftLength = 2048;
                //here we double the buffer for stereo as we are going to skip half the data
                _fftLengthBytes = _fftLength * _bytesPerSameple * format.Channels;
                int readSegment = Math.Min(_fftLengthBytes, bytesPerSecond * format.Channels / 50);
                

                var buffer = new byte[_fftLengthBytes];
                var read = audioFile.Read(buffer, 0, _fftLengthBytes);
                while (read >= readSegment)
                {
                    Complex[] complex = new Complex[_fftLength];
                    for (int i = 0; i < _fftLength ; i++)
                    {
                        int step = i;
                        if (format.Channels == 2)
                        {
                        //todo add config for this to select channel
                            step = i * format.Channels + 1;
                        }
                            complex[i] = new Complex
                        {
                           
                            X = Convert.ToSingle(BitConverter.ToSingle(buffer, step * _bytesPerSameple) * FastFourierTransform.HammingWindow(i, _fftLength)),
                            Y = 0

                        };
                        //  Y = Convert.ToSingle(BitConverter.ToSingle(buffer, i * bytesPerSameple +2 )) } ;
                    }


                    FastFourierTransform.FFT(true, 11, complex);
                    _fftBuffer.Add(complex);
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
            //FlacReader reader = new FlacReader(@"C:\Users\Home\Documents\Audacity\codexparts\guardian_obelisk_08.flac");
            //FlacFrame frame = FlacFrame.FromStream(reader);
            //var sampleSize = frame.Header.BitsPerSample;
            //var buffer = new float[2048];
            //reader.Read(buffer, 0, 2048);
            //Complex num = new Complex();
        }

        public void NormaliseArray(int lowerCutoff, int divider)
        {
            NormaliseArrayRange(lowerCutoff, divider, 0.009f);
            NormaliseArrayRange(divider, _fftLength/2, 0.03f);
            foreach (var buffer in _fftBuffer)
            {
                for (int i = 0; i < lowerCutoff; i++)
                {
                    buffer[i].X = 0;
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
                    float value = buffer[i].X;
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
                    float value = buffer[i].X;
                    if (value< 0)
                    {
                        value /= min;
                    }
                    else
                    {
                        value /= max;
                    }
                    if (value > cutoff)
                    {
                        value *= .75f;
                        value = value + 0.25f;
                    }

                    if (value < -cutoff)
                    {
                        value *= .75f;
                        value = value - 0.25f;
                    }

                    buffer[i].X = value;
                }
            }
        }

        public List<Complex[]> Buffer
        {
            get { return _fftBuffer; }
        }

    }
}
