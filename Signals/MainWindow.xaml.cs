using Microsoft.Win32;
using ProcessingLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Signals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        private string _filePath;
        private ProcessImage _process;

        public event PropertyChangedEventHandler PropertyChanged;
        private int _bitmapWidth = 4096;
        private int _bitmapHeight = 1024;
        public MainWindow()
        {
            HighRangeBoost = 1;
            OverlayAboveSement = 720;
            StartOverlayIndex = 191;
            Gain = 2500;// 7500000.0f;
            //_filePath = @"C:\Users\Home\Documents\Audacity\codexparts\guardian_obelisk_08.flac";
            //_process = new ProcessImage(_filePath);
            //_process.NormaliseArray(StartOverlayIndex*2, OverlayAboveSement * 2);
            //_process.ProcessAndSave(_filePath);
            //Draw();

            DataContext = this;
            InitializeComponent();
        }

        public void Draw()
        {
            Bitmap bitmap = new Bitmap(_bitmapWidth, _bitmapHeight);

            int block = 50;
            //if (_process.Buffer.Count / _bitmapWidth > 1)
            //{
            //    block = _process.Buffer.Count / _bitmapWidth;
            //}
            for (int x = 0; x < _bitmapWidth && x* block + block < _process.Buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] buffer = new float[_bitmapHeight];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = _process.Buffer[x*block+y];

                    var frequencyBins = blockSegment.Length / _bitmapHeight;
                  
                    for (int z = 0; z < _bitmapHeight; z++)
                    {
                        for (int w = 0; w < frequencyBins; w++)
                        {
                            buffer[z] += (blockSegment[z* frequencyBins + w] / (float)frequencyBins / (float)block);
                        }
                    }
                }
                for (int i = 0; i < _bitmapHeight; i++)
                {
                    //float power = 0;

                    //float power2 = 0;
                    //if (i >= StartOverlayIndex && OverlayAboveSement - StartOverlayIndex + i < 1024)
                    //{
                    //    //we want to start reading up from overlay above
                    //    //this should be = to overlayAboveSegment when it starts and not overflow
                    //    power2 += HighRangeBoost * Math.Abs(buffer[ i - StartOverlayIndex + OverlayAboveSement]);
                    //}

                    //{
                    //    power += Math.Abs(buffer[i]);

                    //}


                    //bitmap.SetPixel(x, 2047 - i, System.Drawing.Color.FromArgb(255,
                    //            Convert.ToInt32(Math.Min(255.0f, Gain * power)),
                    //            Convert.ToInt32(Math.Min(255.0f, Gain * power2)),
                    //            0));

                    float powerInverse = 0;
                    int red = 0;
                    int blue = 0;
                    int green = 0;
                    //for (int y = 0; y < 12; y++)

                    var value = buffer[i];

                    powerInverse += Math.Abs(value);


                    //if (powerInverse < 0.5)
                    {
                        blue = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                    }
                    //if (powerInverse >= 0.5f && powerInverse < 0.75f)
                    {
                        green = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                    }
                    //if (powerInverse > 0.75f)
                    {
                        red = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                    }
                    if (i > OverlayAboveSement)
                    {
                        powerInverse *= HighRangeBoost;
                    }
                    //if (i >= OverlayAboveSement - 1 && i <= OverlayAboveSement + 1)
                    //{
                    //    bitmap.SetPixel(x, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                    //                   255,
                    //                   0,
                    //                   0));
                    //}
                    //else if (x == 35)
                    //{
                    //    bitmap.SetPixel(x, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                    //                       255,
                    //                       0,
                    //                       0));
                    //}
                    //else
                    {
                        bitmap.SetPixel(x, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                                    red,
                                    green,
                                    blue));
                    }
                }
                
            }
            //bitmap = new ShapeDetection().GetShapeDetectionImage(bitmap);

            //bitmap.Save(@"C:\Users\Home\Documents\Audacity\codexparts\Test.bmp");


            var bits = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, _bitmapWidth, _bitmapHeight), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var depth = System.Drawing.Bitmap.GetPixelFormatSize(bitmap.PixelFormat);

            var hbit = bitmap.GetHbitmap();

            var pixels = new byte[_bitmapHeight*_bitmapWidth];
            var Iptr = bits.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(Iptr, pixels, 0, pixels.Length);

            Bitmap = Imaging.CreateBitmapSourceFromHBitmap(hbit, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(_bitmapWidth, _bitmapHeight));

            OnPropertyChanged(null);
        }

        public float HighRangeBoost
        { get; set; }


        public float Gain
        { get; set; }


        public int OverlayAboveSement
        { get; set; }

        public int StartOverlayIndex
        { get; set; }

        public string Min
        { get; set; }

        public string Max
        { get; set; }


        public BitmapSource Bitmap
        {
            get;
            set;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (true == dialog.ShowDialog())
            {
                var fileName = dialog.FileName;
                try
                {
                    _process = new ProcessImage(fileName);
                    _process.NormaliseArray(StartOverlayIndex * 2, OverlayAboveSement * 2);
                    _process.ProcessAndSave(fileName);
                    Draw();
                }
                catch
                {
                }
            }
        }
    }
}
