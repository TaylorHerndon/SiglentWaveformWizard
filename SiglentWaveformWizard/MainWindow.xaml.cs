// Ignore Spelling: Siglent

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using HandyControl.Themes;
using HandyControl.Tools;
using HandyControl.Data;
using SiglentWaveformWizard.Communications;
using SiglentWaveformWizard.Resources;
using SiglentWaveformWizard.UI;
using SiglentWaveformWizard.Communications.Supporting;
using System.Windows.Threading;

namespace SiglentWaveformWizard
{
    public delegate void WaveformCanvasEvent(object sender);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SDG1032X? waveformGen;
        DispatcherTimer exportTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            foreach (double time in Common.standardDivisionTimes) { HorizontalScaleComboBox.Items.Add($"{time.ToEngineering()}s"); }
            foreach (double voltage in Common.standardDivisionVoltages) { VerticalScaleComboBox.Items.Add($"{voltage.ToEngineering()}V"); }

            HorizontalScaleComboBox.SelectedItem = "1ms";
            VerticalScaleComboBox.SelectedItem = "1V";
            HorizontalScaleComboBox.SelectionChanged += HorizontalScaleComboBox_SelectionChanged;
            VerticalScaleComboBox.SelectionChanged += VerticalScaleComboBox_SelectionChanged;

            WavCanvas.SampleCount = 128;

            exportTimer.Interval = new TimeSpan(0, 0, 0, 1);
            exportTimer.Tick += ExportTimer_Tick;
            exportTimer.Start();
        }    

        private void ConnectToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try 
            { 
                waveformGen = new SDG1032X(IPTextBox.Text, (int)PortTextBox.Value);
                ConnectToggleButton.BorderBrush = new SolidColorBrush(Colors.DarkGreen);

                DeviceLabel.Content = waveformGen.ModelNumber;
                SerialNumberLabel.Content = waveformGen.SerialNumber;
                StartButton.IsEnabled = true;
            }
            catch (Exception ex)
            { 
                HandyControl.Controls.Growl.ErrorGlobal($"Error occurred: {ex.Message}");
                StartButton.IsEnabled = false;
                ConnectToggleButton.IsChecked = false; //Will also dispose of the device.
            }
            
        }

        private void ConnectToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            DeviceLabel.Content = "";
            SerialNumberLabel.Content = "";
            ConnectToggleButton.BorderBrush = new SolidColorBrush(Colors.DarkRed);

            waveformGen?.Close();
            waveformGen = null;
        }

        private void SampleCountUpDown_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            if (WavCanvas == null) { return; }
            SampleCountSlider.ValueChanged -= SampleCountSlider_ValueChanged;
            SampleCountSlider.Value = SampleCountUpDown.Value;
            SampleCountSlider.ValueChanged += SampleCountSlider_ValueChanged;

            WavCanvas.SampleCount = (int)SampleCountUpDown.Value;
        }

        private void SampleCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WavCanvas == null) { return; }
            SampleCountUpDown.ValueChanged -= SampleCountUpDown_ValueChanged;
            SampleCountUpDown.Value = SampleCountSlider.Value;
            SampleCountUpDown.ValueChanged += SampleCountUpDown_ValueChanged;

            WavCanvas.SampleCount = (int)SampleCountSlider.Value;
        }

        private void WavCanvas_WaveformRedrawComplete(object sender)
        {
            //TimeDivLabel.Content = $"Time / Div: {WavCanvas.HorizontalScale.ToEngineering()}s";
        }

        private void HorizontalScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WavCanvas == null) { return; }
            WavCanvas.HorizontalScale = Common.standardDivisionTimes[HorizontalScaleComboBox.SelectedIndex];
        }

        private void VerticalScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WavCanvas == null) { return; }
            WavCanvas.VerticalScale = Common.standardDivisionVoltages[VerticalScaleComboBox.SelectedIndex];
        }

        private void VectorLineButton_Checked(object sender, RoutedEventArgs e) => WavCanvas.VectorLines = true;
        private void VectorLineButton_Unchecked(object sender, RoutedEventArgs e) => WavCanvas.VectorLines = false;
        private void ConnectPointsToggleButton_Checked(object sender, RoutedEventArgs e) => WavCanvas.ConnectPoints = true;
        private void ConnectPointsToggleButton_Unchecked(object sender, RoutedEventArgs e) => WavCanvas.ConnectPoints = false;

        private void ExportTimer_Tick(object? sender, EventArgs e) 
        {
            if (WavCanvas.WaveformChanged && !(WavCanvas.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)) { ExportWavformToGenerator(); }
        }
        private void ExportWavformToGenerator()
        {
            if (waveformGen == null || StartButton.IsChecked == false) { return; }

            //Interpret data points of graph to voltage values to scaled 16bit values.
            double pixelsPerDiv = WavCanvas.ActualHeight / (WavCanvas.VerticalDivisions * 2);
            double verticalRatio = WavCanvas.VerticalScale / pixelsPerDiv;

            double[] dataPointsVolts = WavCanvas.DataPoints.Select(v => v * verticalRatio).ToArray();
            double maxVoltage = Math.Max(dataPointsVolts.Max(), Math.Abs(dataPointsVolts.Min()));

            short[] scaledPoints = dataPointsVolts.Select(v => (short)((v / maxVoltage) * 32767)).ToArray();

            waveformGen.Channels[0].Waveform = new ArbitraryWaveform("C1", WavCanvas.SampleRate / WavCanvas.SampleCount, (float)maxVoltage, scaledPoints);
            waveformGen.Channels[0].IsEnabled = true;

            waveformGen.Channels[1].ConfigAsSyncGen(WavCanvas.SampleRate / WavCanvas.SampleCount);
            waveformGen.Channels[1].IsEnabled = true;

            WavCanvas.WaveformChanged = false;
        }

        private void StartButton_Checked(object sender, RoutedEventArgs e)
        {
            if (waveformGen == null) { return; }

            StartButton.Content = "Running";
            StartButton.BorderBrush = new SolidColorBrush(Colors.DarkGreen);
            ExportWavformToGenerator();
        }

        private void StartButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (waveformGen == null) { return; }

            StartButton.Content = "Stopped";
            StartButton.BorderBrush = new SolidColorBrush(Colors.DarkRed);
            waveformGen.Channels.ForEach(ch => ch.IsEnabled = false);
        }
    }
}