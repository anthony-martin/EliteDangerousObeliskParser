using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingLogic
{
    public class SegmentDetector
    {
        int _hz = 1000;
        int _sampleInterval = 25;
        int _from = 720;
        int _to = 1024;
        int _frame = 1024;

        public SegmentDetector()
        {
        }

        public int FindStart(float[] fftBuffer)
        {
          

            //when the value spikes here we check duration. If it's more than 1 second 
            int index = -1;
            int count = 0;
            int start = 0;
            do
            {
                float value = 0;
                for (int i = start + _from; i < start + _to; i++)
                {
                    value += fftBuffer[i];
                }

                if (value >6 )
                {
                    if (count == 0)
                    {
                        index = start / _frame;
                    }
                    count++;
                }
                else
                {
                    count = 0;
                    index = -1;
                }
                if (count >= 5)
                {
                    start = fftBuffer.Length;
                }
                start += _frame;
            }
            while (start < fftBuffer.Length);


            return index;
        }

        public int FindEnd(List<float[]> fftBuffer, int start)
        {
            //set a baseline here
            float baseline = 0;
            for (int i = 0; i < _hz / _sampleInterval; i += _sampleInterval)
            {
                var fft = fftBuffer[i];

                for (int x = 720; x < 1024; x++)
                {
                    baseline += fft[x];
                }
            }
            baseline = baseline / (_hz / _sampleInterval);

            //when the value spikes here we check duration. If it's more than 1 second 
            int index = -1;
            int count = 0;
            int end = start + _hz + _sampleInterval;
            do
            {
                float value = 0;
                for (int y = 0; y < 5; y++)
                {
                    var fft = fftBuffer[end + y];
                    for (int x = 720; x < 1024; x++)
                    {
                        value += fft[x];
                    }
                }
                value /= 5;
                if (end > 73000)
                {
                    if (Math.Abs(value - baseline) < 0.15)
                    {
                        if (count == 0)
                        { index = end; }
                        count++;
                    }
                }
                else if (Math.Abs(value - baseline) < 0.15)
                {
                    if (count == 0)
                    { index = end; }
                    count++;
                }
                else
                {
                    count = 0;
                    index = -1;
                }
                if (count >= 10)
                {

                    end = fftBuffer.Count;
                }
                end += _sampleInterval;
            }
            while (end < fftBuffer.Count - 5);


            return index;
        }
    }
}
