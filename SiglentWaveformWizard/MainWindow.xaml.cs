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

namespace SiglentWaveformWizard
{
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

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            string? response = waveformGen?.IDN();
            if (response != null) { MessageBox.Show(response); }
        }

        private void FuncButton_Click(object sender, RoutedEventArgs e)
        {
            string? response = waveformGen?.Channels[0].OutputState;
            if (response != null) { Common.InfoPopup(response); }
        }
    }
}