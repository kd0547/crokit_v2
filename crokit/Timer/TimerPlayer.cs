using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using System.Xml.Schema;

namespace crokit.Timer
{
    public class TimerPlayer
    {
        private DispatcherTimer _timer;
        private TimeSpan _remaining;
        private TimeSpan _originTime;
        private bool _paused = false;
        public bool Pasused { get; private set; }

        public Action? OnTick;           // 매 초마다 호출
        public Action? OnCompleted;      // 타이머 종료 시 호출


        private int _saveHour = 0;
        public int SaveHour { get { return _saveHour; }  }

        private int _saveMinute = 0;
        public int SaveMinute { get { return _saveMinute; } }
        private int _saveSecond = 0;
        public int SaveSecond { get { return _saveSecond; } }

        public TimeSpan Remaining {
                get => _remaining;
        }
        
        private TaskCompletionSource<bool>? _tcs;

        public TimerPlayer()
        {

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

        }

        public Task WaitUntilFinishedAsync()
        {
            return _tcs.Task;
        }


        public void SetTimer(int hour,int minute,int second) 
        {
            if(!_paused)
            {
                _remaining = new TimeSpan(hour, minute, second);
                _originTime = _remaining;
                SvaeTime(hour, minute, second);
            }
            
        }
        private void SvaeTime(int hour, int minute, int second)
        {
            _saveHour = hour;
            _saveMinute = minute;
            _saveSecond = second;
        }

        public void StartTimer()
        {
            if(!_paused)
            {
                _remaining = _originTime;
                _paused = false;
                if (_tcs == null || _tcs.Task.IsCompleted)
                    _tcs = new TaskCompletionSource<bool>();
            }
            _timer.Start();
        }
  

        public void StopTimer()
        {
            _timer.Stop();
            if (_tcs != null && !_tcs.Task.IsCompleted)
            {
                _tcs.TrySetResult(true);
            }
            _remaining = _originTime;
            _paused = false;
            _tcs = null;

        }

        public void PauseTimer()
        {
            _paused = true;
            _timer?.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_remaining.TotalSeconds <= 0)
            {
                _timer.Stop();
                _paused = false;
                if (_tcs != null && !_tcs.Task.IsCompleted)
                {
                    _tcs.TrySetResult(true);
                }
                Debug.WriteLine("타이머 끝!");
                return;
            }
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            OnTick?.Invoke();

            Debug.WriteLine($"남은 시간: {_remaining}");
        }
    }
}
