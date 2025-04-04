using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json; // Used for SSH config deserialization

namespace ShadowPaws
{
    public partial class MainWindow : Window
    {
        // SSH settings (modifiable via SettingsWindow)
        public static string sshHost = "127.0.0.1";
        public static int sshPort = 22;
        public static string sshUsername = "username";
        public static string sshPassword = "password";

        // File to store macros.
        private const string MacrosFile = "macros.json";

        // List of macros.
        public List<Macro> Macros { get; set; } = new List<Macro>();

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // Load SSH configuration from config file.
            LoadSshConfig();

            // Load macros and render them.
            LoadMacros();
            RenderMacroButtons();
        }

        private async Task<string> ExecuteSshCommandAsync(string commandText, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                string output = "";
                try
                {
                    using (var client = new SshClient(sshHost, sshPort, sshUsername, sshPassword))
                    {
                        client.Connect();
                        var result = client.RunCommand(commandText);
                        output = result.Result;
                        client.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    output = "Error: " + ex.Message;
                }
                return output;
            }, cancellationToken);
        }

        // Execute a custom command from the textbox.
        private async void ExecuteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            var commandText = this.FindControl<TextBox>("CommandTextBox")?.Text;
            var executeButton = sender as Button;
            if (executeButton != null)
                executeButton.IsEnabled = false;

            // Show a busy indicator by updating the output textbox.
            var outputTextBox = this.FindControl<TextBox>("OutputTextBox");
            if (outputTextBox != null)
                outputTextBox.Text = "Executing command...";

            try
            {
                var cts = new CancellationTokenSource();
                string output = await ExecuteSshCommandAsync(commandText, cts.Token);
                if (outputTextBox != null)
                    outputTextBox.Text = output;
            }
            catch (OperationCanceledException)
            {
                if (outputTextBox != null)
                    outputTextBox.Text = "Command execution was canceled.";
            }
            finally
            {
                if (executeButton != null)
                    executeButton.IsEnabled = true;
            }
        }

        // Execute a macro command.
        private void MacroButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string macroCommand)
            {
                ExecuteSshCommand(macroCommand);
            }
        }

        private void ExecuteSshCommand(string commandText)
        {
            Task.Run(() =>
            {
                string output = "";
                try
                {
                    using (var client = new SshClient(sshHost, sshPort, sshUsername, sshPassword))
                    {
                        client.Connect();
                        var result = client.RunCommand(commandText);
                        output = result.Result;
                        client.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    output = "Error: " + ex.Message;
                }
                Dispatcher.UIThread.Post(() =>
                {
                    var outputTextBox = this.FindControl<TextBox>("OutputTextBox");
                    if (outputTextBox != null)
                        outputTextBox.Text = output;
                });
            });
        }

        // Open the Settings window.
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog(this);
        }

        // Open the Macro Management window.
        private void ManageMacrosButton_Click(object sender, RoutedEventArgs e)
        {
            var macroWindow = new MacroManagementWindow(Macros);
            macroWindow.OnMacrosUpdated += (updatedMacros) =>
            {
                Macros = updatedMacros;
                SaveMacros();
                RenderMacroButtons();
            };
            macroWindow.ShowDialog(this);
        }

        // Load macros from a JSON file using System.Text.Json.
        private void LoadMacros()
        {
            try
            {
                if (File.Exists(MacrosFile))
                {
                    var json = File.ReadAllText(MacrosFile);
                    Macros = System.Text.Json.JsonSerializer.Deserialize<List<Macro>>(json) ?? new List<Macro>();
                }
            }
            catch (Exception)
            {
                // Optionally log error.
                Macros = new List<Macro>();
            }
        }

        // Save macros to a JSON file using System.Text.Json.
        private void SaveMacros()
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(Macros, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(MacrosFile, json);
            }
            catch (Exception)
            {
                // Optionally log error.
            }
        }

        // Create buttons dynamically for each macro.
        private void RenderMacroButtons()
        {
            var macrosPanel = this.FindControl<StackPanel>("MacrosPanel");
            if (macrosPanel != null)
            {
                macrosPanel.Children.Clear();
                foreach (var macro in Macros)
                {
                    var button = new Button
                    {
                        Content = macro.Name,
                        Tag = macro.Command,
                        Margin = new Avalonia.Thickness(5)
                    };
                    button.Click += MacroButton_Click;
                    macrosPanel.Children.Add(button);
                }
            }
        }

        // Load the SSH configuration from config.json.
        private void LoadSshConfig()
        {
            const string ConfigFilePath = "config.json";
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonConvert.DeserializeObject<SshConfig>(json);
                    if (config != null)
                    {
                        sshHost = config.Host;
                        sshPort = config.Port;
                        sshUsername = config.Username;
                        sshPassword = config.Password;
                    }
                }
                catch (Exception ex)
                {
                    // Optionally log error.
                    Console.Error.WriteLine($"Error loading SSH config: {ex.Message}");
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
