using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace crokit.Timer
{
    class TimerModelView : INotifyPropertyChanged
    {
        public ICommand StartCommand { get; }

        //타이머 설정 활성화/비활성화
        private bool _isTimeEditable = true;
        public bool IsTimeEditable {  
            get { return _isTimeEditable; }
            set { 
                _isTimeEditable = value;
            }
        }

        private string _hour = "00";

        public string Hour { 
            get => _hour; 
            set
            {
                string hour = isTextValid(value);
                if (_hour != hour) 
                {
                    _hour = hour;
                    OnPropertyChanged();
                    (StartCommand as StartCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _minute = "00";
        public string Minute
        {
            get => _minute;
            set
            {
                string minute = isTextValid(value);
                if (_minute != minute)
                {
                    _minute = minute;
                    OnPropertyChanged();
                    (StartCommand as StartCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _seconds = "00";

        public string Seconds {
            get => _seconds;
            set
            {
                string seconds = isTextValid(value);
                if (_seconds != seconds)
                {
                    _seconds = seconds;
                    OnPropertyChanged();
                    (StartCommand as StartCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private TimerPlayer _timer { get; set; }
        public TimerModelView()
        {
            _timer = new TimerPlayer(this);
            _timer.OnTick = ShowTimer;
            StartCommand = new StartCommand(Start, () => !(Hour == "00" && Minute == "00" && Seconds == "00"));
        }

        public void Start()
        {
            IsTimeEditable = false;
            int h = int.Parse(Hour);
            int m = int.Parse(Minute);
            int s = int.Parse(Seconds);
            _timer.StartTimer(h, m, s);
        }

        public void ShowTimer()
        {
            TimeSpan time = _timer.Remaining;
            int hours = time.Hours;
            int minutes = time.Minutes;
            int seconds = time.Seconds;

            Hour = hours.ToString("D2");
            Minute = minutes.ToString("D2");
            Seconds = seconds.ToString("D2");

        }


        public string isTextValid(string input)
        {
            // 숫자만 남기기
            input = Regex.Replace(input, @"\D", "");

            // 최대 2자리 제한
            if (input.Length > 2)
            {
                input = input.Substring(0, 2);
            }
            

            return input;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
