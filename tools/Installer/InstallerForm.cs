using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

public class InstallerForm : Form
{
    private Panel page1, page2, page3;
    private CheckBox cbShortcut;
    private Button btnNext, btnInstall;
    private ProgressBar progressBar;
    private Label lblStatus;

    public InstallerForm()
    {
        Text = "osu-splitter Installer";
        Width = 500; Height = 300;
        StartPosition = FormStartPosition.CenterScreen;

        page1 = new Panel() { Dock = DockStyle.Fill };
        page2 = new Panel() { Dock = DockStyle.Fill, Visible = false };
        page3 = new Panel() { Dock = DockStyle.Fill, Visible = false };

        cbShortcut = new CheckBox() { Text = "Create desktop shortcut", Left = 20, Top = 20, Width = 300 };
        btnNext = new Button() { Text = "Next", Left = 380, Top = 220, Width = 80 };
        btnNext.Click += (s, e) => ShowPage(2);

        page1.Controls.Add(cbShortcut);
        page1.Controls.Add(btnNext);

        var lbl2 = new Label() { Text = "Ready to install. Click Install to continue.", Left = 20, Top = 20, Width = 420 };
        btnInstall = new Button() { Text = "Install", Left = 380, Top = 220, Width = 80 };
        btnInstall.Click += async (s, e) => await StartInstallAsync();
        page2.Controls.Add(lbl2);
        page2.Controls.Add(btnInstall);

        progressBar = new ProgressBar() { Left = 20, Top = 60, Width = 440, Height = 24 };
        lblStatus = new Label() { Left = 20, Top = 20, Width = 440, Text = "Installing..." };
        page3.Controls.Add(lblStatus);
        page3.Controls.Add(progressBar);

        Controls.Add(page1);
        Controls.Add(page2);
        Controls.Add(page3);
    }

    private void ShowPage(int p)
    {
        page1.Visible = p == 1;
        page2.Visible = p == 2;
        page3.Visible = p == 3;
    }

    private async Task StartInstallAsync()
    {
        ShowPage(3);
        try
        {
            string exeFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "publish", "win-x64"));
            string exeName = "osu-splitter.exe";
            string sourceExe = Path.Combine(exeFolder, exeName);
            if (!File.Exists(sourceExe))
            {
                MessageBox.Show($"Cannot find {sourceExe}. Make sure you published the application to tools\\Installer\\..\\publish\\win-x64.");
                ShowPage(2);
                return;
            }

            string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu-splitter");
            Directory.CreateDirectory(installDir);

            string targetExe = Path.Combine(installDir, exeName);

            // Copy with progress
            await Task.Run(() =>
            {
                const int bufferSize = 81920;
                using (var src = File.OpenRead(sourceExe))
                using (var dst = File.Create(targetExe))
                {
                    long total = src.Length;
                    long copied = 0;
                    var buffer = new byte[bufferSize];
                    int read;
                    while ((read = src.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        dst.Write(buffer, 0, read);
                        copied += read;
                        int percent = (int)((copied * 100) / total);
                        Invoke((Action)(() => progressBar.Value = percent));
                    }
                }
            });

            lblStatus.Text = "Installed.";

            // Create desktop shortcut if requested
            if (cbShortcut.Checked)
            {
                try
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string lnkPath = Path.Combine(desktop, "osu-splitter.lnk");

                    // Use PowerShell to create shortcut (avoids COMReference in project)
                    string ps = $"$W = New-Object -ComObject WScript.Shell; $S = $W.CreateShortcut('{lnkPath}'); $S.TargetPath='{targetExe}'; $S.WorkingDirectory='{installDir}'; $S.Save();";
                    var psi = new ProcessStartInfo("powershell", $"-NoProfile -NonInteractive -Command \"{ps}\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi)?.WaitForExit(2000);
                }
                catch { }
            }

            // Launch app
            try
            {
                Process.Start(new ProcessStartInfo { FileName = targetExe, UseShellExecute = true });
            }
            catch { }

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Installation failed: {ex.Message}");
            ShowPage(2);
        }
    }
}
