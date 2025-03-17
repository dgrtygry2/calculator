using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private async void LoadWiFiNetworks()
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed) return;

            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
            if (devices.Count == 0) return;

            wifiAdapter = await WiFiAdapter.FromIdAsync(devices[0].Id);
            await wifiAdapter.ScanAsync();

            WifiListView.ItemsSource = wifiAdapter.NetworkReport.AvailableNetworks
                .Select(network => new WiFiNetwork { Ssid = network.Ssid, Network = network })
                .ToList();
        }

        private void RefreshNetworks(object sender, RoutedEventArgs e)
        {
            LoadWiFiNetworks();
        }

        private async void ConnectToNetwork(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedNetwork = button.DataContext as WiFiNetwork;

            if (selectedNetwork == null) return;

            var password = await ShowPasswordPromptAsync(selectedNetwork.Ssid);
            if (password == null) return;

            var credential = new PasswordCredential();
            credential.Password = password;

            await wifiAdapter.ConnectAsync(selectedNetwork.Network, WiFiReconnectionKind.Automatic, credential);
        }

        private async void ConnectToOtherNetwork(object sender, RoutedEventArgs e)
        {
            var (ssid, password) = await ShowOtherNetworkDialogAsync();
            if (string.IsNullOrEmpty(ssid) || string.IsNullOrEmpty(password)) return;

            var credential = new PasswordCredential { Password = password };

            await wifiAdapter.ConnectAsync(
                new WiFiAvailableNetwork { Ssid = ssid },
                WiFiReconnectionKind.Automatic,
                credential
            );
        }

        private async Task<string> ShowPasswordPromptAsync(string ssid)
        {
            TextBox passwordBox = new TextBox { PlaceholderText = "Enter Wi-Fi password", AcceptsReturn = false };
            ContentDialog inputDialog = new ContentDialog
            {
                Title = $"Enter password for {ssid}",
                Content = passwordBox,
                PrimaryButtonText = "Connect",
                CloseButtonText = "Cancel"
            };

            var result = await inputDialog.ShowAsync();
            return result == ContentDialogResult.Primary ? passwordBox.Text : null;
        }

        private async Task<(string ssid, string password)> ShowOtherNetworkDialogAsync()
        {
            StackPanel panel = new StackPanel { Spacing = 10 };
            TextBox ssidBox = new TextBox { PlaceholderText = "Network Name (SSID)" };
            TextBox passwordBox = new TextBox { PlaceholderText = "Enter Wi-Fi password", AcceptsReturn = false };
            panel.Children.Add(ssidBox);
            panel.Children.Add(passwordBox);

            ContentDialog inputDialog = new ContentDialog
            {
                Title = "Connect to Other Network",
                Content = panel,
                PrimaryButtonText = "Connect",
                CloseButtonText = "Cancel"
            };

            var result = await inputDialog.ShowAsync();
            return result == ContentDialogResult.Primary ? (ssidBox.Text, passwordBox.Text) : (null, null);
        }
    }

    public class WiFiNetwork
    {
        public string Ssid { get; set; }
        public WiFiAvailableNetwork Network { get; set; }
    }
}
