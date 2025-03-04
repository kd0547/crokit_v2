using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics.Eventing.Reader;

namespace crokit.util
{
    public enum ResizeDirection
    {
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8,
        None = 9
    }
    public static class ResizeCursorHelper
    {
        public static void UpdateResizeCursor(Window window, object sender, MouseEventArgs e)
        {
            window.Cursor = Cursors.Arrow;

            ResizeDirection resizeDirection = SetCursor(window, sender, e);

            if (resizeDirection == ResizeDirection.TopLeft)
            {
                window.Cursor = Cursors.SizeNWSE; // 좌측 상단
            }
            else if (resizeDirection == ResizeDirection.BottomRight)
            {
                window.Cursor = Cursors.SizeNWSE; // 우측 하단
            }
            else if (resizeDirection == ResizeDirection.TopRight)
            {
                window.Cursor = Cursors.SizeNESW; // 우측 상단 (추가)
            }
            else if(resizeDirection == ResizeDirection.BottomLeft)
            {
                window.Cursor = Cursors.SizeNESW; // 좌측 하단 (추가)
            }
            else if(resizeDirection == ResizeDirection.Left)
            {
                window.Cursor = Cursors.SizeWE;    // 좌측
            }
            else if(resizeDirection== ResizeDirection.Right)
            {
                window.Cursor = Cursors.SizeWE;    // 우측
            }
            else if(resizeDirection == ResizeDirection.Top)
            {
                window.Cursor = Cursors.SizeNS;    // 상단
            }
            else if(resizeDirection == ResizeDirection.Bottom)
            {
                window.Cursor = Cursors.SizeNS;    // 하단
            }
        }
        public static void UpdateResizeCursor(Window window, object sender, MouseButtonEventArgs e)
        {

            ResizeDirection resizeDirection = SetCursor(window, sender, e);
            ResizeWindow(window, resizeDirection);
        }
        public static ResizeDirection SetCursor(Window window, object sender, MouseEventArgs e)
        {

            var border = sender as Border;
            
            var position = e.GetPosition(border);
            var borderWidth = border.BorderThickness.Left;

            if (position.X <= borderWidth || position.X >= border.ActualWidth - borderWidth ||
                position.Y <= borderWidth || position.Y >= border.ActualHeight - borderWidth)
            {
                // 마우스가 테두리 영역 내에 있을 때
                // 여기에 특정 동작을 구현합니다.
                var mousePos = e.GetPosition(window);
                var width = window.ActualWidth;
                var height = window.ActualHeight;

                const double edgeSize = 5.0; // 창 모서리에서의 크기 조절 감지 범위

                if (mousePos.X != 0 && mousePos.Y != 0)
                {
                    var direction = ResizeDirection.TopLeft;

                    if (mousePos.X <= edgeSize && mousePos.Y <= edgeSize)
                    {
                        return ResizeDirection.TopLeft;
                    }
                    else if (mousePos.X >= width - edgeSize && mousePos.Y >= height - edgeSize)
                    {
                        return ResizeDirection.BottomRight;
                    }
                    else if (mousePos.X >= width - edgeSize && mousePos.Y <= edgeSize)
                    {
                        return ResizeDirection.TopRight;
                    }
                    else if (mousePos.X <= edgeSize && mousePos.Y >= height - edgeSize)
                    {
                        return ResizeDirection.BottomLeft;
                    }
                    else if (mousePos.X <= edgeSize)
                    {
                        return ResizeDirection.Left;
                    }
                    else if (mousePos.X >= width - edgeSize)
                    {
                        return ResizeDirection.Right;
                    }
                    else if (mousePos.Y <= edgeSize)
                    {
                        return ResizeDirection.Top;
                    }
                    else if (mousePos.Y >= height - edgeSize)
                    {
                        return ResizeDirection.Bottom;
                    }
                }
            }
            return ResizeDirection.None;

        }
        private static void ResizeWindow(Window window,ResizeDirection direction)
        {
            SendMessage(new WindowInteropHelper(window).Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    }
}
