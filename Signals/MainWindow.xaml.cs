﻿using Newtonsoft.Json;
using ProcessingLogic;
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
        private int _imageSegmentTop = 0;
        private int _imageSegmentBottom = 0;
        private int _block = 4;
        private int _hdFreqOffset = 4000;
        private bool _blockMode = false;
        private bool _lineMode = false;


        private ObservableCollection<MessagePartModel> _parts;
        private MessagePartModel _selectedPart;

        public MainWindow()
        {
            _parts = new ObservableCollection<MessagePartModel> { new MessagePartModel { Type="Start"}};
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
            for (int x = 0; x < _bitmapWidth && _imageSegmentTop + x * block + block < _process.Buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] buffer = new float[_bitmapHeight];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = _process.Buffer[_imageSegmentTop +  x * block + y];

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
                    
                    float powerInverse = 0;
                    int red = 0;
                    int blue = 0;
                    int green = 0;
                    //for (int y = 0; y < 12; y++)

                    var value = buffer[i];

                    powerInverse += Math.Abs(value);

                    var binsBottom = 1;
                    //set the image colour here
                    {
                        blue = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                        green = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                        red = Convert.ToInt32(Math.Min(255.0f, Gain * powerInverse));
                    }
                    //override with the HD bounds after
                    if ( ( i == (_hdFreqOffset + _bitmapWidth) / frequencyBins / binsBottom )
                    &&  x >= ((_imageSegmentBottom - _imageSegmentTop) / block ) &&  x <= ((_imageSegmentBottom - _imageSegmentTop) / block ) + _bitmapHeight / 2)
                    {
                        blue = 0;
                        red = 255;
                        green = 0;
                    }

                    if ((i == _hdFreqOffset / frequencyBins / binsBottom)
                   && x >= ((_imageSegmentBottom - _imageSegmentTop) / block) && x <= ((_imageSegmentBottom - _imageSegmentTop) / block) + _bitmapHeight / 2)
                    {
                        blue = 255;
                        red = 0;
                        green = 0;
                    }
                    if (i >= _hdFreqOffset / frequencyBins / binsBottom && i <= ((_hdFreqOffset + _bitmapWidth) / frequencyBins  / binsBottom) 
                    &&  x == ((_imageSegmentBottom - _imageSegmentTop )/ block))
                    {
                        blue = 0;
                        red = 255;
                        green = 0;
                    }

                    if (i >= _hdFreqOffset / frequencyBins / binsBottom && i <= ((_hdFreqOffset + _bitmapWidth) / frequencyBins  / binsBottom)
                   && ( x == ((_imageSegmentBottom - _imageSegmentTop) / block) + _bitmapHeight / 2))
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
            
            foreach(var part in _parts)
            {
                if (part.End > 0 && part.End < _imageSegmentTop + _bitmapWidth * block) 
                {
                    var x = part.End  - _imageSegmentTop;
                    x /= block;
                    for (int i = 0; i < _bitmapHeight; i++)
                    {
                        if (x >= 0 && x < _bitmapWidth)
                        {
                            bitmap.SetPixel(x, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                                            255,
                                            0,
                                            0));
                        }
                    }
                }
            }


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

            int block = _block / 2;
            var frequencyBins = 1;

            for (int x = 0; x < _bitmapHeight && _imageSegmentBottom + x * block + block < _process.Buffer.Count; x++)
            {
                // here we add all the blocks together
                float[] buffer = new float[_bitmapWidth];
                for (int y = 0; y < block; y++)
                {
                    var blockSegment = _process.Buffer[_imageSegmentBottom + x * block + y];


                    for (int z = 0; z < _bitmapWidth; z++)
                    {
                        for (int w = 0; w < frequencyBins; w++)
                        {
                            buffer[z] += blockSegment[_hdFreqOffset + z * frequencyBins + w] / (float)block;
                        }
                    }
                }
                for (int i = 0; i < _bitmapWidth; i++)
                {

                    float powerInverse = 0;
                    int red = 0;
                    int blue = 0;
                    int green = 0;

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

            foreach (var part in _parts)
            {
                if (part.End > _imageSegmentBottom && part.End < _imageSegmentBottom + _bitmapHeight * block)
                {
                    var x = part.End - _imageSegmentBottom;
                    x /= block;
                    for (int i = 0; i < _bitmapWidth; i++)
                    {
                        bitmap.SetPixel(_bitmapWidth - 1 - i, _bitmapHeight - 1 - x, System.Drawing.Color.FromArgb(255,
                                        255,
                                        0,
                                        0));
                    }
                }
            }


            foreach (var part in _parts)
            {
                if (part != null && !part.IsBlock)
                {
                    var freq = part.Frequencies.FirstOrDefault();
                    // int timestamp = _imageSegmentBottom + (512 - (int)val.Y) * _block / 2;
                    var start = (part.Start - _imageSegmentBottom) / _block * 2;
                    var end = Math.Min(512, (part.End - _imageSegmentBottom) / _block * 2);
                    if (end > start && start > 0)
                    {
                        for (int band = 0; band < 10; band++)
                        {
                            var pixel = (freq / 1.46484375 - _hdFreqOffset) / frequencyBins;

                            for (int i = start; i < end; i++)
                            {
                                if (pixel >= 0 && pixel < _bitmapWidth)
                                {
                                    bitmap.SetPixel(_bitmapWidth - (int)pixel, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                                                    255,
                                                    0,
                                                    0));
                                }
                            }

                            freq += part.LineSeparation;
                        }
                    }

                }

                else if (part != null && part.IsBlock && part.Frequencies.Count > 1)
                {
                    // int timestamp = _imageSegmentBottom + (512 - (int)val.Y) * _block / 2;
                    var start = (part.Start - _imageSegmentBottom) / _block * 2;
                    var end = Math.Min(512, (part.End - _imageSegmentBottom) / _block * 2);
                    if (end > start && start >= 0)
                    {
                        for (int band = 0; band < 2; band++)
                        {
                            var freq = part.Frequencies[band];

                            var pixel = (freq / 1.46484375 - _hdFreqOffset) / frequencyBins;

                            for (int i = start; i < end; i++)
                            {
                                if (pixel >= 0 && pixel < _bitmapWidth)
                                {
                                    bitmap.SetPixel(_bitmapWidth - (int)pixel, _bitmapHeight - 1 - i, System.Drawing.Color.FromArgb(255,
                                                    255,
                                                    0,
                                                    0));
                                }
                            }

                        }
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
               
                var frequency =(int) ((_hdFreqOffset + (_bitmapWidth - val.X) ) * 1.46484375);
                _selectedPart.Frequencies.Add(frequency);
                if (_selectedPart.IsBlock && _selectedPart.Frequencies.Count == 2)
                {
                    _selectedPart.LineSeparation = _selectedPart.Frequencies[1] - _selectedPart.Frequencies[0];
                }
                //FindFrequencies finder = new FindFrequencies();
                //var res = finder.FindBlock(_process.Buffer, _selectedPart.Start, _selectedPart.End, 4000, 9500);
                //magic number is 24000/4096 which is our frequency accuracy and we should link this to values in the processor

                //foreach (var freq in res)
                //{
                //    float index = freq.Item1;
                //    index *= 1.46484375f;
                //    _selectedPart.Frequencies.Add((int)index);
                //}
                DrawZoomed();
            }

        }

        private void OnTime(object sender, MouseButtonEventArgs e)
        {
            var val = e.GetPosition(sender as IInputElement);

            int timestamp = _imageSegmentBottom + (512 - (int)val.Y) * _block / 2;
            if (_selectedPart != null)
            {

                 _selectedPart.End = timestamp;
                Draw();
                DrawZoomed();
            }
        }

        private void PreviousSegmentTop(object sender, RoutedEventArgs e)
        {
            if (_imageSegmentTop > 0)
            {
                _imageSegmentTop -= _bitmapWidth * _block / 2;
                Draw();
                DrawZoomed();
            }

        }

        private void NextSegmentTop(object sender, RoutedEventArgs e)
        {
            if (_imageSegmentTop + _bitmapWidth * (_block / 2) <= _process.Buffer.Count)
            {
                _imageSegmentTop += _bitmapWidth * _block / 2;
                Draw();
                DrawZoomed();
            }

        }

        private void PreviousSegmentBottom(object sender, RoutedEventArgs e)
        {
            if (_imageSegmentBottom > 0)
            {
                _imageSegmentBottom -= _bitmapHeight * _block /4;
                Draw();
                DrawZoomed();
            }
            
        }

        private void NextSegmentBottom(object sender, RoutedEventArgs e)
        {
            if (_imageSegmentBottom + _bitmapHeight * (_block /4 )  <= _process.Buffer.Count)
            {
                _imageSegmentBottom += _bitmapHeight * _block /4;
                Draw();
                DrawZoomed();
            }
           
        }

        private void IncreaseHdFreq(object sender, RoutedEventArgs e)
        {
            if (_hdFreqOffset + _bitmapWidth / 2 < 16000- _bitmapWidth )
            {
                _hdFreqOffset += _bitmapWidth / 2;
                Draw();
                DrawZoomed();
            }

        }

        private void DecreaseHdFreq(object sender, RoutedEventArgs e)
        {
            if (_hdFreqOffset - _bitmapWidth / 2 > 0)
            {
                _hdFreqOffset -= _bitmapWidth / 2;
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

        private void OnAddToEnd(object sender, RoutedEventArgs e)
        {
            var parent = _parts.Last();
            if (parent.End != 0)
            {
                var newPart = new MessagePartModel { Type = "Unknown", Parent = parent };
                parent.Child = newPart;
                _parts.Add(newPart);
                SelectedPart = newPart;
            }
        }

        private void OnDeletePart(object sender, RoutedEventArgs e)
        {
            var parent = _selectedPart.Parent;
            var child = _selectedPart.Child;
            if (parent != null)
            {
                parent.Child = child;
                _selectedPart.Parent = null;
                _selectedPart.Child = null;
                _parts.Remove(_selectedPart);
            }
            if(child != null)
            {
                child.Parent = parent;
            }

        }

        private void ClearFrequencies(object sender, RoutedEventArgs e)
        {
            if (_selectedPart != null)
            {
                _selectedPart.Frequencies.Clear();
            }
        }

        private void ToggleBlockMode(object sender, RoutedEventArgs e)
        {
            _blockMode = !_blockMode;
            _lineMode = false;
        }

        private void ToggleLineMode(object sender, RoutedEventArgs e)
        {
            _lineMode = !_lineMode;
            _blockMode = false;
        }

        private void SaveOverview(object sender, RoutedEventArgs e)
        {
            var json = JsonConvert.SerializeObject(_parts.ToArray());

            File.WriteAllText(_filePath + ".json", json);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _imageSegmentBottom = 0;
            var dialog = new Microsoft.Win32.OpenFileDialog();

            if (true == dialog.ShowDialog())
            {
                _filePath = dialog.FileName;
                
                if (File.Exists(_filePath + ".json"))
                {
                    _parts.Clear();
                    var parts = JsonConvert.DeserializeObject<MessagePartModel[]>(File.ReadAllText(_filePath + ".json"));
                    MessagePartModel previous = null;
                    foreach(var part in parts)
                    {
                        if(previous != null)
                        {
                            previous.Child = part;
                            part.Parent = previous;
                        }
                        previous = part;
                        _parts.Add(part);
                        
                    }
                }
                else
                {
                    _parts.Clear();
                    _parts.Add(new MessagePartModel { Type = "Start" });
                }
                await ProcessFile(_filePath);

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
