using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming

namespace App_Updater_Pro
{
    public partial class MainWindow
    {
        private readonly string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateLog.txt");
        private readonly DispatcherTimer autoRefreshTimer;

        public MainWindow()
        {
            InitializeComponent();

            autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };

            autoRefreshTimer.Tick += (_, _) => ShowUpdates_Click(this, new RoutedEventArgs());
        }

        private async void ShowInstalled_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await RunWingetCommandAsync("list", "Installierte Programme");
            }
            catch (Exception ex)
            {
                OutputBox.AppendText($"\n❌ Fehler: {ex.Message}");
                StatusText.Text = "❌ Fehler beim Ausführen";
            }
        }

        private async void ShowUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await RunWingetCommandAsync("upgrade", "Verfügbare Updates");
            }
            catch (Exception ex)
            {
                OutputBox.AppendText($"\n❌ Fehler: {ex.Message}");
                StatusText.Text = "❌ Fehler beim Ausführen";
            }
        }

        private async void UpdateAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Alle Programme aktualisieren?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    await RunWingetCommandAsync("upgrade --all --accept-source-agreements --accept-package-agreements", "Alle Updates");
            }
            catch (Exception ex)
            {
                OutputBox.AppendText($"\n❌ Fehler: {ex.Message}");
                StatusText.Text = "❌ Fehler beim Ausführen";
            }
        }

        private async void UpdateFiltered_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filter = FilterBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(filter))
                {
                    MessageBox.Show("Bitte zuerst einen Filter eingeben (z. B. Programmname).");
                    return;
                }

                await RunWingetCommandAsync(
                    $"upgrade --query \"{filter}\" --accept-source-agreements --accept-package-agreements",
                    $"Gefilterte Updates ({filter})");
            }
            catch (Exception ex)
            {
                OutputBox.AppendText($"\n❌ Fehler: {ex.Message}");
                StatusText.Text = "❌ Fehler beim Ausführen";
            }
        }

        private async Task RunWingetCommandAsync(string args, string actionName)
        {
            try
            {
                StatusText.Text = $"{actionName} läuft...";
                Progress.Value = 0;
                OutputBox.Clear();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    },
                    EnableRaisingEvents = true
                };

                process.OutputDataReceived += (_, dataArgs) =>
                {
                    if (!string.IsNullOrEmpty(dataArgs.Data))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OutputBox.AppendText(dataArgs.Data + Environment.NewLine);
                            OutputBox.ScrollToEnd();
                            Progress.Value = Math.Min(100, Progress.Value + 0.5);
                        });
                    }
                };

                process.ErrorDataReceived += (_, dataArgs) =>
                {
                    if (!string.IsNullOrEmpty(dataArgs.Data))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OutputBox.AppendText($"⚠ {dataArgs.Data}\n");
                            OutputBox.ScrollToEnd();
                        });
                    }
                };

                process.Exited += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Progress.Value = 100;
                        StatusText.Text = $"✅ {actionName} abgeschlossen ({DateTime.Now:T})";
                        LogToFile(actionName, OutputBox.Text, string.Empty);
                    });
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());
            }
            catch (Exception ex)
            {
                OutputBox.AppendText($"\n❌ Fehler: {ex.Message}");
                StatusText.Text = "❌ Fehler beim Ausführen";
            }
        }

        private void LogToFile(string action, string output, string error)
        {
            string logEntry = $"\n=========================\n" +
                              $"🕒 {DateTime.Now}\n" +
                              $"Aktion: {action}\n" +
                              $"Ergebnis:\n{output}\n" +
                              $"{(string.IsNullOrEmpty(error) ? "" : "Fehler:\n" + error)}" +
                              $"=========================\n";

            File.AppendAllText(logFile, logEntry, Encoding.UTF8);
        }

        private void AutoRefreshBox_Checked(object sender, RoutedEventArgs e)
        {
            autoRefreshTimer.Start();
            StatusText.Text = "🔄 Auto-Refresh aktiviert";
        }

        private void AutoRefreshBox_Unchecked(object sender, RoutedEventArgs e)
        {
            autoRefreshTimer.Stop();
            StatusText.Text = "⏹️ Auto-Refresh deaktiviert";
        }

        private void ShowCommands_Click(object sender, RoutedEventArgs e)
        {
            string helpText =
                "📘 WINGET KOMPAKT\n\n" +
                "list                   → installierte Programme anzeigen\n" +
                "search <n>             → Programme suchen\n" +
                "install <n>            → Programm installieren\n" +
                "upgrade                → Updates anzeigen\n" +
                "upgrade --all          → alle Updates installieren\n" +
                "upgrade --query <n>    → nur bestimmte Programme updaten\n" +
                "uninstall <n>          → Programm deinstallieren\n" +
                "--help                 → komplette Hilfe im Terminal\n";

            MessageBox.Show(helpText, "Winget Befehle", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                File.WriteAllText(fileName, OutputBox.Text, Encoding.UTF8);
                MessageBox.Show($"Exportiert nach:\n{fileName}", "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Export: {ex.Message}");
            }
        }
    }
}