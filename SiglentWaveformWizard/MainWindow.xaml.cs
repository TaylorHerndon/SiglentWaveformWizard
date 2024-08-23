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

namespace SiglentWaveformWizard
{
    public delegate void WaveformCanvasEvent(object sender);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SDG1032X? waveformGen;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try 
            { 
                waveformGen = new SDG1032X(IPTextBox.Text, (int)PortTextBox.Value);
                ConnectToggleButton.Background = new SolidColorBrush(Colors.DarkGreen);

                DeviceLabel.Content = waveformGen.ModelNumber;
                SerialNumberLabel.Content = waveformGen.SerialNumber;
            }
            catch (Exception ex)
            { 
                HandyControl.Controls.Growl.ErrorGlobal($"Error occurred: {ex.Message}");
                ConnectToggleButton.IsChecked = false; //Will also dispose of the device.
            }
            
        }

        private void ConnectToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            DeviceLabel.Content = "";
            SerialNumberLabel.Content = "";
            ConnectToggleButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1C, 0x1C, 0x1C));

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
            TimeDivLabel.Content = $"Time / Div: {WavCanvas.HorizontalScale.ToEngineering()}s";
        }

        private void SampleRateUpDown_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            if (WavCanvas == null) { return; }
            SampleRateSlider.ValueChanged -= SampleRateSlider_ValueChanged;
            SampleRateSlider.Value = SampleRateUpDown.Value;
            SampleRateSlider.ValueChanged += SampleRateSlider_ValueChanged;

            WavCanvas.SampleRate = (int)SampleRateUpDown.Value;
        }
        private void SampleRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WavCanvas == null) { return; }
            SampleRateUpDown.ValueChanged -= SampleRateUpDown_ValueChanged;
            SampleRateUpDown.Value = SampleRateSlider.Value;
            SampleRateUpDown.ValueChanged += SampleRateUpDown_ValueChanged;

            WavCanvas.SampleRate = (int)SampleRateSlider.Value;
        }
    }
}