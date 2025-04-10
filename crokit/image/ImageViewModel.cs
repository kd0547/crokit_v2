using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace crokit.image
{
    public class ImageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> UserImages { get; set; } = new ObservableCollection<string>();

        
        private string _NextImage;
        public string NextImage
        {
            get
            {
                return _NextImage;
            }
            set
            {
                if (_NextImage != value)
                {
                    _NextImage = value;
                    OnPropertyChanged();
                }
            }
        }

        public Action? RequestImageLoad { get; set; }
        public ICommand LoadImagesCommand { get; }  

        public ImageViewModel() 
        {
            LoadImagesCommand = new ImageLoadCommand(ImageLoad);
        }


        public void ImageLoad()
        {
            RequestImageLoad?.Invoke();
        }

        public void AddImage(IEnumerable<string> images)
        {
            foreach (var image in images)
            {
                UserImages.Add(image);
            }
        }

        private int count = 0;

        /// <summary>
        /// 이미지를 보여줌 
        /// </summary>
        public void NextImageShow()
        {
            string imagePath = UserImages[count];
            NextImage = imagePath;
            count++;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name ="") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
