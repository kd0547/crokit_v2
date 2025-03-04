using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace crokit.util
{
    class DragSelectionAdorner : Adorner
    {
        private Point startPoint;
        private Point endPoint;
        private readonly Rectangle selectionRect;
        private readonly VisualCollection visuals;


        public DragSelectionAdorner(UIElement adornedElement, Point startPoint) : base(adornedElement)
        {
            this.startPoint = startPoint;
            this.endPoint = startPoint;
            visuals = new VisualCollection(this);

            selectionRect = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255))
            };
            visuals.Add(selectionRect);
        }
        /// <summary>
        /// 드래그 영역의 끝 점을 갱신
        /// </summary>
        public void UpdateEndPoint(Point point)
        {
            endPoint = point;
            InvalidateArrange();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // 시작점과 끝점에 따라 사각형의 위치와 크기를 계산
            double x = Math.Min(startPoint.X, endPoint.X);
            double y = Math.Min(startPoint.Y, endPoint.Y);
            double width = Math.Abs(startPoint.X - endPoint.X);
            double height = Math.Abs(startPoint.Y - endPoint.Y);
            selectionRect.Arrange(new Rect(x, y, width, height));
            return finalSize;
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Visual GetVisualChild(int index) => visuals[index];
    }
}
