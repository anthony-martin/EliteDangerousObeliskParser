using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Signals
{
    public class MessagePartModel : INotifyPropertyChanged
    {
        private string _type;
        private int _start;
        private int _end;
        private TimeSpan _duration;
        private readonly ObservableCollection<int> _frequencies;

        public MessagePartModel()
        {
            _frequencies = new ObservableCollection<int>();
        }

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
        public int Start 
        { 
            get { return _start; } 
            set 
            {
                _start = value;
                
                 SetDuration();
                OnPropertyChanged();
            } 
        }

        public int End
        {
            get { return _end; }
            set
            {
                _end = value;
                
                SetDuration();
                OnPropertyChanged();
            }
        }

        public TimeSpan Duration { get { return _duration; } }

        public ObservableCollection<int> Frequencies { get { return _frequencies; } }

        private void SetDuration()
        {
            var duration = _end - _start;
            if (duration > 0)
            {
                //magic number taken from the High Frequency plot at some point consider making it less magic
                _duration = TimeSpan.FromMilliseconds(duration );
                OnPropertyChanged(nameof(Duration));
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
