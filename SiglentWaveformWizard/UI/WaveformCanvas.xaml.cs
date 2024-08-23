//Ignore Spelling: Siglent

using SiglentWaveformWizard.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SiglentWaveformWizard.UI
{
    /// <summary>
    /// Interaction logic for WaveformCanvas.xaml
    /// </summary>
    public partial class WaveformCanvas : UserControl
    {
        public event WaveformCanvasEvent? WaveformRedrawComplete;

        private List<double> standardDivisionTimes = new List<double>();
        public WaveformCanvas()
        {
            for (int i = 0; i < 6; i++)
            {
                standardDivisionTimes.Add(5 / Math.Pow(10, i));
                standardDivisionTimes.Add(2 / Math.Pow(10, i));
                standardDivisionTimes.Add(1 / Math.Pow(10, i));
            }

            InitializeComponent();
            GenerateSinWave();
        }

        private int sampleCount = 256;
        public int SampleCount 
        { 
            get => sampleCount; 
            set 
            { 
                sampleCount = value;
                int[] newDataPoints = new int[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    if (DataPoints.Length > i) { newDataPoints[i] = DataPoints[i]; }
                    else { newDataPoints[i] = 0; }
                }

                DrawWaveform();
            } 
        }

        private int sampleRate = 10000;
        /// <summary>
        /// Sample rate of waveform in Hz
        /// </summary>
        public int SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                DrawWaveform();
            }
        }

        public int[] DataPoints;

        public double HorizontalScale { get; private set; } = 0.001;
        public double HorizontalDivisions { get; set; } = 10;
        public double VerticalDivisions { get; set; } = 3;

        private void DrawLine(double x1, double y1, double x2, double y2, Color c, double thickness)
        {
            Math.Min(Math.Max(x1, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(x2, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(y1, 0), CanvasControl.ActualHeight);
            Math.Min(Math.Max(y2, 0), CanvasControl.ActualHeight);

            Line l = new Line();    
            l.Visibility = Visibility.Visible;
            l.StrokeThickness = thickness;
            l.Stroke = new SolidColorBrush(c);
            l.X1 = x1;
            l.Y2 = y2;
            l.X2 = x2;
            l.Y1 = y1;
            CanvasControl.Children.Add(l);
        }

        private void DrawDashedLine(double x1, double y1, double x2, double y2, Color c, double thickness)
        {
            Math.Min(Math.Max(x1, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(x2, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(y1, 0), CanvasControl.ActualHeight);
            Math.Min(Math.Max(y2, 0), CanvasControl.ActualHeight);

            Line l = new Line();
            l.Visibility = Visibility.Visible;
            l.StrokeThickness = thickness;
            l.Stroke = new SolidColorBrush(c);
            l.StrokeDashArray = new DoubleCollection() { 1, 3 };
            l.X1 = x1;
            l.Y2 = y2;
            l.X2 = x2;
            l.Y1 = y1;
            CanvasControl.Children.Add(l);
        }

        private void DrawRectangle(double x1, double y1, double x2, double y2, Color borderColor, double borderThickness, Color? fillColor = null)
        {
            Math.Min(Math.Max(x1, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(x2, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(y1, 0), CanvasControl.ActualHeight);
            Math.Min(Math.Max(y2, 0), CanvasControl.ActualHeight);

            Rectangle r = new Rectangle();
            r.Visibility = Visibility.Visible;
            r.StrokeThickness = borderThickness;
            r.Stroke = new SolidColorBrush(borderColor);
            r.Fill = new SolidColorBrush(fillColor ?? Colors.Transparent);
            r.Width = x2 - x1;
            r.Height = y2 - y1;
            Canvas.SetLeft(r, x1);
            Canvas.SetTop(r, y1);
            CanvasControl.Children.Add(r);
        }

        private void DrawCircle(double x, double y, double r, Color borderColor, double borderThickness, Color? fillColor = null)
        {
            Math.Min(Math.Max(x, 0), CanvasControl.ActualWidth);
            Math.Min(Math.Max(y, 0), CanvasControl.ActualHeight);

            Ellipse e = new Ellipse();
            e.Visibility = Visibility.Visible;
            e.StrokeThickness = borderThickness;
            e.Stroke = new SolidColorBrush(borderColor);
            e.Fill = new SolidColorBrush(fillColor ?? Colors.Transparent);
            e.Width = r * 2;
            e.Height = r * 2;
            Canvas.SetLeft(e, x);
            Canvas.SetTop(e, y);
            CanvasControl.Children.Add(e);
        }

        public void DrawGrid()
        {
            double verticalCenter = CanvasControl.ActualHeight / 2;
            double divisionWidth = CanvasControl.ActualWidth / HorizontalDivisions;
            double divisionHeight = CanvasControl.ActualHeight / (VerticalDivisions * 2);

            DrawLine(0, verticalCenter, CanvasControl.ActualWidth, verticalCenter, Colors.LightGray, 3);
            for (int i = 0; i < HorizontalDivisions + 1; i++)
            {
                DrawLine(divisionWidth * i, verticalCenter - 10, divisionWidth * i, verticalCenter + 10, Colors.LightGray, 2);
                if (i != 0 && i != 10) { DrawDashedLine(divisionWidth * i, 0, divisionWidth * i, CanvasControl.ActualHeight, Color.FromArgb(0xFF, 0x30, 0x30, 0x30), 2); }
            }

            for (int i = 1; i < VerticalDivisions; i++)
            {
                DrawDashedLine(0, verticalCenter + (divisionHeight * i), CanvasControl.ActualWidth, verticalCenter + (divisionHeight * i), Color.FromArgb(0xFF, 0x30, 0x30, 0x30), 2);
                DrawDashedLine(0, verticalCenter - (divisionHeight * i), CanvasControl.ActualWidth, verticalCenter - (divisionHeight * i), Color.FromArgb(0xFF, 0x30, 0x30, 0x30), 2);
            }
        }

        public void DrawDataPoints()
        {
            double verticalCenter = CanvasControl.ActualHeight / 2;

            //double samplePeriod = 1 / (double)sampleRate;
            //double timePerPixel = (HorizontalScale * 10) / CanvasControl.ActualWidth;
            //double pixelsPerSample = samplePeriod / timePerPixel;

            //double pixelsPerSample = (double)sampleRate * ((HorizontalScale * 10) / CanvasControl.ActualWidth);

            double pixelsPerSample = CanvasControl.ActualWidth / (SampleRate * HorizontalScale * 10);

            //double a = (HorizontalScale * 10);
            //double b = ((double)sampleRate * (double)CanvasControl.ActualWidth);
            //double pixelsPerSample = a / b;

            for (int i = 0; i < DataPoints.Length; i++)
            {
                if (i * pixelsPerSample > CanvasControl.ActualWidth) { break; } 
                DrawCircle(i * pixelsPerSample, verticalCenter - DataPoints[i], 1, Colors.Green, 1);
            }
        }

        public void GenerateSinWave()
        {
            DataPoints = new int[SampleCount];

            double period = 0.002;
            double voltage = 100;

            double samplePeriod = 1 / (double)sampleRate;
            double samplesPerCycle = (int)(period / samplePeriod);

            for (int i = 0; i < DataPoints.Length; i++) 
            {
                DataPoints[i] = (int)(Math.Sin(((i % samplesPerCycle) / (double)samplesPerCycle) * 2 * Math.PI) * voltage); 
            }

            //Common.InfoPopup(string.Join('\n', DataPoints));
        }

        private void DrawWaveform()
        {
            if (CanvasControl == null) { return; }
            CanvasControl.Children.Clear();
            GenerateSinWave();

            DrawGrid();
            DrawDataPoints();

            //double totalWaveformTime = ((double)sampleCount / (double)sampleRate);
            //double idealDivisionTime = totalWaveformTime / 10f;
            //double standardDivisionTime = standardDivisionTimes.FirstOrDefault(div => idealDivisionTime >= div, 0.001);
            //double samplesBetweenDivisions = standardDivisionTime * sampleRate;
            //HorizontalScale = standardDivisionTime;

            //DrawLine(0, verticalCenter, SampleCount, verticalCenter, Colors.LightGray, 3);
            //for (int i = 0; i < Math.Floor(totalWaveformTime / standardDivisionTime); i++) 
            //{ DrawLine(samplesBetweenDivisions * i, verticalCenter + 10, samplesBetweenDivisions * i, verticalCenter - 10, Colors.LightGray, 2); }

            WaveformRedrawComplete?.Invoke(this);
        }

        private void CanvasControl_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void CanvasControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawWaveform();
        }
    }
}
