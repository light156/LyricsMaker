using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace LyricsMaker
{
    public class BindLyrics :INotifyPropertyChanged
    {
        private ObservableCollection<string> lyrics = new ObservableCollection<string>();
        private ObservableCollection<string> originallyrics = new ObservableCollection<string>();

        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Lyrics
        {
            get { return lyrics; }
            set
            {
                if (lyrics == value)
                    return;

                lyrics = value;
                NotifyPropertyChanged("Lyrics");
            }
        }

        public ObservableCollection<string> OriginalLyrics
        {
            get { return originallyrics; }
            set { originallyrics = value; }
        }

    }
}
