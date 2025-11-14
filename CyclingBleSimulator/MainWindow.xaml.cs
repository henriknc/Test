using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CyclingBleSimulator.ViewModels;

namespace CyclingBleSimulator
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Add the color converter as a resource
            Resources.Add("BoolToColorConverter", new BoolToColorConverter());
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.StartSelectedDeviceAsync();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.StopSelectedDevice();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedDevice?.ResetMetrics();
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddDevice();
        }

        private void RemoveDevice_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedDevice != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{_viewModel.SelectedDevice.DeviceName}'?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.RemoveDevice(_viewModel.SelectedDevice);
                }
            }
        }

        private void StartAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.StartAllDevices();
        }

        private void StopAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.StopAllDevices();
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.Dispose();
            base.OnClosed(e);
        }
    }

    // Converter to show green when running, red when stopped
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRunning)
            {
                return isRunning
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))   // Green
                    : new SolidColorBrush(Color.FromRgb(244, 67, 54));  // Red
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
