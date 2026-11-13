using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NumberConversions.Plugin
{
    public static class PluginUninstaller
    {
        public static void InitiateUninstall(Action? onCloseAction = null)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to uninstall the Tamil Number Conversions plugin from Paratext 9?\n\n" +
                "• Your Bible translation text, project files, and notes will NOT be altered.\n" +
                "• Paratext must close so Windows can release the plugin files.\n\n" +
                "A background uninstallation assistant will wait for Paratext to close, remove the plugin files, and update Paratext's plugin cache.\n\n" +
                "Do you want to proceed with uninstallation?",
                "Confirm Plugin Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                // Determine Paratext installation folder from plugin assembly location
                string assemblyPath = typeof(PluginUninstaller).Assembly.Location;
                string pluginDir = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
                string paratextDir = string.Empty;

                if (!string.IsNullOrEmpty(pluginDir))
                {
                    paratextDir = Path.GetFullPath(Path.Combine(pluginDir, "..", ".."));
                }

                if (string.IsNullOrEmpty(paratextDir) || !File.Exists(Path.Combine(paratextDir, "Paratext.exe")))
                {
                    string[] candidates = { "D:\\Transdata\\Paratext 9", "C:\\Program Files\\Paratext 9", "C:\\Program Files (x86)\\Paratext 9", "D:\\Program Files\\Paratext 9" };
                    foreach (var c in candidates)
                    {
                        if (File.Exists(Path.Combine(c, "Paratext.exe")))
                        {
                            paratextDir = c;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(paratextDir))
                {
                    MessageBox.Show("Could not locate Paratext 9 installation folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Copy uninstaller script to TEMP so it doesn't hold locks on the plugin directory
                string tempScriptPath = Path.Combine(Path.GetTempPath(), "uninstall-paratext-numberconversions.ps1");
                string localScript = Path.Combine(pluginDir, "uninstall-plugin.ps1");

                if (File.Exists(localScript))
                {
                    File.Copy(localScript, tempScriptPath, overwrite: true);
                }
                else
                {
                    string scriptContent = $@"
param()
Set-Location $env:TEMP
$procs = Get-Process -Name 'Paratext*' -ErrorAction SilentlyContinue
if ($procs) {{
    $procs | Wait-Process -Timeout 300 -ErrorAction SilentlyContinue
}}
Start-Sleep -Seconds 2
$target = '{pluginDir.Replace("'", "''")}'
if (Test-Path $target) {{
    Remove-Item -Path $target -Recurse -Force
}}
$updater = Join-Path '{paratextDir.Replace("'", "''")}' 'UpdateParatextPluginCache.exe'
if (Test-Path $updater) {{
    Start-Process -FilePath $updater -Wait -WindowStyle Hidden -ErrorAction SilentlyContinue
}}
Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.MessageBox]::Show(
    'Tamil Number Conversions plugin has been successfully uninstalled from Paratext 9.`n`nYou can now reopen Paratext.',
    'Plugin Uninstalled',
    [System.Windows.Forms.MessageBoxButtons]::OK,
    [System.Windows.Forms.MessageBoxIcon]::Information) | Out-Null
";
                    File.WriteAllText(tempScriptPath, scriptContent);
                }

                // Launch background uninstaller waiting for Paratext exit
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{tempScriptPath}\" -ParatextDir \"{paratextDir}\" -WaitForParatextExit -NotifyWhenDone",
                    WorkingDirectory = Path.GetTempPath(),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(psi);

                MessageBox.Show(
                    "The uninstallation assistant is waiting in the background.\n\n" +
                    "Please close Paratext now so the plugin files can be removed.\n\n" +
                    "You will receive a notification as soon as uninstallation is complete.",
                    "Close Paratext to Finish",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                onCloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initiate uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
