using crokit.image;
using crokit.Timer;
using crokit.util;
using Microsoft.Win32;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crokit
{
    

    public partial class MainWindow : Window
    {
        private Point selectionStart;
        private DragSelectionAdorner selectionAdorner;
        private AdornerLayer adornerLayer;
        List<Border> borderList;
        private TimerPlayer timerPlayer;
        private TimerWindow timerWindow;

        private TimerViewModel timerViewModel;
        private ImageViewModel imageViewModel;
        private CroquisPlayer croquisPlayer;

        public MainWindow()
        {
            InitializeComponent();

            imageViewModel = new ImageViewModel();
            imageViewModel.RequestImageLoad = OpenImageDialog;
            DataContext = imageViewModel;

            timerPlayer = new TimerPlayer();
            timerViewModel = new TimerViewModel(timerPlayer);

            timerWindow = new TimerWindow();
            timerWindow.DataContext = timerViewModel;

            croquisPlayer = new CroquisPlayer(imageViewModel,timerViewModel, timerPlayer);

        }



        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                // 마우스 휠의 Delta 값에 따라 위(LineUp) 혹은 아래(LineDown) 스크롤
                if (e.Delta > 0)
                    scrollViewer.LineUp();
                else
                    scrollViewer.LineDown();

                // 이벤트 처리를 완료하여 부모 컨트롤로 이벤트가 전달되지 않도록 함
                e.Handled = true;
            }
        }

        public void OpenImageDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;)|*.jpg;*.jpeg;*.png;*.bmp;",
                Title = "사진 파일 선택",
                Multiselect = true
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true) 
            {
                var vm = DataContext as ImageViewModel;
                if (vm != null)
                {
                    vm?.AddImage(openFileDialog.FileNames);
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if (image != null)
            {
                SelectedImage.Source = image.Source;  
            }
        }

        private void ImageItemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemControl = sender as ItemsControl;
            if (itemControl == null)
                return;

            if(borderList != null)
            {
                foreach (var item in borderList)
                {
                    item.BorderBrush = null;
                    item.BorderThickness = new Thickness(0);
                }
            }

            selectionStart = e.GetPosition(itemControl);
            adornerLayer = AdornerLayer.GetAdornerLayer(itemControl);
            if(adornerLayer != null)
            {
                selectionAdorner = new DragSelectionAdorner(itemControl, selectionStart);
                adornerLayer.Add(selectionAdorner);
            }
            borderList = null;
            itemControl.CaptureMouse();
        }

        private void ImageItemsControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectionAdorner != null)
            {
                var itemsControl = sender as ItemsControl;
                if (itemsControl != null)
                {
                    Point currentPoint = e.GetPosition(itemsControl);
                    selectionAdorner.UpdateEndPoint(currentPoint);
                }
            }
        }

        private void ImageItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            if (itemsControl == null || selectionAdorner == null)
                return;
            itemsControl.ReleaseMouseCapture();
            // 최종 선택 영역 계산
            Point endPoint = e.GetPosition(itemsControl);
            Rect selectionRect = new Rect(
                Math.Min(selectionStart.X, endPoint.X),
                Math.Min(selectionStart.Y, endPoint.Y),
                Math.Abs(selectionStart.X - endPoint.X),
                Math.Abs(selectionStart.Y - endPoint.Y));

            List<Border> borders = new List<Border>();

            // 예: ItemsControl의 각 항목에 대해 선택 영역과의 교차 여부를 확인
            foreach (var item in itemsControl.Items)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    // 컨테이너의 위치를 ItemsControl 내 좌표로 변환
                    Point itemPos = container.TranslatePoint(new Point(0, 0), itemsControl);
                    Rect itemRect = new Rect(itemPos, container.RenderSize);
                    if (selectionRect.IntersectsWith(itemRect))
                    {
                        DependencyObject dependency = container as DependencyObject;
                        Border image = FindVisualChild<Border>(dependency);
                        if (image != null)
                        {
                            image.BorderBrush = new SolidColorBrush(Colors.White);
                            image.BorderThickness = new Thickness(1);
                            borders.Add(image);
                        }

                    }
                }
            }
            borderList = borders;
            // Adorner 제거
            adornerLayer.Remove(selectionAdorner);
            selectionAdorner = null;
        }
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
     
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timerWindow.Show();
        }
    }
}