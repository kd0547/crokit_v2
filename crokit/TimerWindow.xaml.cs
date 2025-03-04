using crokit.util;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace crokit
{
    /// <summary>
    /// TimerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TimerWindow : Window
    {
        public TimerWindow()
        {
            InitializeComponent();
            
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            ResizeCursorHelper.UpdateResizeCursor(this,sender,e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ResizeCursorHelper.UpdateResizeCursor(this, sender, e);
        }




    }
}
