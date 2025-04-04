using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShadowPaws
{
    public partial class SettingsWindow : Window
    {
        private const string ConfigFilePath = "config.json";

        public SettingsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // Load config after the window is opened
            this.Opened += async (_, __) => await LoadConfigAsync();
        }

        // Asynchronously load settings from the config file
        private async Task LoadConfigAsync()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(ConfigFilePath);
                    var config = JsonConvert.DeserializeObject<SshConfig>(json);
                    if (config != null)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.FindControl<TextBox>("HostTextBox").Text = config.Host;
                            this.FindControl<TextBox>("PortTextBox").Text = config.Port.ToString();
                            this.FindControl<TextBox>("UsernameTextBox").Text = config.Username;
                            // Use the Password getter which automatically decrypts.
                            this.FindControl<TextBox>("PasswordBox").Text = config.Password;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error loading config: {ex.Message}");
                }
            }
        }

        // Asynchronously save settings to the config file
        private async Task SaveConfigAsync()
        {
            var config = new SshConfig
            {
                Host = this.FindControl<TextBox>("HostTextBox").Text,
                Port = int.TryParse(this.FindControl<TextBox>("PortTextBox").Text, out var p) ? p : 22,
                Username = this.FindControl<TextBox>("UsernameTextBox").Text,
                Password = this.FindControl<TextBox>("PasswordBox").Text  // This uses the setter to encrypt.
            };

            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        // Test connection using the entered settings.
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var host = this.FindControl<TextBox>("HostTextBox").Text;
            var portText = this.FindControl<TextBox>("PortTextBox").Text;
            int port = int.TryParse(portText, out var p) ? p : 22;
            var username = this.FindControl<TextBox>("UsernameTextBox").Text;
            var password = this.FindControl<TextBox>("PasswordBox").Text;

            Task.Run(() =>
            {
                string result = "";
                try
                {
                    using (var client = new SshClient(host, port, username, password))
                    {
                        client.Connect();
                        result = client.IsConnected ? "Connection Successful!" : "Connection Failed!";
                        client.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    result = "Error: " + ex.Message;
                }
                Dispatcher.UIThread.Post(() =>
                {
                    this.FindControl<TextBlock>("TestResultTextBlock").Text = result;
                });
            });
        }

        // Save the settings to config asynchronously and close the window.
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveConfigAsync();
            this.Close();
        }

        // Cancel without saving.
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    // Example SSH configuration class
    public class SshConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 22;
        public string Username { get; set; } = "username";

        // This property holds the encrypted password.
        public string EncryptedPassword { get; set; } = EncryptionHelper.EncryptString("password");

        // Helper to get the decrypted password
        [JsonIgnore]
        public string Password
        {
            get => EncryptionHelper.DecryptString(EncryptedPassword);
            set => EncryptedPassword = EncryptionHelper.EncryptString(value);
        }
    }

}
