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
                for (int i = start + 735; i < start + 980; i++)
                {
                    value += fftBuffer[i];
                }

                if (value > 100)
                {
                    index = start / _frame;
                    start = fftBuffer.Length;
                }
                else if (value > 6)
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
                if (count >= 15)
                {
                    start = fftBuffer.Length;
                }
                start += _frame;
            }
            while (start < fftBuffer.Length);


            return index;
        }

        public int FindEnd(float[] fftBuffer, int start)
        {
            int index = -1;
            int count = 0;
            int dropped = 0;
            start *= _frame;
            do
            {
                float value = 0;
                for (int i = start + 763; i < start + 980; i++)
                {
                    value += fftBuffer[i];
                }

                if (value > 160)
                {
                    count++;
                    dropped = 0;
                }
                else
                {
                    dropped++;
                    if (dropped > 3)
                    {
                        count = 0;
                    }
                }
                if (count >= 72)
                {
                    index = start / _frame;
                    start = fftBuffer.Length;
                }
                start += _frame;
            }
            while (start < fftBuffer.Length - _frame);
            return index;
        }

        public List<int> FindSeperators(float[] fftBuffer, int start, int end)
        {
            var results = new List<int>();
            int endIndex = end * _frame;
            int index = 0;
            int count = 0;
            int dropped = 0;
            start *= _frame;
            
            do
            {
                float value = 0;
                for (int i = start + 200; i < start + 545; i++)
                {
                    value += fftBuffer[i];
                }

                if (value > 240)
                {
                    if (count == 0)
                    {
                        index = start / _frame;
                    }
                    count++;
                    dropped = 0;
                }
                else
                {
                    
                    if (count == 5 && dropped > 4)
                    {
                        results.Add(index);
                    }
                    if (dropped > 4)
                    {
                        count = 0;
                    }
                    else if(count > 0 && count < 5)
                    {
                        count++;
                    }
                    dropped++;
                }
                
                start += _frame;
            }
            while (start < endIndex - _frame);
            return results;
        }
    }
}
