using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace LyricsMaker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private BindLyrics source = new BindLyrics();
        private string songname;
        private DispatcherTimer readDataTimer = new DispatcherTimer();
        private bool drag = true;
        public static TimeSpan duration = TimeSpan.Zero;

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            LyricsList.DataContext = source;
            readDataTimer.Tick += new EventHandler(timeCycle);
            readDataTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            readDataTimer.Stop(); 
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "选择音乐",
                Filter = "合适的音乐文件(*.mp3,*.wav)|*.mp3;*.wav",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == false)
                return;
            songname = openFileDialog.SafeFileName;

            player.MediaOpened += new EventHandler(opensong);
            player.Open(new Uri(openFileDialog.FileName, UriKind.Relative));
            
            Start.IsEnabled = true;
            Stop.IsEnabled = true;
            Forward.IsEnabled = true;
            Backward.IsEnabled = true;
            slider.IsEnabled = true;
        }

        public void timeCycle(object sender, EventArgs e)
        {
            if(drag)
                slider.Value = 10 * player.Position.TotalSeconds / duration.TotalSeconds;
        }

        private void opensong(object sender, EventArgs e)
        {
            duration = player.NaturalDuration.TimeSpan;
            string str = duration.Minutes + ":" + duration.Seconds / 10 + duration.Seconds % 10;
            str = str.PadLeft(5, '0');
            time.Content = "时长：" + str;
            totaltime.Content = str;

            name.Text = songname.Remove(songname.Length - 4);
            songname = songname.Remove(songname.Length - 4) + ".lrc";
            readDataTimer.Start();    
        } 

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "选择歌词",
                Filter = "文本文件(*.txt)|*.txt",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == false)
                return;
            string lyricsname = openFileDialog.FileName;
            using (StreamReader sr = new StreamReader(lyricsname, Encoding.Default))
            {
                source.Lyrics.Clear();
                while (sr.Peek() > 0)
                {
                    string temp = sr.ReadLine();
                    if (temp.Replace(" ", "") == "")
                        continue;
                    source.Lyrics.Add(temp);
                }
            }
            source.OriginalLyrics = new ObservableCollection<string>(source.Lyrics);
            LyricsList.Focus();
            LyricsList.SelectedIndex = 0;
       
            Refresh.IsEnabled = true;
            Save.IsEnabled = true;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Start.Content.ToString() == "开始播放")
            {
                Start.Content = "暂停播放";
                player.Play();
                LyricsList.Focus();
            }
            else
            {
                Start.Content = "开始播放";
                player.Pause();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            Start.Content = "开始播放";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            source.Lyrics = new ObservableCollection<string>(source.OriginalLyrics);
            LyricsList.Focus();
            LyricsList.SelectedIndex = 0;
        }

        private void LyricsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F9 && Refresh.IsEnabled == true)
            {
                int i = LyricsList.SelectedIndex;
                TimeSpan temp = player.Position;
                string time = "[";
                time = time + temp.Minutes / 10 + temp.Minutes % 10 + ":";
                time = time + temp.Seconds / 10 + temp.Seconds % 10 + ".";
                time = time + temp.Milliseconds / 100 + (temp.Milliseconds / 10) % 10 + ']';
                source.Lyrics[i] = time + source.Lyrics[i];
                if (i < source.Lyrics.Count-1)
                    LyricsList.SelectedIndex = i + 1;
                else
                    LyricsList.SelectedIndex = i;
            }
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            goforward();
        }

        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            gobackward();
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                if (Start.IsEnabled == true)
                    goforward();
            }
            else if (e.Key == Key.F7)
            {
                if (Start.IsEnabled == true)
                    gobackward();
            }
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.V))
            {
                string temptext = System.Windows.Clipboard.GetText();
                if (temptext == string.Empty)
                    MessageBox.Show("请确认你复制了歌词");
                else
                {
                    source.Lyrics.Clear();
                    string[] arr = Regex.Split(temptext, "\r\n");
                    foreach (string temp in arr)
                    {
                        if (temp.Replace(" ", "") == "")
                            continue;
                        source.Lyrics.Add(temp);
                    }
                    source.OriginalLyrics = new ObservableCollection<string>(source.Lyrics);
                    LyricsList.Focus();
                    LyricsList.SelectedIndex = 0;
                    Refresh.IsEnabled = true;
                    Save.IsEnabled = true;
                }
            }
        }

        private void goforward()
        {
            TimeSpan temp = player.Position + TimeSpan.Parse("00:00:05");
            if (temp > player.NaturalDuration.TimeSpan)
                player.Position = player.NaturalDuration.TimeSpan;
            else
                player.Position = temp;
        }

        private void gobackward()
        {
            TimeSpan temp = player.Position - TimeSpan.Parse("00:00:05");
            if (temp < TimeSpan.Zero)
                player.Position = TimeSpan.Zero;
            else
                player.Position = temp;
        }


        private void LyricsList_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            int i = LyricsList.SelectedIndex;
            if (source.Lyrics[i][0] == '[')
            {
                source.Lyrics[i] = source.Lyrics[i].Remove(0, 10);
                LyricsList.SelectedIndex = i;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "保存lrc文件",
                Filter = "lrc文件|*.lrc",
                FileName = songname,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() != true)
                return;
            
            using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                {
                    for (int i = 0; i < source.Lyrics.Count; i++)
                        sw.WriteLine(source.Lyrics[i]);
                }
            }
        }

        private void slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            drag = false;
        }

        private void slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            int planned = (int)(duration.TotalSeconds * slider.Value / 10);
            player.Position = new TimeSpan(0, planned / 60, planned % 60);
            drag = true;
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            string str = "F7快退，F8快进，F9标注时间，每句可多次标注。\r\n右键单击某行可去掉该行标注的时间。\r\n"+
                "导入歌词可使用Ctrl+V粘贴或者使用txt";
            MessageBox.Show(str, "操作说明");
        }

    }
}
