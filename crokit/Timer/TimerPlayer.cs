using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace crokit.Timer
{
    public class TimerPlayer
    {
        private DispatcherTimer _timer;
        private TimeSpan _remaining;

        public Action? OnTick;           // 매 초마다 호출
        public Action? OnCompleted;      // 타이머 종료 시 호출

        public TimeSpan Remaining {
                get => _remaining;
        }


        public void SetTimer(int hour,int minute,int second) 
        {
            _remaining = new TimeSpan(hour, minute, second);
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
        }

        public void StartTimer()
        {
            _timer.Start();
        }
  

        public void Stoptimer()
        {
            _timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_remaining.TotalSeconds <= 0)
            {
                _timer.Stop();
                Debug.WriteLine("타이머 끝!");
                return;
            }
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            OnTick?.Invoke();

            Debug.WriteLine($"남은 시간: {_remaining}");
        }
    }
}
