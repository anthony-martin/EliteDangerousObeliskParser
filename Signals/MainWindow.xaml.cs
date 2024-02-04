using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
        private int _bitmapWidth = 2048;
        private int _bitmapHeight = 512;
        private int _imageSegment = 0;
        private int _block = 50;

        private ObservableCollection<MessagePartModel> _parts;
        private MessagePartModel _selectedPart;

        public MainWindow()
        {
            _parts = new ObservableCollection<MessagePartModel> { new MessagePartModel { Type="1111"}, new MessagePartModel { Type = "22222" } };
            _selectedPart = _parts.FirstOrDefault();
            HighRangeBoost = 1;
            OverlayAboveSement = 1400;
            StartOverlayIndex = 360;
            Gain = 180;// 7500000.0f;


            DataContext = this;
            InitializeComponent();
        }

        public void Draw()
        {
            Bitmap bitmap = new Bitmap(_bitmapWidth, _bitmapHeight);

            int block = _block;
            int frequencyBins = 0;
            for (int x = 0; x < _bitmapWidth &&  x * block + block < _process.Buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] buffer = new float[_bitmapHeight];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = _process.Buffer[ x * block + y];

                    frequencyBins = blockSegment.Length / _bitmapHeight;

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



                    {
                        blue = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                        green = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                        red = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                    }
                    if( ( i == (800+ _bitmapWidth) / frequencyBins) 
                    &&  x >= (_imageSegment / block ) &&  x <= (_imageSegment / block ) + _bitmapHeight / 2)
                    {
                        blue = 0;
                        red = 255;
                        green = 0;
                    }

                    if ((i == 800 / frequencyBins )
                   && x >= (_imageSegment / block) && x <= (_imageSegment / block) + _bitmapHeight / 2)
                    {
                        blue = 255;
                        red = 0;
                        green = 0;
                    }
                    if (i >= 800 / frequencyBins && i <= ((800 + _bitmapWidth) / frequencyBins) 
                    &&  x == (_imageSegment / block))
                    {
                        blue = 0;
                        red = 255;
                        green = 0;
                    }

                    if (i >= 800 / frequencyBins && i <= ((800 + _bitmapWidth) / frequencyBins)
                   && ( x == (_imageSegment / block) + _bitmapHeight / 2))
                    {
                        blue = 0;
                        red = 0;
                        green = 255;
                    }

                    if (i > OverlayAboveSement)
                    {
                        powerInverse *= HighRangeBoost;
                    }
                   
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

        public void DrawZoomed()
        {
            Bitmap bitmap = new Bitmap(_bitmapWidth, _bitmapHeight);

            int block = _block /2;

            for (int x = 0; x < _bitmapHeight && _imageSegment + x * block + block < _process.Buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] buffer = new float[_bitmapWidth];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = _process.Buffer[_imageSegment + x * block + y];

                    var frequencyBins = 1;

                    for (int z = 0; z < _bitmapWidth; z++)
                    {
                        for (int w = 0; w < frequencyBins; w++)
                        {
                            buffer[z] += (blockSegment[800 + z * frequencyBins + w] / (float)block);
                        }
                    }
                }
                for (int i = 0; i < _bitmapWidth; i++)
                {
                   
                    float powerInverse = 0;
                    int red = 0;
                    int blue = 0;
                    int green = 0;
                    //for (int y = 0; y < 12; y++)

                    var value = buffer[i];

                    powerInverse += Math.Abs(value);


                    {
                        blue = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                        green = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                        red = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                    }

                    if (i > OverlayAboveSement)
                    {
                        powerInverse *= HighRangeBoost;
                    }
                   
                    {
                        bitmap.SetPixel(_bitmapWidth - 1 - i, _bitmapHeight - 1 - x, System.Drawing.Color.FromArgb(255,
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

            var pixels = new byte[_bitmapHeight * _bitmapWidth];
            var Iptr = bits.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(Iptr, pixels, 0, pixels.Length);

            BitmapDetailed = Imaging.CreateBitmapSourceFromHBitmap(hbit, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(_bitmapWidth, _bitmapHeight));

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

        public BitmapSource BitmapDetailed
        {
            get;
            set;
        }

        private void OnFrequency(object sender, MouseButtonEventArgs e)
        {
            var val =  e.GetPosition(sender as IInputElement);
            if(_selectedPart != null)
            {
                //magic number is 24000/4096 which is our frequency accuracy and we should link this to values in the processor
                _selectedPart.Frequencies.Add((int) ((800.0 + 2048 - val.X) * 5.859375));
            }

        }

        private void OnTime(object sender, MouseButtonEventArgs e)
        {
            var val = e.GetPosition(sender as IInputElement);

            int timestamp = _imageSegment + (512 - (int)val.Y) * _block / 2;
            if (_selectedPart != null)
            {
                if(_selectedPart.Start == 0 )
                {
                    _selectedPart.Start = timestamp;
                }
                else if(_selectedPart.End != 0)
                {
                    _selectedPart.Start = timestamp;
                    _selectedPart.End = 0;
                }
                else
                {
                    _selectedPart.End = timestamp;
                }
            }
        }

        private void PreviousSegment(object sender, RoutedEventArgs e)
        {
            if (_imageSegment > 0)
            {
                _imageSegment -= _bitmapHeight * _block /4;
                Draw();
                DrawZoomed();
            }
            
        }

        private void NextSegment(object sender, RoutedEventArgs e)
        {
            if (_imageSegment + _bitmapHeight * (_block /4 )  <= _process.Buffer.Count)
            {
                _imageSegment += _bitmapHeight * _block /4;
                Draw();
                DrawZoomed();
            }
           
        }

        public ObservableCollection<MessagePartModel> Parts
        {
            get{ return _parts; }
            set{
            
            }
        }

        public MessagePartModel SelectedPart
        {
            get { return _selectedPart; }
            set 
            { 
                _selectedPart = value;
                OnPropertyChanged(null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Draw();
            DrawZoomed();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _imageSegment = 0;
            var dialog = new Microsoft.Win32.OpenFileDialog();

            if (true == dialog.ShowDialog())
            {
                var fileName = dialog.FileName;
                await ProcessFile(fileName);
            }
        }

        private async void ProcessFolder(object sender, RoutedEventArgs e)
        {
            var inputDirectory = "";
            var outputDirectory = "";

            var folderBrowser =  new FolderBrowserDialog();
            folderBrowser.Description = "Select Input directory";
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputDirectory = folderBrowser.SelectedPath;
                folderBrowser.Description = "Select output directory";
                if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    outputDirectory = folderBrowser.SelectedPath;

                    DirectoryInfo dinfo = new DirectoryInfo(inputDirectory);

                    var flakFiles = dinfo.GetFiles("*.flac");
                    foreach (var file in flakFiles)
                    {
                        await ProcessFile(file.FullName, outputDirectory);
                    }
                }
            }

        }

        public async Task<bool> ProcessFile(string fileName, string outputDir = null)
        {
            bool success = false;
            try
            {
                _process = new ProcessImage(fileName);
                _process.NormaliseArray(StartOverlayIndex, OverlayAboveSement);
                //_process.ProcessAndSave(fileName, outputDir);
                Draw();
                DrawZoomed();
                success = true;
            }
            catch
            {
            }

            return success;
        }
    }
}
