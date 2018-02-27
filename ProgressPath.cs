using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Controls
{
    public class ProgressPath : Shape
    {
        private double progressPathLength;
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Geometry), typeof(ProgressPath),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(ProgressPath), new PropertyMetadata(0.0, OnProgressChanged));

        static ProgressPath()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressPath), new FrameworkPropertyMetadata(typeof(Path)));
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressPath = d as ProgressPath;
            progressPath.OnDataChanged();
        }
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressPath = d as ProgressPath;
            progressPath.Update();
        }

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                Geometry data = Data;
                return data ?? Geometry.Empty;
            }
        }
        private void Update()
        {
            double strokeDashLength = progressPathLength / StrokeThickness * Progress / 100;
            double strokeDashOffset = progressPathLength / StrokeThickness;
            StrokeDashArray = new DoubleCollection { strokeDashLength, strokeDashOffset };
        }
        private void OnDataChanged()
        {
            progressPathLength = GetGeometryLength(Data);
            Update();
        }

        private double GetGeometryLength(Geometry geometry)
        {
            double result = 0;
            var pathGeometry = geometry as PathGeometry;
            //var pathGeometry = geometry.GetFlattenedPathGeometry();
            if (pathGeometry != null)
            {
                foreach (var figure in pathGeometry.Figures)
                {
                    var currentPoint = figure.StartPoint;
                    foreach (var segment in figure.Segments)
                    {
                        if (segment is BezierSegment)
                        {
                            var bezier = segment as BezierSegment;
                            result += GetBezierLength(currentPoint, bezier.Point1, bezier.Point2, bezier.Point3);
                            currentPoint = bezier.Point3;
                        }
                        else if (segment is LineSegment)
                        {
                            var line = segment as LineSegment;
                            result += GetLength(currentPoint, line.Point);
                            currentPoint = line.Point;
                        }
                        else if (segment is PolyLineSegment)
                        {
                            var polyline = segment as PolyLineSegment;
                            var allPoints = currentPoint.And(polyline.Points).ToArray();
                            result += GetPolyLineLength(allPoints);
                            currentPoint = polyline.Points.Last();
                        }
                        else if (segment is ArcSegment)
                        {
                            var arc = segment as ArcSegment;
                            result += GetArcLength(currentPoint, arc);
                            currentPoint = arc.Point;
                        }
                    }
                }
            }

            return result;
        }

        private double GetArcLength(Point startPoint, ArcSegment arc)
        {
            throw new NotSupportedException();
        }

        private double GetBezierLength(Point p0, Point p1, Point p2, Point p3)
        {
            double result = 0;
            Point lastPoint = p0;

            for (double t = 0.001; t <= 1; t += 0.001)
            {
                var x = Poly(p0.X, p1.X, p2.X, p3.X, t);
                var y = Poly(p0.Y, p1.Y, p2.Y, p3.Y, t);
                var currentPoint = new Point(x, y);
                double dx = currentPoint.X - lastPoint.X;
                double dy = currentPoint.Y - lastPoint.Y;
                result += Math.Sqrt((dx * dx) + (dy * dy));
                lastPoint = currentPoint;
            }

            return result;
        }

        private static double Poly(double p0, double p1, double p2, double p3, double x)
        {
            return Math.Pow(1 - x, 3) * p0 +
                3 * x * Math.Pow(1 - x, 2) * p1 +
                3 * Math.Pow(x, 2) * (1 - x) * p2 +
                Math.Pow(x, 3) * p3;
        }

        private double GetPolyLineLength(Point[] points)
        {
            double result = 0.0;
            for (int i = 1; i < points.Length; i++)
            {
                result += GetLength(points[i], points[i - 1]);
            }
            return result;
        }

        private double GetLength(Point p0, Point p1)
        {
            double dx = p0.X - p1.X;
            double dy = p0.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}