using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Signals
{
    public class MessagePartModel : INotifyPropertyChanged
    {
        [JsonIgnore]
        private MessagePartModel _parent;
        [JsonIgnore]
        private MessagePartModel _child;
        [JsonIgnore]
        private string _type;
        [JsonIgnore]
        private int _end;
        [JsonIgnore]
        private TimeSpan _duration;
        private readonly ObservableCollection<int> _frequencies;
        private readonly ObservableCollection<int> _frequenciesIndex;

        private int _lineSeparation = 680;
        private int _blockSize;

        private bool _isBlock = false;

        [JsonConstructor]
        public MessagePartModel()
        {
            _frequencies = new ObservableCollection<int>();
        }

        [JsonProperty]
        public string Type
        { 
            get 
            { 
                return _type; 
                } 
            set 
            { 
                _type = value; 
                OnPropertyChanged(); 
            } 
        }
        [JsonProperty]
        public int Start 
        { 
            get 
            {
                int start = 0;
                if (_parent != null)
                {
                    return _parent.End;
                }
                return start;
            } 

        }

        [JsonProperty]
        public int End
        {
            get { return _end; }
            set
            {
                if (value > Start)
                {
                    _end = value;

                    SetDuration();
                }
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public MessagePartModel Parent
        {
            get{ return _parent; }
            set { _parent = value;
                SetDuration();
            }
        }

        [JsonIgnore]
        public MessagePartModel Child
        {
            get { return _child; }
            set
            {
                _child = value;
                SetDuration();
            }
        }

        [JsonProperty]
        public TimeSpan Duration { get { return _duration; } }

        [JsonIgnore]
        public ObservableCollection<int> Frequencies { get { return _frequencies; } }

        [JsonProperty]
        private int[] FrequenciesArray 
        { 
            get { return _frequencies.ToArray(); 
        }
            set
            {
                foreach(var freq in value)
                {
                    _frequencies.Add(freq);
                }
            }
        }

        public int LineSeparation
        {
            get{ return _lineSeparation; }
            set{
                _lineSeparation = value;
                OnPropertyChanged();
            }
        }

        public bool IsBlock
        {
            get{ return _isBlock; }
            set
            {
                _isBlock = value;
                OnPropertyChanged();
            }
        }

        private void SetDuration()
        {
            var duration = _end - Start;
            if (duration > 0)
            {
                //magic number taken from the High Frequency plot at some point consider making it less magic
                _duration = TimeSpan.FromMilliseconds(duration );
                OnPropertyChanged(nameof(Duration));
            }
            else
            {
                _duration = TimeSpan.Zero;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
