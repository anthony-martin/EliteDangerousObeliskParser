using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public MainWindow()
        {
            HighRangeBoost = 1;
            OverlayAboveSement = 660;
            StartOverlayIndex = 191;
            Gain = 255;// 7500000.0f;
            _filePath = @"C:\Users\Home\Documents\Audacity\codexparts\guardian_obelisk_08.flac";
            _process = new ProcessImage(_filePath);
            _process.NormaliseArray(StartOverlayIndex, OverlayAboveSement);
            Draw();

            DataContext = this;
            InitializeComponent();
        }

        public void Draw()
        {
            Bitmap bitmap = new Bitmap(4096, 2048);
            
            for (int x = 0; x < 4096 && x < _process.Buffer.Count; x++)
            {
                var buffer = _process.Buffer[x];
                for (int i = 0; i < 1024; i++)
                {
                    float power = 0;

                    float power2 = 0;
                    if (i >= StartOverlayIndex && OverlayAboveSement - StartOverlayIndex + i < 1024)
                    {
                        //we want to start reading up from overlay above
                        //this should be = to overlayAboveSegment when it starts and not overflow
                        power2 += HighRangeBoost * Math.Abs(buffer[ i - StartOverlayIndex + OverlayAboveSement].X);
                    }

                    {
                        power += Math.Abs(buffer[i].X);

                    }


                    bitmap.SetPixel(x, 2047 - i, System.Drawing.Color.FromArgb(255,
                                Convert.ToInt32(Math.Min(255.0f, Gain * power)),
                                Convert.ToInt32(Math.Min(255.0f, Gain * power2)),
                                0));

                    float powerInverse = 0;
                    int red = 0;
                    int blue = 0;
                    int green = 0;
                    //for (int y = 0; y < 12; y++)
                    
                    var value = buffer[i].X;
                    
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

                    bitmap.SetPixel(x, 1023-i, System.Drawing.Color.FromArgb(255,
                                red,
                                green,
                                blue));

                }
            }
            bitmap.Save(@"C:\Users\Home\Documents\Audacity\codexparts\Test.bmp");
            var hbit = bitmap.GetHbitmap();
            Bitmap = Imaging.CreateBitmapSourceFromHBitmap(hbit, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(2048, 1024));

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
                    _process.NormaliseArray(StartOverlayIndex, OverlayAboveSement);
                    Draw();
                }
                catch
                {
                }
            }
        }
    }
}
