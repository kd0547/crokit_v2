using crokit.image;
using crokit.Timer;
using crokit.util;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crokit
{
    enum ResizeDirection
    {
        None,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft
    }

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

            croquisPlayer = new CroquisPlayer(imageViewModel, timerViewModel, timerPlayer);

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

            if (borderList != null)
            {
                foreach (var item in borderList)
                {
                    item.BorderBrush = null;
                    item.BorderThickness = new Thickness(0);
                }
            }

            selectionStart = e.GetPosition(itemControl);
            adornerLayer = AdornerLayer.GetAdornerLayer(itemControl);
            if (adornerLayer != null)
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

     

        private ResizeDirection GetResizeDirection(Point position,int index)
        {

            bool left = position.X <= index;
            bool right = position.X >= this.ActualWidth - index;
            bool top = position.Y <= index;
            bool bottom = position.Y >= this.ActualHeight - index;

            // 꼭짓점 (모서리)
            if (left && top) return ResizeDirection.TopLeft;
            if (right && top) return ResizeDirection.TopRight;
            if (left && bottom) return ResizeDirection.BottomLeft;
            if (right && bottom) return ResizeDirection.BottomRight;

            // 가장자리
            if (left) return ResizeDirection.Left;
            if (right) return ResizeDirection.Right;
            if (top) return ResizeDirection.Top;
            if (bottom) return ResizeDirection.Bottom;

            return ResizeDirection.None;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            int index = 10;

            Point position = e.GetPosition(this);
            ResizeDirection _resizeDirection = GetResizeDirection(position, index);
            Debug.WriteLine(_resizeDirection);

            if ((position.X > index && this.ActualWidth - index > position.X)
                && (position.Y > index && this.ActualHeight - index > position.Y))
            {
                this.Cursor = Cursors.Arrow;
            }
            else
            {

                switch (_resizeDirection)
                {
                    case ResizeDirection.BottomRight:
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case ResizeDirection.TopLeft:
                        this.Cursor = Cursors.SizeNWSE;
                        break;  
                    case ResizeDirection.Left:
                    case ResizeDirection.Right:
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case ResizeDirection.Top:
                    case ResizeDirection.Bottom:
                        this.Cursor = Cursors.SizeNS;
                        break;
                    default: break;
                }

            }

            if(e.LeftButton == MouseButtonState.Pressed) {

                ReleaseCapture();
                var hWnd = new WindowInteropHelper(this).Handle;
                switch (_resizeDirection)
                {
                    case ResizeDirection.Left:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTLEFT, IntPtr.Zero);
                        break;
                    case ResizeDirection.BottomRight:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTBOTTOMRIGHT, IntPtr.Zero);
                        break;
                    case ResizeDirection.Top:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTTOP, IntPtr.Zero);
                        break;
                    case ResizeDirection.Right:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTRIGHT, IntPtr.Zero);
                        break;
                    case ResizeDirection.Bottom:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTBOTTOM, IntPtr.Zero);
                        break;
                    case ResizeDirection.TopLeft:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTTOPLEFT, IntPtr.Zero);
                        break;
                    case ResizeDirection.TopRight:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTTOPRIGHT, IntPtr.Zero);
                        break;
                    case ResizeDirection.BottomLeft:
                        SendMessage(hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTBOTTOMLEFT, IntPtr.Zero);
                        break;
                }
            }

        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int HTLEFT = 10;         // 왼쪽 테두리
        const int HTRIGHT = 11;        // 오른쪽 테두리
        const int HTTOP = 12;
        const int HTBOTTOM = 15;
        const int HTTOPLEFT = 13;
        const int HTTOPRIGHT = 14;
        const int HTBOTTOMLEFT = 16;
        const int HTBOTTOMRIGHT = 17;
        private void Windows_Resize(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
            }
        }
    }
}