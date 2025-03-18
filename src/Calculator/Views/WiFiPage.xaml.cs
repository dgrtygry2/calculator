using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CalculatorApp.Views
{
    public sealed partial class WiFiPage : Page
    {
        private WiFiAdapter wifiAdapter;
        private List<string> savedNetworks;

        public WiFiPage()
        {
            this.InitializeComponent();
            LoadSavedNetworks();
            LoadWiFiNetworks();
        }

        private async void LoadWiFiNetworks()
        {
            StatusText.Visibility = Visibility.Collapsed;

            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed) return;

            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
            if (devices.Count == 0) return;

            wifiAdapter = await WiFiAdapter.FromIdAsync(devices[0].Id);
            await wifiAdapter.ScanAsync();

            var availableNetworks = wifiAdapter.NetworkReport.AvailableNetworks
                .Select(network => new WiFiNetwork
                {
                    Ssid = network.Ssid,
                    Network = network,
                    SignalEmoji = GetSignalEmoji(network.SignalBars),
                    IsConnected = wifiAdapter.NetworkAdapter.NetworkItem != null &&
                                  wifiAdapter.NetworkAdapter.NetworkItem.GetNetworkTypes().HasFlag(Windows.Networking.Connectivity.NetworkTypes.Wireless) &&
                                  wifiAdapter.NetworkAdapter.NetworkItem.Name == network.Ssid
                }).ToList();

            WifiListView.ItemsSource = availableNetworks;

            AutoConnectToSavedNetwork(availableNetworks);
        }

        private async void AutoConnectToSavedNetwork(List<WiFiNetwork> availableNetworks)
        {
            foreach (var network in availableNetworks)
            {
                if (savedNetworks.Contains(network.Ssid))
                {
                    var credential = new PasswordCredential();
                    credential.Password = await GetSavedPasswordAsync(network.Ssid);

                    if (!string.IsNullOrEmpty(credential.Password))
                    {
                        await wifiAdapter.ConnectAsync(network.Network, WiFiReconnectionKind.Automatic, credential);
                        ShowStatusMessage($"✅ Auto-connected to {network.Ssid}", true);
                        return;
                    }
                }
            }
        }

        private async void ConnectToNetwork(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedNetwork = button.DataContext as WiFiNetwork;

            if (selectedNetwork == null) return;

            var password = await ShowPasswordPromptAsync(selectedNetwork.Ssid);
            if (password == null) return;

            var credential = new PasswordCredential { Password = password };

            var result = await wifiAdapter.ConnectAsync(selectedNetwork.Network, WiFiReconnectionKind.Automatic, credential);

            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                ShowStatusMessage($"✅ Connected to {selectedNetwork.Ssid}", true);
                SaveNetworkCredentials(selectedNetwork.Ssid, password);
            }
            else
            {
                ShowStatusMessage($"❌ Failed to connect to {selectedNetwork.Ssid}", false);
            }
        }

        private async void ConnectToOtherNetwork(object sender, RoutedEventArgs e)
        {
            var (ssid, password) = await ShowOtherNetworkDialogAsync();
            if (string.IsNullOrEmpty(ssid) || string.IsNullOrEmpty(password)) return;

            var credential = new PasswordCredential { Password = password };

            var network = new WiFiAvailableNetwork { Ssid = ssid };

            var result = await wifiAdapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, credential);

            if (result.ConnectionStatus == WiFiConnectionStatus.Success)
            {
                ShowStatusMessage($"✅ Connected to {ssid}", true);
                SaveNetworkCredentials(ssid, password);
            }
            else
            {
                ShowStatusMessage($"❌ Failed to connect to {ssid}", false);
            }
        }

        private async void ForgetNetwork(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedNetwork = button.DataContext as WiFiNetwork;

            if (selectedNetwork == null) return;

            savedNetworks.Remove(selectedNetwork.Ssid);
            await SaveNetworksAsync();

            ShowStatusMessage($"🗑️ Forgot {selectedNetwork.Ssid}", true);
            LoadWiFiNetworks();
        }

        private string GetSignalEmoji(byte signalBars)
        {
            return signalBars switch
            {
                4 => "📶📶📶📶",
                3 => "📶📶📶",
                2 => "📶📶",
                1 => "📶",
                _ => "❌",
            };
        }

        private void ShowStatusMessage(string message, bool success)
        {
            StatusText.Text = message;
            StatusText.Foreground = success ? new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green)
                                             : new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            StatusText.Visibility = Visibility.Visible;
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

        private async void LoadSavedNetworks()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            savedNetworks = (localSettings.Values["SavedNetworks"] as string)?.Split(';').ToList() ?? new List<string>();
        }

        private async Task SaveNetworksAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["SavedNetworks"] = string.Join(";", savedNetworks);
        }

        private void SaveNetworkCredentials(string ssid, string password)
        {
            savedNetworks.Add(ssid);
            SaveNetworksAsync();
        }
    }

    public class WiFiNetwork
    {
        public string Ssid { get; set; }
        public WiFiAvailableNetwork Network { get; set; }
        public string SignalEmoji { get; set; }
        public bool IsConnected { get; set; }
    }
}
