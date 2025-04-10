using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace crokit.Timer
{
    public class TimerViewModel : INotifyPropertyChanged
    {
        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }

        public ICommand StopCommand { get; }

        public Action? OnStart;
        public Action? OnPause;
        public Action? OnStop;

        //타이머 설정 활성화/비활성화
        private bool _isTimeEditable = true;
        public bool IsTimeEditable {  
            get { return _isTimeEditable; }
            set { 
                _isTimeEditable = value;
                OnPropertyChanged();
            }
        }
        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }
        private void RaiseCommandStates()
        {
            (StartCommand as StartCommand)?.RaiseCanExecuteChanged();
            (PauseCommand as StartCommand)?.RaiseCanExecuteChanged();
            (StopCommand as StartCommand)?.RaiseCanExecuteChanged();
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
        public TimerViewModel(TimerPlayer timerPlayer)
        {
            _timer = timerPlayer;
            _timer.OnTick = ShowTimer;
            StartCommand = new StartCommand(Start, () => (!(Hour == "00" && Minute == "00" && Seconds == "00") && !IsRunning));
            PauseCommand = new StartCommand(Pause, () => IsRunning);
            StopCommand = new StartCommand(Stop, () => IsRunning || !_isStop);
        }

        public void Start()
        {
            IsTimeEditable = false;
            int h = int.Parse(Hour);
            int m = int.Parse(Minute);
            int s = int.Parse(Seconds);
            _timer.SetTimer(h, m, s);

            SaveTime(h, m, s);
            IsRunning = true;

            OnStart?.Invoke();
        }

        private bool _isStop = true;

        private void SaveTime(int h, int m, int s)
        {
            if(_isStop) {
                _saveHour = h;
                _saveMinute = m;
                _saveSecond = s;
                _isStop = false;
            }
        }


        public void Pause()
        {

            if (_timer == null)
            {
                return;
            }
            IsTimeEditable = true;
            IsRunning = false;
            //_timer.Stoptimer();
            OnPause?.Invoke();

        }

        private int _saveHour = 0;
        private int _saveMinute = 0;
        private int _saveSecond = 0;

        public void Stop()
        {
            if (_timer == null)
                return;
            IsRunning = false;
            OnStop?.Invoke();
            IsTimeEditable = true;
            ReloadTime(_saveHour, _saveMinute, _saveSecond);

        }

        private void ReloadTime(int saveHour, int saveMinute, int saveSecond)
        {
            if (!_isStop) 
            {
                Hour = saveHour.ToString("D2");
                Minute = saveMinute.ToString("D2");
                Seconds = saveSecond.ToString("D2");
            }
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
