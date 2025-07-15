using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Harder_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private bool isMaximized;
        private Dictionary<string, bool> toggleStates;
        private Dictionary<string, Dictionary<string, bool>> detailedToggleStates;
        private const string BAT_FOLDER = "bat";
        private double normalWidth = 1200;
        private double normalHeight = 650;
        private string currentDetailCategory = "";
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            isMaximized = false;
            toggleStates = new Dictionary<string, bool>
            {
                { "Local", false },
                { "Domain", false },
                { "Password", false },
                { "Storage", false }
            };
            
            // Initialize detailed toggle states for individual tools (all disabled by default)
            detailedToggleStates = new Dictionary<string, Dictionary<string, bool>>
            {
                { "Local", new Dictionary<string, bool>
                    {
                        { "Aktifkan Windows Firewall", false },
                        { "Nonaktifkan shutdown tanpa login", false },
                        { "Paksakan keamanan ekstensi shell", false },
                        { "Aktifkan desktop aman", false },
                        { "Atur ulang percobaan autentikasi (3x)", false },
                        { "Nonaktifkan properti My Computer", false },
                        { "Nonaktifkan kustomisasi toolbar", false },
                        { "Nonaktifkan layanan messenger", false },
                        { "Sembunyikan nama pengguna terakhir", false },
                        { "Nonaktifkan opsi folder", false }
                    }
                },
                { "Domain", new Dictionary<string, bool>
                    {
                        { "Nonaktifkan inklusi akses anonim", false },
                        { "Cegah auto-run pada drive", false },
                        { "Nonaktifkan pengalihan CD-ROM", false },
                        { "Alokasikan floppy drive hanya untuk pengguna", false },
                        { "Paksa penggunaan profil lokal", false },
                        { "Batasi jumlah login cache (1)", false },
                        { "Nonaktifkan bypass Ctrl+Alt+Del", false },
                        { "Satu sesi per pengguna", false }
                    }
                },
                { "Password", new Dictionary<string, bool>
                    {
                        { "Atur panjang password minimum (8)", false },
                        { "Jalankan skrip validasi password", false },
                        { "Jalankan deteksi password kosong", false },
                        { "Jalankan pemeriksaan password VNC", false }
                    }
                },
                { "Storage", new Dictionary<string, bool>
                    {
                        { "Konversi drive C: ke NTFS", false },
                        { "Hapus flag NoSecurity", false },
                        { "Aktifkan fitur keamanan lanjutan", false }
                    }
                }
            };

            // Auto-detect current tool status on startup
            this.Loaded += (s, e) => 
            {
                // Show loading message
                MyTextBlock.Text = "🔄 Memuat status keamanan sistem...\nMohon tunggu sebentar.";
                MyTextBlock.Visibility = System.Windows.Visibility.Visible;
                
                DetectCurrentToolStatus();
            };
        }
        #endregion

        #region Window Management
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Disabled double-click maximize functionality
            // Users should use the maximize button instead
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMaximized)
            {
                this.WindowState = WindowState.Normal;
                this.Width = normalWidth;
                this.Height = normalHeight;
                this.Left = (SystemParameters.PrimaryScreenWidth - normalWidth) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - normalHeight) / 2;
                isMaximized = false;
            }
            else
            {
                // Store current size before maximizing
                if (this.WindowState == WindowState.Normal)
                {
                    normalWidth = this.Width;
                    normalHeight = this.Height;
                }
                this.WindowState = WindowState.Maximized;
                isMaximized = true;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Script Execution
        private Dictionary<string, Dictionary<string, string>> toolCommands = new Dictionary<string, Dictionary<string, string>>
        {
            { "Local", new Dictionary<string, string>
                {
                    { "Aktifkan Windows Firewall", "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\" /v EnableFirewall /t REG_DWORD /d 1 /f" },
                    { "Nonaktifkan shutdown tanpa login", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\policies\\system\" /v ShutdownWithoutLogon /t REG_DWORD /d 0 /f" },
                    { "Paksakan keamanan ekstensi shell", "reg add \"HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v EnforceShellExtensionSecurity /t REG_DWORD /d 1 /f" },
                    { "Aktifkan desktop aman", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Windows\" /v SecureDesktop /t REG_DWORD /d 1 /f" },
                    { "Atur ulang percobaan autentikasi (3x)", "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\RemoteAccess\\Parameters\" /v AuthenticateRetries /t REG_DWORD /d 3 /f" },
                    { "Nonaktifkan properti My Computer", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoPropertiesMyComputer /t REG_DWORD /d 1 /f" },
                    { "Nonaktifkan kustomisasi toolbar", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoBandCustomize /t REG_DWORD /d 1 /f & reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoToolbarCustomize /t REG_DWORD /d 1 /f" },
                    { "Nonaktifkan layanan messenger", "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\Messenger\" /v Start /t REG_DWORD /d 4 /f" },
                    { "Sembunyikan nama pengguna terakhir", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" /v DontDisplayLastUserName /t REG_DWORD /d 1 /f" },
                    { "Nonaktifkan opsi folder", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoFolderOptions /t REG_DWORD /d 1 /f & reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoFileMenu /t REG_DWORD /d 1 /f" }
                }
            },
            { "Domain", new Dictionary<string, string>
                {
                    { "Nonaktifkan inklusi akses anonim", "reg add \"HKLM\\System\\CurrentControlSet\\Control\\Lsa\" /v \"EveryoneIncludesAnonymous\" /t REG_DWORD /d 2 /f" },
                    { "Cegah auto-run pada drive", "reg add \"HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v \"NoDriveTypeAutoRun\" /t REG_DWORD /d 2 /f" },
                    { "Nonaktifkan pengalihan CD-ROM", "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services\" /v \"DisableCdm\" /t REG_DWORD /d 1 /f & reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v \"AllocateCDRoms\" /t REG_DWORD /d 1 /f" },
                    { "Alokasikan floppy drive hanya untuk pengguna", "reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v \"AllocateFloppies\" /t REG_DWORD /d 1 /f" },
                    { "Paksa penggunaan profil lokal", "reg add \"HKLM\\Software\\Policies\\Microsoft\\Windows\\System\" /v \"LocalProfile\" /t REG_DWORD /d 2 /f" },
                    { "Batasi jumlah login cache (1)", "reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v \"CachedLogonsCount\" /t REG_DWORD /d 1 /f" },
                    { "Nonaktifkan bypass Ctrl+Alt+Del", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Winlogon\" /v \"DisableCAD\" /t REG_DWORD /d 0 /f" },
                    { "Satu sesi per pengguna", "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services\" /v \"SingleSessionPerUser\" /t REG_DWORD /d 2 /f" }
                }
            },
            { "Password", new Dictionary<string, string>
                {
                    { "Atur panjang password minimum (8)", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Network\" /v MinPwdLen /t REG_DWORD /d 8 /f" },
                    { "Jalankan skrip validasi password", "cscript //nologo reg_minPasswdLength.vbs" },
                    { "Jalankan deteksi password kosong", "cscript //nologo blank_password.vbs" },
                    { "Jalankan pemeriksaan password VNC", "python vnc-nopasswd.py" }
                }
            },
            { "Storage", new Dictionary<string, string>
                {
                    { "Konversi drive C: ke NTFS", "convert C: /FS:NTFS" },
                    { "Hapus flag NoSecurity", "convert C: /FS:NTFS /NoSecurity" },
                    { "Aktifkan fitur keamanan lanjutan", "convert C: /FS:NTFS" }
                }
            }
        };

        // Disable commands (undo)
        private Dictionary<string, Dictionary<string, string>> toolDisableCommands = new Dictionary<string, Dictionary<string, string>>
        {
            { "Local", new Dictionary<string, string>
                {
                    { "Aktifkan Windows Firewall", "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile\" /v EnableFirewall /t REG_DWORD /d 0 /f" },
                    { "Nonaktifkan shutdown tanpa login", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\policies\\system\" /v ShutdownWithoutLogon /t REG_DWORD /d 1 /f" },
                    { "Paksakan keamanan ekstensi shell", "reg add \"HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v EnforceShellExtensionSecurity /t REG_DWORD /d 0 /f" },
                    { "Aktifkan desktop aman", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Windows\" /v SecureDesktop /t REG_DWORD /d 0 /f" },
                    { "Atur ulang percobaan autentikasi (3x)", "reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\RemoteAccess\\Parameters\" /v AuthenticateRetries /f" },
                    { "Nonaktifkan properti My Computer", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoPropertiesMyComputer /t REG_DWORD /d 0 /f" },
                    { "Nonaktifkan kustomisasi toolbar", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoBandCustomize /t REG_DWORD /d 0 /f & reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoToolbarCustomize /t REG_DWORD /d 0 /f" },
                    { "Nonaktifkan layanan messenger", "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\Messenger\" /v Start /t REG_DWORD /d 3 /f" },
                    { "Sembunyikan nama pengguna terakhir", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" /v DontDisplayLastUserName /t REG_DWORD /d 0 /f" },
                    { "Nonaktifkan opsi folder", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoFolderOptions /t REG_DWORD /d 0 /f & reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v NoFileMenu /t REG_DWORD /d 0 /f" }
                }
            },
            { "Domain", new Dictionary<string, string>
                {
                    { "Nonaktifkan inklusi akses anonim", "reg add \"HKLM\\System\\CurrentControlSet\\Control\\Lsa\" /v \"EveryoneIncludesAnonymous\" /t REG_DWORD /d 1 /f" },
                    { "Cegah auto-run pada drive", "reg add \"HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v \"NoDriveTypeAutoRun\" /t REG_DWORD /d 95 /f" },
                    { "Nonaktifkan pengalihan CD-ROM", "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services\" /v \"DisableCdm\" /t REG_DWORD /d 0 /f & reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v \"AllocateCDRoms\" /t REG_DWORD /d 0 /f" },
                    { "Alokasikan floppy drive hanya untuk pengguna", "reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v \"AllocateFloppies\" /t REG_DWORD /d 0 /f" },
                    { "Paksa penggunaan profil lokal", "reg add \"HKLM\\Software\\Policies\\Microsoft\\Windows\\System\" /v \"LocalProfile\" /t REG_DWORD /d 0 /f" },
                    { "Batasi jumlah login cache (1)", "reg add \"HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\" /v \"CachedLogonsCount\" /t REG_DWORD /d 10 /f" },
                    { "Nonaktifkan bypass Ctrl+Alt+Del", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Winlogon\" /v \"DisableCAD\" /t REG_DWORD /d 1 /f" },
                    { "Satu sesi per pengguna", "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services\" /v \"SingleSessionPerUser\" /t REG_DWORD /d 1 /f" }
                }
            },
            { "Password", new Dictionary<string, string>
                {
                    { "Atur panjang password minimum (8)", "reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Network\" /v MinPwdLen /t REG_DWORD /d 0 /f" },
                    { "Jalankan skrip validasi password", "echo Skrip validasi password dinonaktifkan" },
                    { "Jalankan deteksi password kosong", "echo Deteksi password kosong dinonaktifkan" },
                    { "Jalankan pemeriksaan password VNC", "echo Pemeriksaan VNC dinonaktifkan" }
                }
            },
            { "Storage", new Dictionary<string, string>
                {
                    { "Konversi drive C: ke NTFS", "echo Konversi NTFS tidak dapat dibatalkan" },
                    { "Hapus flag NoSecurity", "echo Flag NoSecurity tidak dapat dibatalkan" },
                    { "Aktifkan fitur keamanan lanjutan", "echo Fitur keamanan NTFS tidak dapat dibatalkan" }
                }
            }
        };

        // Registry paths for detection
        private Dictionary<string, Dictionary<string, (string path, string value, string expectedValue)>> toolRegistryDetection = new Dictionary<string, Dictionary<string, (string, string, string)>>
        {
            { "Local", new Dictionary<string, (string, string, string)>
                {
                    { "Aktifkan Windows Firewall", ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\StandardProfile", "EnableFirewall", "1") },
                    { "Nonaktifkan shutdown tanpa login", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "ShutdownWithoutLogon", "0") },
                    { "Paksakan keamanan ekstensi shell", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "EnforceShellExtensionSecurity", "1") },
                    { "Aktifkan desktop aman", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Windows", "SecureDesktop", "1") },
                    { "Atur ulang percobaan autentikasi (3x)", ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\RemoteAccess\\Parameters", "AuthenticateRetries", "3") },
                    { "Nonaktifkan properti My Computer", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "NoPropertiesMyComputer", "1") },
                    { "Nonaktifkan kustomisasi toolbar", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "NoBandCustomize", "1") },
                    { "Nonaktifkan layanan messenger", ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\Messenger", "Start", "4") },
                    { "Sembunyikan nama pengguna terakhir", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "DontDisplayLastUserName", "1") },
                    { "Nonaktifkan opsi folder", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "NoFolderOptions", "1") }
                }
            },
            { "Domain", new Dictionary<string, (string, string, string)>
                {
                    { "Nonaktifkan inklusi akses anonim", ("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", "EveryoneIncludesAnonymous", "0") },
                    { "Cegah auto-run pada drive", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", "NoDriveTypeAutoRun", "255") },
                    { "Nonaktifkan pengalihan CD-ROM", ("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services", "DisableCdm", "1") },
                    { "Alokasikan floppy drive hanya untuk pengguna", ("HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "AllocateFloppies", "1") },
                    { "Paksa penggunaan profil lokal", ("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System", "LocalProfile", "1") },
                    { "Batasi jumlah login cache (1)", ("HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "CachedLogonsCount", "1") },
                    { "Nonaktifkan bypass Ctrl+Alt+Del", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Winlogon", "DisableCAD", "0") },
                    { "Satu sesi per pengguna", ("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services", "SingleSessionPerUser", "1") }
                }
            },
            { "Password", new Dictionary<string, (string, string, string)>
                {
                    { "Atur panjang password minimum (8)", ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Network", "MinPwdLen", "8") },
                    { "Jalankan skrip validasi password", ("", "", "") }, // Scripts can't be detected easily
                    { "Jalankan deteksi password kosong", ("", "", "") },
                    { "Jalankan pemeriksaan password VNC", ("", "", "") }
                }
            },
            { "Storage", new Dictionary<string, (string, string, string)>
                {
                    { "Konversi drive C: ke NTFS", ("", "", "") }, // File system type detection requires different approach
                    { "Hapus flag NoSecurity", ("", "", "") },
                    { "Aktifkan fitur keamanan lanjutan", ("", "", "") }
                }
            }
        };

        private void ExecuteSelectedTools()
        {
            try
            {
                // Create a temporary batch file with only selected commands
                string tempBatPath = Path.Combine(Path.GetTempPath(), "harder_selected_tools.bat");
                using (StreamWriter writer = new StreamWriter(tempBatPath))
                {
                    writer.WriteLine("@echo off");
                    writer.WriteLine("echo Menjalankan alat keamanan yang dipilih...");
                    writer.WriteLine();

                    foreach (var category in toggleStates)
                    {
                        if (category.Value || detailedToggleStates[category.Key].Any(tool => tool.Value))
                        {
                            writer.WriteLine($"echo === {category.Key.ToUpper()} SECURITY ===");
                            
                            foreach (var tool in detailedToggleStates[category.Key])
                            {
                                if (tool.Value)
                                {
                                    // Check if tool is currently enabled and determine action
                                    bool isCurrentlyEnabled = IsToolEnabled(category.Key, tool.Key);
                                    
                                    if (isCurrentlyEnabled)
                                    {
                                        // Tool is enabled, provide disable option
                                        writer.WriteLine($"echo MENONAKTIFKAN: {tool.Key}");
                                        if (toolDisableCommands[category.Key].ContainsKey(tool.Key))
                                        {
                                            writer.WriteLine(toolDisableCommands[category.Key][tool.Key]);
                                        }
                                    }
                                    else
                                    {
                                        // Tool is disabled, enable it
                                        writer.WriteLine($"echo MENGAKTIFKAN: {tool.Key}");
                                        if (toolCommands[category.Key].ContainsKey(tool.Key))
                                        {
                                            writer.WriteLine(toolCommands[category.Key][tool.Key]);
                                        }
                                    }
                                    writer.WriteLine("if %errorlevel% neq 0 echo PERINGATAN: Error pada perintah di atas");
                                    writer.WriteLine();
                                }
                            }
                            writer.WriteLine();
                        }
                    }
                    
                    writer.WriteLine("echo Selesai!");
                    writer.WriteLine("echo RESTART aplikasi untuk melihat status terbaru.");
                    writer.WriteLine("pause");
                }

                // Execute the temporary batch file
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/c \"{tempBatPath}\"";
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas"; // Run as administrator
                Process.Start(startInfo);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error menjalankan alat keamanan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsToolEnabled(string category, string toolName)
        {
            try
            {
                if (!toolRegistryDetection.ContainsKey(category) || 
                    !toolRegistryDetection[category].ContainsKey(toolName))
                {
                    return false; // Can't detect, assume disabled
                }

                var detection = toolRegistryDetection[category][toolName];
                
                // Skip detection for tools without registry paths (like scripts)
                if (string.IsNullOrEmpty(detection.path))
                {
                    return false;
                }

                try
                {
                    string registryPath = detection.path.Replace("HKLM\\", "");
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
                    {
                        if (key != null)
                        {
                            var value = key.GetValue(detection.value);
                            if (value != null)
                            {
                                string currentValue = value.ToString();
                                return currentValue == detection.expectedValue;
                            }
                        }
                    }
                }
                catch
                {
                    // Registry access failed, assume disabled
                    return false;
                }

                return false; // Default to disabled if can't determine
            }
            catch
            {
                return false; // Default to disabled on any error
            }
        }

        private void DetectCurrentToolStatus()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    foreach (var category in detailedToggleStates.Keys)
                    {
                        foreach (var tool in detailedToggleStates[category].Keys.ToList())
                        {
                            bool isEnabled = IsToolEnabled(category, tool);
                            
                            // Update UI on main thread
                            Dispatcher.Invoke(() =>
                            {
                                detailedToggleStates[category][tool] = isEnabled;
                            });
                        }
                        
                        // Update main category toggle based on individual tools
                        Dispatcher.Invoke(() =>
                        {
                            UpdateCategoryToggleState(category);
                        });
                    }
                    
                    // Clear loading message and refresh display
                    Dispatcher.Invoke(() =>
                    {
                        MyTextBlock.Text = "";
                        MyTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                        
                        if (!string.IsNullOrEmpty(currentDetailCategory))
                        {
                            ShowDetailedToolsList(currentDetailCategory);
                        }
                    });
                }
                catch (System.Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MyTextBlock.Text = $"❌ Error: {ex.Message}";
                        MessageBox.Show($"Error mendeteksi status tools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
            });
        }

        private void ExecuteScript(string scriptName, bool enable)
        {
            string scriptPath = Path.Combine(BAT_FOLDER, scriptName);
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show($"Skrip tidak ditemukan: {scriptPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/c {scriptPath}";
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas"; // Run as administrator
                Process.Start(startInfo);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error menjalankan skrip: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string confirmMessage = "Perubahan keamanan berikut akan diterapkan:\n\n";
            bool hasEnabledTools = false;
            
            foreach (var category in toggleStates)
            {
                if (category.Value) // If category toggle is ON
                {
                    confirmMessage += $"🔧 KEAMANAN {category.Key.ToUpper()}:\n";
                    var tools = detailedToggleStates[category.Key];
                    
                    foreach (var tool in tools)
                    {
                        if (tool.Value) // If individual tool is selected
                        {
                            bool isCurrentlyEnabled = IsToolEnabled(category.Key, tool.Key);
                            string action = isCurrentlyEnabled ? "🔄 NONAKTIFKAN" : "✅ AKTIFKAN";
                            confirmMessage += $"   {action}: {tool.Key}\n";
                            hasEnabledTools = true;
                        }
                    }
                    confirmMessage += "\n";
                }
                else
                {
                    // Check if category toggle is OFF but individual tools are enabled
                    var tools = detailedToggleStates[category.Key];
                    var enabledTools = tools.Where(t => t.Value).ToList();
                    
                    if (enabledTools.Any())
                    {
                        confirmMessage += $"🔧 KEAMANAN {category.Key.ToUpper()}:\n";
                        foreach (var tool in enabledTools)
                        {
                            bool isCurrentlyEnabled = IsToolEnabled(category.Key, tool.Key);
                            string action = isCurrentlyEnabled ? "🔄 NONAKTIFKAN" : "✅ AKTIFKAN";
                            confirmMessage += $"   {action}: {tool.Key}\n";
                            hasEnabledTools = true;
                        }
                        confirmMessage += "\n";
                    }
                }
            }
            
            if (!hasEnabledTools)
            {
                MessageBox.Show("Tidak ada alat keamanan yang dipilih untuk diubah. Silakan pilih beberapa alat terlebih dahulu.", "Tidak Ada Alat Yang Dipilih", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            confirmMessage += "⚠️ Perubahan ini memerlukan hak akses administrator.\n";
            confirmMessage += "💡 Tools yang sudah aktif akan DINONAKTIFKAN, yang nonaktif akan DIAKTIFKAN.\n";
            confirmMessage += "🔄 Restart aplikasi setelah selesai untuk melihat status terbaru.\n";
            confirmMessage += "Apakah Anda ingin melanjutkan?";
            
            var result = MessageBox.Show(confirmMessage, "Konfirmasi Perubahan Keamanan", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // Use the new granular execution method
                ExecuteSelectedTools();
            }
        }
        #endregion

        #region Local Security Handlers
        private void InfoLocalButton_Click(object sender, RoutedEventArgs e)
        {
            string localSecurityInfo = @"🏠 PENGATURAN KEAMANAN LOKAL
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔥 Keamanan Firewall & Jaringan:
   ✓ Aktifkan Windows Firewall
   ✓ Atur ulang percobaan autentikasi (maks 3x)

🖥️ Keamanan Desktop & Interface:
   ✓ Nonaktifkan shutdown tanpa login
   ✓ Aktifkan desktop aman
   ✓ Sembunyikan nama pengguna terakhir saat login
   ✓ Nonaktifkan modifikasi desktop
   ✓ Paksa unlock login

🚫 Pembatasan Sistem:
   ✓ Nonaktifkan properti My Computer
   ✓ Nonaktifkan kustomisasi toolbar
   ✓ Nonaktifkan menu opsi folder
   ✓ Nonaktifkan akses menu file
   ✓ Batasi perubahan Active Desktop

🔧 Manajemen Layanan:
   ✓ Nonaktifkan layanan messenger
   ✓ Paksakan keamanan ekstensi shell

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📁 File: local_set.bat
🎯 Tujuan: Penguatan keamanan lokal menyeluruh
   melalui modifikasi registry";
            
            ToggleTextBlockContent(localSecurityInfo);
        }

        private void ComboLocalButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailedToolsList("Local");
        }

        private void ToggleLocalButton_Checked(object sender, RoutedEventArgs e)
        {
            toggleStates["Local"] = true;
            // Enable all individual Local tools when main toggle is turned ON
            foreach (var tool in detailedToggleStates["Local"].Keys.ToList())
            {
                detailedToggleStates["Local"][tool] = true;
            }
            // Refresh the display if currently viewing Local tools
            if (currentDetailCategory == "Local")
            {
                ShowDetailedToolsList("Local");
            }
        }

        private void ToggleLocalButton_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleStates["Local"] = false;
            // Disable all individual Local tools when main toggle is turned OFF
            foreach (var tool in detailedToggleStates["Local"].Keys.ToList())
            {
                detailedToggleStates["Local"][tool] = false;
            }
            // Refresh the display if currently viewing Local tools
            if (currentDetailCategory == "Local")
            {
                ShowDetailedToolsList("Local");
            }
        }
        #endregion

        #region Domain Security Handlers
        private void InfoDomainButton_Click(object sender, RoutedEventArgs e)
        {
            string domainSecurityInfo = @"🌐 PENGATURAN KEAMANAN DOMAIN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔒 Kontrol Akses:
   ✓ Nonaktifkan inklusi akses anonim
   ✓ Paksa penggunaan profil lokal
   ✓ Batasi jumlah login cache (1 sesi)
   ✓ Kebijakan satu sesi per pengguna

🖱️ Penguatan Terminal Services:
   ✓ Nonaktifkan pengalihan CD-ROM
   ✓ Nonaktifkan pengalihan clipboard
   ✓ Nonaktifkan pengalihan port COM
   ✓ Nonaktifkan pengalihan port LPT

💾 Manajemen Drive:
   ✓ Cegah auto-run pada drive
   ✓ Alokasikan floppy drive hanya untuk pengguna
   ✓ Alokasikan CD-ROM hanya untuk pengguna

⌨️ Kontrol Sistem:
   ✓ Nonaktifkan bypass Ctrl+Alt+Del

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📁 File: domain_set.bat
🎯 Tujuan: Keamanan domain lanjutan dengan
   Terminal Services & pembatasan drive";
            
            ToggleTextBlockContent(domainSecurityInfo);
        }

        private void ComboDomainButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailedToolsList("Domain");
        }

        private void ToggleDomainButton_Checked(object sender, RoutedEventArgs e)
        {
            toggleStates["Domain"] = true;
            // Enable all individual Domain tools when main toggle is turned ON
            foreach (var tool in detailedToggleStates["Domain"].Keys.ToList())
            {
                detailedToggleStates["Domain"][tool] = true;
            }
            // Refresh the display if currently viewing Domain tools
            if (currentDetailCategory == "Domain")
            {
                ShowDetailedToolsList("Domain");
            }
        }

        private void ToggleDomainButton_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleStates["Domain"] = false;
            // Disable all individual Domain tools when main toggle is turned OFF
            foreach (var tool in detailedToggleStates["Domain"].Keys.ToList())
            {
                detailedToggleStates["Domain"][tool] = false;
            }
            // Refresh the display if currently viewing Domain tools
            if (currentDetailCategory == "Domain")
            {
                ShowDetailedToolsList("Domain");
            }
        }
        #endregion

        #region Password Security Handlers
        private void InfoPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string passwordSecurityInfo = @"🔐 PENGATURAN KEAMANAN PASSWORD
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🛡️ Penegakan Kebijakan Password:
   ✓ Panjang password minimum (8 karakter)
   ✓ Validasi kompleksitas password
   ✓ Deteksi & penghapusan password kosong

🔍 Skrip Validasi Keamanan:
   📄 reg_minPasswdLength.vbs
      → Validasi panjang password registry
   📄 blank_password.vbs
      → Mendeteksi & memperbaiki password kosong
   🐍 vnc-nopasswd.py
      → Audit keamanan password VNC

⚠️  Persyaratan:
   🔧 Hak akses administrator diperlukan
   🐍 Runtime Python untuk skrip VNC
   🖥️ Izin akses registry

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📁 File: password_set.bat
🎯 Tujuan: Penegakan keamanan password
   multi-layer di seluruh layanan sistem";
            
            ToggleTextBlockContent(passwordSecurityInfo);
        }

        private void ComboPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailedToolsList("Password");
        }

        private void TogglePasswordButton_Checked(object sender, RoutedEventArgs e)
        {
            toggleStates["Password"] = true;
            // Enable all individual Password tools when main toggle is turned ON
            foreach (var tool in detailedToggleStates["Password"].Keys.ToList())
            {
                detailedToggleStates["Password"][tool] = true;
            }
            // Refresh the display if currently viewing Password tools
            if (currentDetailCategory == "Password")
            {
                ShowDetailedToolsList("Password");
            }
        }

        private void TogglePasswordButton_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleStates["Password"] = false;
            // Disable all individual Password tools when main toggle is turned OFF
            foreach (var tool in detailedToggleStates["Password"].Keys.ToList())
            {
                detailedToggleStates["Password"][tool] = false;
            }
            // Refresh the display if currently viewing Password tools
            if (currentDetailCategory == "Password")
            {
                ShowDetailedToolsList("Password");
            }
        }
        #endregion

        #region Storage Security Handlers
        private void InfoStorageButton_Click(object sender, RoutedEventArgs e)
        {
            string storageSecurityInfo = @"💾 PENGATURAN KEAMANAN PENYIMPANAN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔄 Konversi Sistem File:
   ✓ Konversi drive C: ke format NTFS
   ✓ Hapus flag NoSecurity konversi
   ✓ Aktifkan fitur keamanan lanjutan

🛡️ Manfaat Keamanan yang Ditingkatkan:
   🔐 Izin file & folder (ACLs)
   🔒 Kemampuan enkripsi (EFS)
   📊 Pencatatan audit komprehensif
   💿 Manajemen kuota disk
   📦 Kompresi file yang ditingkatkan
   🔧 Keandalan sistem yang diperkuat

⚠️  PERINGATAN KRITIS:
   🔄 Restart sistem DIPERLUKAN
   💾 Backup data SANGAT disarankan
   ⏱️ Proses mungkin memerlukan waktu lama
   🚫 Proses konversi tidak dapat dibatalkan

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📁 File: storage_set.bat
🎯 Tujuan: Upgrade ke keamanan sistem file
   NTFS tingkat enterprise";
            
            ToggleTextBlockContent(storageSecurityInfo);
        }

        private void ComboStorageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailedToolsList("Storage");
        }

        private void ToggleStorageButton_Checked(object sender, RoutedEventArgs e)
        {
            toggleStates["Storage"] = true;
            // Enable all individual Storage tools when main toggle is turned ON
            foreach (var tool in detailedToggleStates["Storage"].Keys.ToList())
            {
                detailedToggleStates["Storage"][tool] = true;
            }
            // Refresh the display if currently viewing Storage tools
            if (currentDetailCategory == "Storage")
            {
                ShowDetailedToolsList("Storage");
            }
        }

        private void ToggleStorageButton_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleStates["Storage"] = false;
            // Disable all individual Storage tools when main toggle is turned OFF
            foreach (var tool in detailedToggleStates["Storage"].Keys.ToList())
            {
                detailedToggleStates["Storage"][tool] = false;
            }
            // Refresh the display if currently viewing Storage tools
            if (currentDetailCategory == "Storage")
            {
                ShowDetailedToolsList("Storage");
            }
        }
        #endregion

        #region Helper Methods
        private void ToggleTextBlockContent(string content)
        {
            // Clear any buttons from previous tool lists
            MyContentPanel.Children.Clear();
            MyContentPanel.Children.Add(MyTextBlock);
            
            // Show/hide text content
            MyTextBlock.Visibility = System.Windows.Visibility.Visible;
            MyTextBlock.Text = MyTextBlock.Text == content ? string.Empty : content;
        }

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentDetailCategory)) return;
            
            var button = sender as System.Windows.Controls.Button;
            if (button == null) return;
            
            string toolName = button.Tag.ToString();
            
            if (detailedToggleStates[currentDetailCategory].ContainsKey(toolName))
            {
                // Toggle the tool state
                detailedToggleStates[currentDetailCategory][toolName] = !detailedToggleStates[currentDetailCategory][toolName];
                
                // Auto-sync the main category toggle based on individual tool states
                UpdateCategoryToggleState(currentDetailCategory);
                
                // Refresh the display
                ShowDetailedToolsList(currentDetailCategory);
            }
        }
        
        private void UpdateCategoryToggleState(string category)
        {
            // If any individual tool is enabled, enable the main category toggle
            // If all individual tools are disabled, disable the main category toggle
            bool hasEnabledTools = detailedToggleStates[category].Any(tool => tool.Value);
            toggleStates[category] = hasEnabledTools;
        }

        private void ShowDetailedToolsList(string category)
        {
            currentDetailCategory = category;
            var tools = detailedToggleStates[category];
            
            // Clear the panel
            MyContentPanel.Children.Clear();
            
            // Add header text
            var headerText = new System.Windows.Controls.TextBlock
            {
                Text = $"🔧 ALAT KEAMANAN {category.ToUpper()}\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 14,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80)),
                Margin = new System.Windows.Thickness(8, 8, 8, 4),
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            MyContentPanel.Children.Add(headerText);
            
            // Add toggle buttons for each tool
            foreach (var tool in tools)
            {
                bool isCurrentlyEnabled = IsToolEnabled(category, tool.Key);
                string statusIcon = isCurrentlyEnabled ? "✅" : "🔳"; // Green circle for enabled, gray square for disabled
                string actionText = tool.Value ? (isCurrentlyEnabled ? " (AKAN DINONAKTIFKAN)" : " (AKAN DIAKTIFKAN)") : "";
                
                var button = new System.Windows.Controls.Button
                {
                    Content = $"{statusIcon} {tool.Key}{actionText}",
                    FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                    FontSize = 14,
                    FontWeight = System.Windows.FontWeights.Normal,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80)),
                    Background = tool.Value ? 
                        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 74, 144, 226)) : // Blue for selected
                        (isCurrentlyEnabled ? 
                            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 34, 139, 34)) : // Green for currently enabled
                            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 108, 117, 125))), // Gray for disabled
                    BorderThickness = new System.Windows.Thickness(0),
                    Margin = new System.Windows.Thickness(4, 2, 4, 2),
                    Padding = new System.Windows.Thickness(8, 4, 8, 4),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = tool.Key // Store the tool name in the Tag property
                };
                
                // Add click event
                button.Click += ToolButton_Click;
                
                // Add hover effects
                button.MouseEnter += (s, e) => {
                    var btn = s as System.Windows.Controls.Button;
                    var toolName = btn.Tag.ToString();
                    var isSelected = detailedToggleStates[currentDetailCategory][toolName];
                    var isEnabled = IsToolEnabled(currentDetailCategory, toolName);
                    
                    if (isSelected)
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 74, 144, 226));
                    else if (isEnabled)
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 34, 139, 34));
                    else
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 108, 117, 125));
                };
                
                button.MouseLeave += (s, e) => {
                    var btn = s as System.Windows.Controls.Button;
                    var toolName = btn.Tag.ToString();
                    var isSelected = detailedToggleStates[currentDetailCategory][toolName];
                    var isEnabled = IsToolEnabled(currentDetailCategory, toolName);
                    
                    if (isSelected)
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 74, 144, 226));
                    else if (isEnabled)
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 34, 139, 34));
                    else
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 108, 117, 125));
                };
                
                MyContentPanel.Children.Add(button);
            }
            
            // Add footer text
            var footerText = new System.Windows.Controls.TextBlock
            {
                Text = "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n💡 Klik untuk memilih tools yang akan diubah!\n✅ = Sudah Aktif | 🔳 = Nonaktif | 🔲 = Dipilih untuk Diubah",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 14,
                FontStyle = System.Windows.FontStyles.Italic,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125)),
                Margin = new System.Windows.Thickness(8, 4, 8, 8),
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            MyContentPanel.Children.Add(footerText);
            
            // Hide the original TextBlock when showing buttons
            MyTextBlock.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion
    }
}
