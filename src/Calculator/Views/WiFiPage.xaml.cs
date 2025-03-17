using System;
using Windows.Devices.WiFi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Security.Credentials;

namespace CalculatorApp.Views
{
    public sealed partial class WiFiPage : Page
    {
        private WiFiAdapter wifiAdapter;

        public WiFiPage()
        {
            this.InitializeComponent();
            LoadWiFiNetworks();
        }

        // Load Wi-Fi networks
        private async void LoadWiFiNetworks()
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed) return;

            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
            if (devices.Count == 0) return;

            wifiAdapter = await WiFiAdapter.FromIdAsync(devices[0].Id);
            await wifiAdapter.ScanAsync();

            WifiListView.ItemsSource = wifiAdapter.NetworkReport.AvailableNetworks;
        }

        // Refresh the list of Wi-Fi networks
        private void RefreshNetworks(object sender, RoutedEventArgs e)
        {
            LoadWiFiNetworks();
        }

        // Connect to a selected Wi-Fi network
        private async void ConnectToNetwork(object sender, RoutedEventArgs e)
        {
            var selectedNetwork = (sender as Button).DataContext as WiFiAvailableNetwork;

            // Prompt for the Wi-Fi password
            var password = await ShowPasswordPromptAsync();
            if (password == null) return;

            var credentials = new PasswordCredential();
            credentials.Password = password;

            // Attempt to connect
            await wifiAdapter.ConnectAsync(selectedNetwork, WiFiReconnectionKind.Automatic, credentials);
        }

        // Show password input dialog
        private async Task<string> ShowPasswordPromptAsync()
        {
            var inputBox = new ContentDialog
            {
                Title = "Enter Wi-Fi Password",
                PrimaryButtonText = "Connect",
                SecondaryButtonText = "Cancel",
                Content = new TextBox { x:Name = "PasswordBox" }
            };

            var result = await inputBox.ShowAsync();
            var passwordBox = (TextBox)inputBox.Content;

            if (result == ContentDialogResult.Primary)
                return passwordBox.Text;

            return null;
        }
    }
}
