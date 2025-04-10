using crokit.image;
using crokit.Timer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace crokit
{
    public class CroquisPlayer
    {
        
        private int _index = 0;
        private bool _running = false;
        private bool _stop = false;
        private ImageViewModel? _imageVIewModel;
        private TimerViewModel? _timerVIewModel;
        private TimerPlayer _timerPlayer;
        
        
        public CroquisPlayer(ImageViewModel imageViewModel,TimerViewModel timerViewModel,TimerPlayer timerPlayer)
        {
            _imageVIewModel = imageViewModel;
            _timerVIewModel = timerViewModel;
            _timerPlayer = timerPlayer;
            Init();
        }

        private void Init()
        {
            _timerVIewModel.OnStart = StartCroquisAsync;
            _timerVIewModel.OnStop = StopCroquisAsync;
            _timerVIewModel.OnPause = PauseCroquisAsync;
        }


        public async void StartCroquisAsync() 
        {
            int count = _imageVIewModel.TotalImage();
            count = 2;
            if (count == 0)
                return;
            for(int i = 0; i < count; i++)
            {
                if (_stop)
                {
                    _stop = false;
                    return;
                }
                _timerPlayer.StartTimer();
                Debug.WriteLine(i);

                await _timerPlayer.WaitUntilFinishedAsync();
            }
            _timerVIewModel.Finish();
            //_timerPlayer.StopTimer();

        }

        public void StopCroquisAsync()
        {
            _timerPlayer.StopTimer();
            _stop = true;
        }

        public void PauseCroquisAsync()
        {
            _timerPlayer.PauseTimer();
        }
    }
}
