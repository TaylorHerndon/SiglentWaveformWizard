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
        public WaveformCanvas()
        {
            InitializeComponent();
            GenerateSinWave();
        }

        private int sampleRate = 10000;
        public int SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                GenerateSinWave();
                DrawWaveform();
            }
        }

        private int sampleCount = 256;
        public int SampleCount 
        { 
            get => sampleCount; 
            set 
            { 
                sampleCount = value;

                //If you change the number of samples then sample rate will need to change to fit the waveform to the screen.
                sampleRate = (int)(sampleCount  / (HorizontalScale * 10));

                double[] newDataPoints = new double[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    if (DataPoints.Length > i) { newDataPoints[i] = DataPoints[i]; }
                    else { newDataPoints[i] = 0; }
                }
                GenerateSinWave();
                DrawWaveform();
            } 
        }

        private double horizontalScale = 0.001;
        public double HorizontalScale 
        { 
            get => horizontalScale;
            set 
            {
                if (!Common.standardDivisionTimes.Contains(value)) { return; }
                horizontalScale = value;

                //Change the sample rate to fit the new window size.
                sampleRate = (int)(sampleCount / (HorizontalScale * 10));
                GenerateSinWave();
                DrawWaveform();
            }
        }

        private double verticalScale = 1.0;
        public double VerticalScale
        {
            get => verticalScale;
            set
            {
                if (!Common.standardDivisionVoltages.Contains(value)) { return; }

                double currentVoltsPerPixel = (CanvasControl.ActualHeight / (VerticalDivisions * 2)) / verticalScale;
                double newVoltsPerPixel = (CanvasControl.ActualHeight / (VerticalDivisions * 2)) / value;
                double voltPerPixelConversion = newVoltsPerPixel / currentVoltsPerPixel;
                verticalScale = value;

                double[] newDataPoints = new double[DataPoints.Length];
                for (int i = 0; i < newDataPoints.Length; i++)
                {
                    newDataPoints[i] = DataPoints[i] * voltPerPixelConversion;
                }

                DataPoints = newDataPoints;
                DrawWaveform();
            }
        }

        public double[] DataPoints;
        public double HorizontalDivisions { get; set; } = 10;
        public double VerticalDivisions { get; set; } = 3;

        private bool vectorLines = false;
        public bool VectorLines 
        { 
            get => vectorLines;
            set { vectorLines = value; DrawWaveform(); } 
        }

        private bool connectPoints = false;
        public bool ConnectPoints
        {
            get => connectPoints;
            set { connectPoints = value; DrawWaveform(); }
        }

        public bool WaveformChanged { get; set; }

        private void DrawLine(double x1, double y1, double x2, double y2, Color c, double thickness)
        {
            x1 = Math.Min(Math.Max(x1, 0), CanvasControl.ActualWidth);
            x2 = Math.Min(Math.Max(x2, 0), CanvasControl.ActualWidth);
            y1 = Math.Min(Math.Max(y1, -2), CanvasControl.ActualHeight - 2);
            y2 = Math.Min(Math.Max(y2, -2), CanvasControl.ActualHeight - 2);

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
            x1 = Math.Min(Math.Max(x1, 0), CanvasControl.ActualWidth);
            x2 = Math.Min(Math.Max(x2, 0), CanvasControl.ActualWidth);
            y1 = Math.Min(Math.Max(y1, -2), CanvasControl.ActualHeight - 2);
            y2 = Math.Min(Math.Max(y2, -2), CanvasControl.ActualHeight - 2);

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
            x1 = Math.Min(Math.Max(x1, 0), CanvasControl.ActualWidth);
            x2 = Math.Min(Math.Max(x2, 0), CanvasControl.ActualWidth);
            y1 = Math.Min(Math.Max(y1, -2), CanvasControl.ActualHeight - 2);
            y2 = Math.Min(Math.Max(y2, -2), CanvasControl.ActualHeight - 2);

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
            x = Math.Min(Math.Max(x, 0), CanvasControl.ActualWidth);
            y = Math.Min(Math.Max(y, -2), CanvasControl.ActualHeight - 2);

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

            DrawLine(0, verticalCenter, CanvasControl.ActualWidth, verticalCenter, Colors.LightGray, 1);
            for (int i = 0; i < HorizontalDivisions + 1; i++)
            {
                DrawLine(divisionWidth * i, verticalCenter - 10, divisionWidth * i, verticalCenter + 10, Colors.LightGray, 1);
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
            double pixelsPerSample = CanvasControl.ActualWidth / (SampleRate * HorizontalScale * 10);

            for (int i = 0; i < DataPoints.Length; i++)
            {
                if (i * pixelsPerSample > CanvasControl.ActualWidth) { break; }

                double x = i * pixelsPerSample;
                double y = verticalCenter - DataPoints[i];

                DrawCircle(x - 2, y - 2, 2, Colors.Green, 1, Colors.Green);
                if (vectorLines) { DrawLine(x, y, x, verticalCenter, Colors.Green, 1); }
                if (connectPoints && i != 0) { DrawLine((i - 1) * pixelsPerSample, verticalCenter - DataPoints[i - 1], x, y, Colors.Green, 1);  }
            }
        }

        public void GenerateSinWave()
        {
            DataPoints = new double[SampleCount];

            //double period = 0.002;
            double period = HorizontalScale * 5;
            double voltage = 100;

            double samplePeriod = 1 / (double)sampleRate;
            double samplesPerCycle = (period / samplePeriod);

            for (int i = 0; i < DataPoints.Length; i++) 
            {
                //DataPoints[i] = (int)(Math.Sin(((i % samplesPerCycle) / (double)samplesPerCycle) * 2 * Math.PI) * voltage); 
                DataPoints[i] = (Math.Sin((i / samplesPerCycle) * 2 * Math.PI) * voltage);
            }

            WaveformChanged = true;
        }

        private void DrawWaveform()
        {
            if (CanvasControl == null) { return; }
            CanvasControl.Children.Clear();

            DrawGrid();
            DrawDataPoints();

            WaveformRedrawComplete?.Invoke(this);
        }

        private void CanvasControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton != MouseButtonState.Pressed) { return; }
            Point mousePos = e.GetPosition(CanvasControl);
            double percentOfXAxis = mousePos.X / CanvasControl.ActualWidth;
            int corespondingIndex = Math.Min((int)Math.Round(percentOfXAxis * DataPoints.Length), DataPoints.Length - 1);
            double verticalCenter = CanvasControl.ActualHeight / 2;
            DataPoints[corespondingIndex] = verticalCenter - mousePos.Y;
            DrawWaveform();
            WaveformChanged = true;
        }

        private void CanvasControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawWaveform();
        }
    }
}
