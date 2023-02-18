using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ProcessingLogic
{
    public class ImageCreator
    {
        private int _padding = 10;
        private int _bitmapHeight = 1024;
        private int _gain = 2500;

        public void CreateImage(List<float[]> buffer, int start, int stop, string name)
        {
            var width = stop - start + (_padding*2);
            if (width <= 0)
            {
                return;
            }

            Bitmap bitmap = new Bitmap(width, _bitmapHeight);

            int block = 50;

            for (int x = 0; x< _padding; x++)
            {
                for (int i = 0; i < _bitmapHeight; i++)
                {
                    bitmap.SetPixel(x, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                                        0,
                                        0,
                                        0));

                    bitmap.SetPixel(width - x - 1, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                    0,
                    0,
                    0));
                }
            }


            for (int x = 0 ; x < (stop-start) && (x * block + block)< buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] imageBuffer = new float[_bitmapHeight];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = buffer[(x + start) * block + y];

                    var frequencyBins = blockSegment.Length / _bitmapHeight;

                    for (int z = 0; z < _bitmapHeight; z++)
                    {
                        for (int w = 0; w < frequencyBins; w++)
                        {
                            imageBuffer[z] += (blockSegment[z * frequencyBins + w] / (float)frequencyBins / (float)block);
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

                    var value = imageBuffer[i];

                    powerInverse += Math.Abs(value);


                    blue = Convert.ToInt32(Math.Min(255.0f, _gain * powerInverse));
                    green = Convert.ToInt32(Math.Min(255.0f, _gain * powerInverse));
                    red = Convert.ToInt32(Math.Min(255.0f, _gain * powerInverse));

                    bitmap.SetPixel(x+_padding, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                                    red,
                                    green,
                                    blue));
                    
                }

            }

            bitmap.Save(name, ImageFormat.Png);
        }

    }
}
