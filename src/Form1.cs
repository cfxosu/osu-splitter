using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;

namespace StructuredOsuMemoryProviderTester
{

    public partial class Form1 : Form
    {
    private System.Drawing.Icon _appIcon;
        private readonly string _osuWindowTitleHint;
        private int _readDelay = 33;

        private readonly StructuredOsuMemoryReader _sreader;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public Form1(string osuWindowTitleHint)
        {
            InitializeComponent();
            // Load application icon from assets/logo.png (PNG) and convert to Icon
            try
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var logoPath = Path.GetFullPath(Path.Combine(exeDir, "..\\..\\..\\assets\\logo.png"));
                if (!File.Exists(logoPath))
                {
                    // fallback to looking relative to the project output
                    logoPath = Path.Combine(exeDir, "logo.png");
                }

                if (File.Exists(logoPath))
                {
                    using var bmp = new Bitmap(logoPath);
                    // Convert PNG to icon by creating an Icon from HICON
                    var hIcon = bmp.GetHicon();
                    _appIcon = System.Drawing.Icon.FromHandle(hIcon);
                    this.Icon = _appIcon;
                }
            }
            catch
            {
                // ignore icon load failures; app will use default icon
            }
            _sreader = StructuredOsuMemoryReader.GetInstance(new("osu!", osuWindowTitleHint));

            // Write a tiny startup debug file so we can see what the published EXE environment looks like
            try
            {
                var startInfo = new
                {
                    BaseDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    EntryAssembly = System.Reflection.Assembly.GetEntryAssembly()?.Location,
                    Is64 = Environment.Is64BitProcess,
                    LogoExists = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png")),
                    CanRead = _sreader?.CanRead ?? false,
                    OsuWindowHint = osuWindowTitleHint
                };

                var startLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_startup.json");
                File.WriteAllText(startLog, JsonConvert.SerializeObject(startInfo, Formatting.Indented));
            }
            catch
            {
                // ignore
            }
            Shown += OnShown;
            Closing += OnClosing;

            // Initialize beatmap splitting controls
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cts.Cancel();
        }

        // Removed read delay controls - using default 33ms delay

        private T ReadProperty<T>(object readObj, string propName, T defaultValue = default) where T : struct
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        private T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        private int ReadInt(object readObj, string propName)
            => ReadProperty<int>(readObj, propName, -5);
        private short ReadShort(object readObj, string propName)
            => ReadProperty<short>(readObj, propName, -5);

        private float ReadFloat(object readObj, string propName)
            => ReadProperty<float>(readObj, propName, -5f);

        private string ReadString(object readObj, string propName)
            => ReadClassProperty<string>(readObj, propName, "INVALID READ");

        private async void OnShown(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_osuWindowTitleHint)) Text += $": {_osuWindowTitleHint}";
            Text += $" ({(Environment.Is64BitProcess ? "x64" : "x86")})";
            _sreader.InvalidRead += SreaderOnInvalidRead;
            await Task.Run(async () =>
            {
                var baseAddresses = new OsuBaseAddresses();
                while (true)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    if (!_sreader.CanRead)
                    {
                        // Backend continues to run even when osu! is not found
                        await Task.Delay(_readDelay);
                        continue;
                    }

                    // Only read beatmap data for backend info
                    _sreader.TryRead(baseAddresses.Beatmap);

                    try
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            // Backend continues to extract beatmap information silently
                            var backendInfo = ExtractBackendBeatmapInfo(baseAddresses.Beatmap);

                            // write a small debug file next to the exe so we can inspect what the published build sees
                            try
                            {
                                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_beatmap.json");
                                File.WriteAllText(logPath, JsonConvert.SerializeObject(backendInfo, Formatting.Indented));
                            }
                            catch
                            {
                                // ignore logging failures
                            }

                            // Auto-update beatmap path when memory data changes
                            UpdateBeatmapPathFromMemory(baseAddresses);
                        }));
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }

                    await Task.Delay(_readDelay);
                }
            }, cts.Token);
        }

        private object ExtractBackendBeatmapInfo(CurrentBeatmap beatmap)
        {
            if (beatmap == null)
                return new { Error = "No beatmap data available" };

            // Parse title and artist from MapString (format: "Artist - Title")
            string artist = "Unknown";
            string title = "Unknown";

            if (!string.IsNullOrEmpty(beatmap.MapString))
            {
                var parts = beatmap.MapString.Split(new[] { " - " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    artist = parts[0].Trim();
                    title = parts[1].Trim();
                }
                else
                {
                    title = beatmap.MapString.Trim();
                }
            }

            // Get difficulty name from .osu file if available
            string difficultyName = "Unknown";
            string osuFilePath = GetBeatmapFilePath(beatmap);
            if (!string.IsNullOrEmpty(osuFilePath) && File.Exists(osuFilePath))
            {
                difficultyName = GetDifficultyNameFromFile(osuFilePath);
            }

            // Get beatmap location
            string beatmapLocation = GetBeatmapDirectory(beatmap);

            return new
            {
                BeatmapTitle = title,
                BeatmapArtist = artist,
                DifficultyName = difficultyName,
                BeatmapLocation = beatmapLocation,
                OsuFileName = beatmap.OsuFileName,
                FullOsuFilePath = osuFilePath
            };
        }

        private string GetBeatmapFilePath(CurrentBeatmap beatmap)
        {
            if (beatmap == null || string.IsNullOrEmpty(beatmap.FolderName) || string.IsNullOrEmpty(beatmap.OsuFileName))
                return null;

            string osuDirectory = GetOsuDirectory();
            if (!string.IsNullOrEmpty(osuDirectory))
            {
                string songsPath = Path.Combine(osuDirectory, "Songs");
                return Path.Combine(songsPath, beatmap.FolderName, beatmap.OsuFileName);
            }

            return null;
        }

        private string GetBeatmapDirectory(CurrentBeatmap beatmap)
        {
            if (beatmap == null || string.IsNullOrEmpty(beatmap.FolderName))
                return null;

            string osuDirectory = GetOsuDirectory();
            if (!string.IsNullOrEmpty(osuDirectory))
            {
                return Path.Combine(osuDirectory, "Songs", beatmap.FolderName);
            }

            return null;
        }

        private string GetDifficultyNameFromFile(string osuFilePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(osuFilePath);
                bool inMetadata = false;

                foreach (string line in lines)
                {
                    if (line.Trim() == "[Metadata]")
                    {
                        inMetadata = true;
                        continue;
                    }

                    if (inMetadata && line.Trim().StartsWith("[") && line.Trim() != "[Metadata]")
                    {
                        break; // End of Metadata section
                    }

                    if (inMetadata && line.StartsWith("Version:"))
                    {
                        return line.Substring(8).Trim(); // Remove "Version:" prefix
                    }
                }
            }
            catch
            {
                // Ignore errors and return default
            }

            return "Unknown";
        }

        private void SreaderOnInvalidRead(object sender, (object readObject, string propPath) e)
        {
            try
            {
                if (InvokeRequired)
                {
                    //Async call to not impact memory read times(too much)
                    BeginInvoke((MethodInvoker)(() => SreaderOnInvalidRead(sender, e)));
                    return;
                }

                // Silently handle invalid reads - no debug logging
            }
            catch (ObjectDisposedException)
            {

            }
        }

        // Removed button_ResetReadTimeMinMax and related functionality

        private void button_RefreshBeatmap_Click(object sender, EventArgs e)
        {
            RefreshBeatmapInfo();
        }



        private void button_SplitBeatmap_Click(object sender, EventArgs e)
        {
            SplitBeatmap();
        }

        private void button_SplitBeatmapBottom_Click(object sender, EventArgs e)
        {
            SplitBeatmap();
        }

        // Hover effect variables
        private bool isHovering = false;
        private Point originalButtonLocation;

        private async void button_SplitBeatmapBottom_MouseEnter(object sender, EventArgs e)
        {
            if (isHovering) return;

            isHovering = true;
            originalButtonLocation = button_SplitBeatmapBottom.Location;

            // Smooth hover up animation
            int hoverDistance = 4; // Move up 2 pixels - subtle hover effect
            int animationSpeed = 5; // milliseconds (ultra-high FPS for silky smooth animation)
            int steps = 8; // Number of steps for smooth animation

            // Move up smoothly
            for (int i = 2; i <= steps; i++)
            {
                int currentY = originalButtonLocation.Y - (hoverDistance * i / steps);
                button_SplitBeatmapBottom.Location = new Point(originalButtonLocation.X, currentY);
                await Task.Delay(animationSpeed);
            }

            // Stay hovered until mouse leaves
        }

        private async void button_SplitBeatmapBottom_MouseLeave(object sender, EventArgs e)
        {
            if (!isHovering) return;

            // Smooth return to original position
            int hoverDistance = 4; // Same distance as hover up
            int animationSpeed = 5; // milliseconds (ultra-high FPS for silky smooth animation)
            int steps = 8; // Number of steps for smooth animation

            Point currentLocation = button_SplitBeatmapBottom.Location;

            // Move down smoothly to original position
            for (int i = steps - 1; i >= 0; i--)
            {
                int currentY = originalButtonLocation.Y - (hoverDistance * i / steps);
                button_SplitBeatmapBottom.Location = new Point(originalButtonLocation.X, currentY);
                await Task.Delay(animationSpeed);
            }

            // Ensure final position is exact
            button_SplitBeatmapBottom.Location = originalButtonLocation;
            isHovering = false;
        }

        // Hover effect for split value label
        private async void label_SplitValue_MouseEnter(object sender, EventArgs e)
        {
            // Move the label up 2 pixels on hover
            label_SplitValue.Location = new Point(label_SplitValue.Location.X, label_SplitValue.Location.Y - 2);
        }

        private async void label_SplitValue_MouseLeave(object sender, EventArgs e)
        {
            // Move the label back to original position
            label_SplitValue.Location = new Point(label_SplitValue.Location.X, label_SplitValue.Location.Y + 2);
        }

        // Custom title bar functionality
        private bool isDragging = false;
        private Point dragStartPoint;

        private void panel_CustomTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void panel_CustomTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - dragStartPoint.X, currentScreenPos.Y - dragStartPoint.Y);
            }
        }

        private void panel_CustomTitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button_Close_MouseEnter(object sender, EventArgs e)
        {
            button_Close.BackColor = Color.FromArgb(232, 17, 35); // Windows red color
            button_Close.ForeColor = Color.White;
        }

        private void button_Close_MouseLeave(object sender, EventArgs e)
        {
            button_Close.BackColor = Color.FromArgb(28, 28, 30);
            button_Close.ForeColor = Color.White;
        }

        private void button_Minimize_MouseEnter(object sender, EventArgs e)
        {
            button_Minimize.BackColor = Color.FromArgb(60, 60, 65);
            button_Minimize.ForeColor = Color.White;
        }

        private void button_Minimize_MouseLeave(object sender, EventArgs e)
        {
            button_Minimize.BackColor = Color.FromArgb(28, 28, 30);
            button_Minimize.ForeColor = Color.White;
        }



        // Rounded window functionality
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Enable layered window for rounded corners and transparency
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set up for rounded window
            this.BackColor = Color.FromArgb(28, 28, 30);
            this.TransparencyKey = Color.FromArgb(1, 1, 1); // Use a color that won't interfere

            // Enable smooth text rendering for the entire form
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            // Make the window rounded with a 30px corner radius
            MakeRoundedWindow(30);

            // Make the split button rounded
            MakeRoundedButton(button_SplitBeatmapBottom, 15);

            // Remove transparency - make window fully opaque
            this.Opacity = 1.0;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Force layered window style
            const int WS_EX_LAYERED = 0x00080000;
            const int LWA_COLORKEY = 0x00000001;
            const int LWA_ALPHA = 0x00000002;

            // Set layered window attributes for proper transparency
            var colorKey = ColorTranslator.ToWin32(this.TransparencyKey);
            SetWindowLong(this.Handle, -20, GetWindowLong(this.Handle, -20) | WS_EX_LAYERED);
            SetWindowLong(this.Handle, -20, GetWindowLong(this.Handle, -20) | LWA_COLORKEY);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private void MakeRoundedWindow(int radius)
        {
            try
            {
                // Ensure minimum size for rounded corners
                if (Width < radius * 2 + 10 || Height < radius * 2 + 10)
                {
                    Region = null;
                    return;
                }

                // Create a rounded rectangle region
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(0, 0, radius * 2, radius * 2, 180, 90); // Top-left corner
                    path.AddArc(Width - radius * 2, 0, radius * 2, radius * 2, 270, 90); // Top-right corner
                    path.AddArc(Width - radius * 2, Height - radius * 2, radius * 2, radius * 2, 0, 90); // Bottom-right corner
                    path.AddArc(0, Height - radius * 2, radius * 2, radius * 2, 90, 90); // Bottom-left corner
                    path.CloseFigure();

                    // Apply the region to the form
                    Region = new Region(path);
                }

                // Force repaint
                Invalidate();
            }
            catch (Exception ex)
            {
                // Fallback: remove rounded corners if there's an error
                Region = null;
                System.Diagnostics.Debug.WriteLine($"Error creating rounded window: {ex.Message}");
            }
        }

        private void MakeRoundedButton(Button button, int radius)
        {
            try
            {
                // Ensure minimum size for rounded corners
                if (button.Width < radius * 2 + 10 || button.Height < radius * 2 + 10)
                {
                    button.Region = null;
                    return;
                }

                // Create a rounded rectangle region for the button
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(0, 0, radius * 2, radius * 2, 180, 90); // Top-left corner
                    path.AddArc(button.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90); // Top-right corner
                    path.AddArc(button.Width - radius * 2, button.Height - radius * 2, radius * 2, radius * 2, 0, 90); // Bottom-right corner
                    path.AddArc(0, button.Height - radius * 2, radius * 2, radius * 2, 90, 90); // Bottom-left corner
                    path.CloseFigure();

                    // Apply the region to the button
                    button.Region = new Region(path);
                }
            }
            catch (Exception ex)
            {
                // Fallback: remove rounded corners if there's an error
                button.Region = null;
                System.Diagnostics.Debug.WriteLine($"Error creating rounded button: {ex.Message}");
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Handle window state changes
            if (WindowState == FormWindowState.Normal)
            {
                // Re-apply rounded corners when returning to normal state
                MakeRoundedWindow(30);
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                // Remove rounded corners when maximized
                Region = null;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // Update rounded corners when size changes
            if (WindowState == FormWindowState.Normal)
            {
                MakeRoundedWindow(30);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Enable smooth text rendering for all background painting
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Call base method to paint the background
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Enable smooth text rendering for all painting operations
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Only draw custom background if we have rounded corners
            if (Region != null && WindowState == FormWindowState.Normal)
            {
                try
                {
                    // Draw the rounded background with a silver metallic border effect
                    using (var brush = new SolidBrush(Color.FromArgb(28, 28, 30)))
                    using (var borderPen = new Pen(Color.FromArgb(192, 192, 192), 3))
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int radius = 30;
                        // Create border path that covers entire window perimeter
                        path.StartFigure();
                        path.AddArc(1, 1, radius * 2, radius * 2, 180, 90); // Top-left corner (inset by 1px)
                        path.AddLine(radius + 1, 1, Width - radius - 1, 1); // Top edge
                        path.AddArc(Width - radius * 2 - 1, 1, radius * 2, radius * 2, 270, 90); // Top-right corner
                        path.AddLine(Width - 1, radius + 1, Width - 1, Height - radius - 1); // Right edge
                        path.AddArc(Width - radius * 2 - 1, Height - radius * 2 - 1, radius * 2, radius * 2, 0, 90); // Bottom-right corner
                        path.AddLine(Width - radius - 1, Height - 1, radius + 1, Height - 1); // Bottom edge
                        path.AddArc(1, Height - radius * 2 - 1, radius * 2, radius * 2, 90, 90); // Bottom-left corner
                        path.AddLine(1, Height - radius - 1, 1, radius + 1); // Left edge
                        path.CloseFigure();

                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        // Fill the background
                        e.Graphics.FillPath(brush, path);

                        // Draw prominent silver border on top of everything
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
                catch (Exception ex)
                {
                    // If painting fails, just use default background
                    System.Diagnostics.Debug.WriteLine($"Error in OnPaint: {ex.Message}");
                    using (var brush = new SolidBrush(Color.FromArgb(28, 28, 30)))
                    {
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                    }
                }
            }
            else
            {
                // Default background for non-rounded states
                using (var brush = new SolidBrush(Color.FromArgb(28, 28, 30)))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }

            // Call base.OnPaint after drawing our background and border
            base.OnPaint(e);
        }

        private void trackBar_SplitPoint_ValueChanged(object sender, EventArgs e)
        {
            label_SplitValue.Text = trackBar_SplitPoint.Value.ToString();
        }

        private void label_SplitValue_Click(object sender, EventArgs e)
        {
            // Show input dialog for user to enter new value
            using (var inputDialog = new Form())
            {
                inputDialog.Text = "Enter Split Value";
                inputDialog.Size = new Size(300, 150);
                inputDialog.StartPosition = FormStartPosition.CenterParent;
                inputDialog.BackColor = Color.FromArgb(28, 28, 30);
                inputDialog.ForeColor = Color.White;
                inputDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputDialog.MaximizeBox = false;
                inputDialog.MinimizeBox = false;

                var label = new Label();
                label.Text = "Enter number of pieces (2-100):";
                label.Location = new Point(20, 20);
                label.Size = new Size(250, 20);
                label.ForeColor = Color.White;
                label.Font = new Font("SF Pro Display", 9F, FontStyle.Regular);

                var textBox = new TextBox();
                textBox.Text = trackBar_SplitPoint.Value.ToString();
                textBox.Location = new Point(20, 45);
                textBox.Size = new Size(100, 25);
                textBox.Font = new Font("SF Pro Display", 10F, FontStyle.Regular);
                textBox.BackColor = Color.FromArgb(45, 45, 48);
                textBox.ForeColor = Color.White;
                textBox.BorderStyle = BorderStyle.FixedSingle;

                var okButton = new Button();
                okButton.Text = "OK";
                okButton.Location = new Point(140, 80);
                okButton.Size = new Size(60, 30);
                okButton.BackColor = Color.FromArgb(10, 132, 255);
                okButton.ForeColor = Color.White;
                okButton.FlatStyle = FlatStyle.Flat;
                okButton.Font = new Font("SF Pro Display", 9F, FontStyle.Bold);
                okButton.DialogResult = DialogResult.OK;

                var cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.Location = new Point(210, 80);
                cancelButton.Size = new Size(60, 30);
                cancelButton.BackColor = Color.FromArgb(45, 45, 48);
                cancelButton.ForeColor = Color.White;
                cancelButton.FlatStyle = FlatStyle.Flat;
                cancelButton.Font = new Font("SF Pro Display", 9F, FontStyle.Regular);
                cancelButton.DialogResult = DialogResult.Cancel;

                inputDialog.Controls.Add(label);
                inputDialog.Controls.Add(textBox);
                inputDialog.Controls.Add(okButton);
                inputDialog.Controls.Add(cancelButton);
                inputDialog.AcceptButton = okButton;
                inputDialog.CancelButton = cancelButton;

                textBox.SelectAll();
                textBox.Focus();

                if (inputDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (int.TryParse(textBox.Text, out int newValue))
                    {
                        if (newValue >= trackBar_SplitPoint.Minimum && newValue <= 100)
                        {
                        // If value is within slider range (2-20), update slider normally
                        if (newValue <= trackBar_SplitPoint.Maximum)
                        {
                            trackBar_SplitPoint.Value = newValue;
                        }
                        else
                        {
                            // If value is above slider max (21-100), handle separately without changing slider value
                            // Don't update trackBar_SplitPoint.Value to prevent ValueChanged event from overriding
                            // Update the display text directly since slider can't go higher
                            label_SplitValue.Text = newValue.ToString();
                        }
                        }
                        else
                        {
                            MessageBox.Show($"Please enter a value between {trackBar_SplitPoint.Minimum} and 100.",
                                          "Invalid Value",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid number.",
                                      "Invalid Input",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                    }
                }
            }
        }



        private void UpdateBeatmapPathFromMemory(OsuBaseAddresses baseAddresses)
        {
            try
            {
                if (baseAddresses?.Beatmap == null)
                    return;

                // Get osu! songs folder path (usually in the osu! directory)
                string osuDirectory = GetOsuDirectory();
                if (!string.IsNullOrEmpty(osuDirectory))
                {
                    string songsPath = Path.Combine(osuDirectory, "Songs");
                    if (Directory.Exists(songsPath))
                    {
                        string beatmapPath = Path.Combine(songsPath, baseAddresses.Beatmap.FolderName, baseAddresses.Beatmap.OsuFileName);
                        if (File.Exists(beatmapPath))
                        {
                            // Only update if the path has actually changed
                            if (textBox_BeatmapPath.Text != beatmapPath)
                            {
                                textBox_BeatmapPath.Text = beatmapPath;

                                // Count hitobjects in the beatmap
                                int hitObjectCount = CountHitObjects(beatmapPath);
                                textBox_HitObjectsCount.Text = hitObjectCount.ToString();

                                // Update background image
                                UpdateBeatmapBackgroundImage(baseAddresses.Beatmap);

                                // Backend update completed silently - no debug text
                            }
                        }
                        else
                        {
                            // File doesn't exist, clear the path
                            if (!string.IsNullOrEmpty(textBox_BeatmapPath.Text))
                            {
                                textBox_BeatmapPath.Text = "";
                                textBox_HitObjectsCount.Text = "0";
                                ClearBackgroundImage();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors during auto-update to avoid spamming logs
                if (!string.IsNullOrEmpty(textBox_BeatmapPath.Text))
                {
                    textBox_BeatmapPath.Text = "";
                    textBox_HitObjectsCount.Text = "0";
                    ClearBackgroundImage();
                }
            }
        }

        private void UpdateBeatmapBackgroundImage(CurrentBeatmap beatmap)
        {
            try
            {
                string beatmapDirectory = GetBeatmapDirectory(beatmap);
                if (!string.IsNullOrEmpty(beatmapDirectory) && Directory.Exists(beatmapDirectory))
                {
                    string backgroundImagePath = FindBackgroundImage(beatmapDirectory);
                    if (!string.IsNullOrEmpty(backgroundImagePath))
                    {
                        // Load and display the background image with custom width-fit scaling and text overlay
                        using (var originalImage = Image.FromFile(backgroundImagePath))
                        {
                            // Custom scaling: Fit to full 420px width, center vertically, crop if needed, with text overlay
                            var fittedImage = FitImageToWidth(originalImage, 420, 170, beatmap);
                            pictureBox_Background.Image = fittedImage;
                        }
                    }
                    else
                    {
                        ClearBackgroundImage();
                    }
                }
                else
                {
                    ClearBackgroundImage();
                }
            }
            catch (Exception ex)
            {
                // Silently handle image loading errors
                ClearBackgroundImage();
            }
        }

        private Bitmap FitImageToExactDimensions(Image originalImage, int targetWidth, int targetHeight, CurrentBeatmap beatmap = null)
        {
            // Create a new bitmap with EXACT target dimensions (420x170)
            var resultImage = new Bitmap(targetWidth, targetHeight);

            using (var graphics = Graphics.FromImage(resultImage))
            {
                // Set high quality rendering
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Fill with black background first
                graphics.Clear(Color.Black);

                // Calculate scaling to fit the entire image within the target dimensions while maintaining aspect ratio
                float scaleX = (float)targetWidth / originalImage.Width;
                float scaleY = (float)targetHeight / originalImage.Height;
                float scale = System.Math.Min(scaleX, scaleY);

                // Calculate the scaled dimensions
                int scaledWidth = (int)(originalImage.Width * scale);
                int scaledHeight = (int)(originalImage.Height * scale);

                // Center the scaled image
                int x = (targetWidth - scaledWidth) / 2;
                int y = (targetHeight - scaledHeight) / 2;

                // Draw the scaled image centered in the target area
                graphics.DrawImage(originalImage, x, y, scaledWidth, scaledHeight);

                // Create gradient overlay from bottom (80% opaque) to top (20% opaque)
                using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, targetWidth, targetHeight), // Rectangle for gradient
                    Color.FromArgb(51, 0, 0, 0),  // Top: 20% opacity (51/255 = 0.2)
                    Color.FromArgb(204, 0, 0, 0), // Bottom: 80% opacity (204/255 = 0.8)
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    // Apply the gradient overlay
                    graphics.FillRectangle(gradientBrush, 0, 0, targetWidth, targetHeight);
                }

                // Draw beatmap information text overlay (bottom left)
                if (beatmap != null)
                {
                    DrawBeatmapInfoText(graphics, beatmap, targetWidth, targetHeight);
                }
            }

            // Ensure the final bitmap is exactly the target size
            return resultImage;
        }

        private Bitmap FitImageToWidth(Image originalImage, int targetWidth, int targetHeight, CurrentBeatmap beatmap = null)
        {
            // Calculate the exact width we want (420px)
            int resizedWidth = targetWidth; // EXACTLY 420px

            // Calculate height maintaining aspect ratio
            float aspectRatio = (float)originalImage.Height / originalImage.Width;
            int resizedHeight = (int)(resizedWidth * aspectRatio);

            // Create a new bitmap with EXACT target dimensions (420x170)
            var resultImage = new Bitmap(targetWidth, targetHeight);

            using (var graphics = Graphics.FromImage(resultImage))
            {
                // Set high quality rendering
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Fill with black background
                graphics.Clear(Color.Black);

                // Calculate Y position to center the resized image vertically in the 420x170 area
                int y = (targetHeight - resizedHeight) / 2;

                // If the resized height is larger than target height, crop from top/bottom
                if (resizedHeight > targetHeight)
                {
                    // Calculate how much to crop from top and bottom
                    int cropAmount = (resizedHeight - targetHeight) / 2;

                    // Create a scaled version of the original image at 420px width
                    var scaledImage = new Bitmap(resizedWidth, resizedHeight);
                    using (var scaleGraphics = Graphics.FromImage(scaledImage))
                    {
                        scaleGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        scaleGraphics.DrawImage(originalImage, 0, 0, resizedWidth, resizedHeight);
                    }

                    // Define the source rectangle (crop from scaled image)
                    var srcRect = new Rectangle(0, cropAmount, resizedWidth, targetHeight);

                    // Define the destination rectangle (EXACTLY 420x170)
                    var destRect = new Rectangle(0, 0, targetWidth, targetHeight);

                    // Draw the cropped portion to EXACTLY fill the 420x170 area
                    graphics.DrawImage(scaledImage, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    // No cropping needed, just center the resized image vertically
                    // Draw the resized image (420px wide) centered in the 420x170 area
                    graphics.DrawImage(originalImage, 0, y, resizedWidth, resizedHeight);
                }

                // Create gradient overlay from bottom (80% opaque) to top (20% opaque)
                using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, targetWidth, targetHeight), // Rectangle for gradient
                    Color.FromArgb(51, 0, 0, 0),  // Top: 20% opacity (51/255 = 0.2)
                    Color.FromArgb(204, 0, 0, 0), // Bottom: 80% opacity (204/255 = 0.8)
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    // Apply the gradient overlay
                    graphics.FillRectangle(gradientBrush, 0, 0, targetWidth, targetHeight);
                }

                // Draw beatmap information text overlay (bottom left)
                if (beatmap != null)
                {
                    DrawBeatmapInfoText(graphics, beatmap, targetWidth, targetHeight);
                }
            }

            // Ensure the final bitmap is exactly the target size
            return resultImage;
        }

        private void DrawBeatmapInfoText(Graphics graphics, CurrentBeatmap beatmap, int imageWidth, int imageHeight)
        {
            // Force load custom SF Pro fonts - no fallback allowed
            Font titleFont = null;
            Font infoFont = null;

            // Try multiple possible paths for the font files
            string[] possiblePaths = {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "SF-Pro-Display-Bold.otf"),
                Path.Combine(Directory.GetCurrentDirectory(), "assets"),
                Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "assets"),
                Path.Combine(Environment.CurrentDirectory, "assets")
            };

            string boldFontPath = null;
            string regularFontPath = null;

            foreach (string basePath in possiblePaths)
            {
                if (Directory.Exists(basePath))
                {
                    string testBoldPath = Path.Combine(basePath, "SF-Pro-Display-Bold.otf");
                    string testRegularPath = Path.Combine(basePath, "SF-Pro-Display-Regular.otf");

                    if (File.Exists(testBoldPath) && File.Exists(testRegularPath))
                    {
                        boldFontPath = testBoldPath;
                        regularFontPath = testRegularPath;
                        break;
                    }
                }
            }

            // ABSOLUTELY REQUIRE the custom fonts - no fallback allowed
            if (string.IsNullOrEmpty(boldFontPath) || string.IsNullOrEmpty(regularFontPath))
            {
                throw new FileNotFoundException("SF Pro Display fonts not found in assets folder. Please ensure both SF-Pro-Display-Bold.otf and SF-Pro-Display-Regular.otf are present in the assets directory.");
            }

            // Load the custom fonts - this will throw an exception if it fails
            var boldFontCollection = new System.Drawing.Text.PrivateFontCollection();
            var regularFontCollection = new System.Drawing.Text.PrivateFontCollection();

            boldFontCollection.AddFontFile(boldFontPath);
            regularFontCollection.AddFontFile(regularFontPath);

            if (boldFontCollection.Families.Length == 0 || regularFontCollection.Families.Length == 0)
            {
                throw new InvalidOperationException("Failed to load SF Pro Display font families from the font files.");
            }

            // Create fonts from the loaded font families
            titleFont = new Font(boldFontCollection.Families[0], 18, FontStyle.Bold);
            infoFont = new Font(regularFontCollection.Families[0], 12, FontStyle.Regular);

            // Define colors
            using (var shadowBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            using (var textBrush = new SolidBrush(Color.White))
            {
                // Extract beatmap information
                string title = "Unknown Title";
                string artist = "Unknown Artist";
                string difficulty = "Unknown Difficulty";

                if (!string.IsNullOrEmpty(beatmap.MapString))
                {
                    var parts = beatmap.MapString.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        artist = parts[0].Trim();
                        string fullTitle = parts[1].Trim();

                        // Remove difficulty name from title (anything in brackets [])
                        int bracketIndex = fullTitle.IndexOf('[');
                        if (bracketIndex > 0)
                        {
                            title = fullTitle.Substring(0, bracketIndex).Trim();
                        }
                        else
                        {
                            title = fullTitle;
                        }
                    }
                    else
                    {
                        string fullTitle = beatmap.MapString.Trim();
                        // Remove difficulty name from title (anything in brackets [])
                        int bracketIndex = fullTitle.IndexOf('[');
                        if (bracketIndex > 0)
                        {
                            title = fullTitle.Substring(0, bracketIndex).Trim();
                        }
                        else
                        {
                            title = fullTitle;
                        }
                    }
                }

                // Get difficulty name from .osu file if available
                if (!string.IsNullOrEmpty(beatmap.FolderName) && !string.IsNullOrEmpty(beatmap.OsuFileName))
                {
                    string osuDirectory = GetOsuDirectory();
                    if (!string.IsNullOrEmpty(osuDirectory))
                    {
                        string osuFilePath = Path.Combine(osuDirectory, "Songs", beatmap.FolderName, beatmap.OsuFileName);
                        if (File.Exists(osuFilePath))
                        {
                            difficulty = GetDifficultyNameFromFile(osuFilePath);
                        }
                    }
                }

                // Position for bottom left (with some padding)
                int startX = 12;
                int startY = imageHeight - 100; // Start 100px from bottom as requested
                int titleToArtistSpacing = 32; // Larger spacing after big title (18pt)
                int artistToDifficultySpacing = 30; // Smaller spacing between same-sized fonts (10pt)

                // Draw text with shadow effect for better readability
                void DrawTextWithShadow(string text, Font font, int x, int y)
                {
                    // Draw shadow (offset by 1 pixel)
                    graphics.DrawString(text, font, shadowBrush, x + 1, y + 1);
                    // Draw main text
                    graphics.DrawString(text, font, textBrush, x, y);
                }

                // Draw title (bold, larger)
                DrawTextWithShadow(title, titleFont, startX, startY);

                // Draw artist (with larger spacing after title)
                DrawTextWithShadow($"by {artist}", infoFont, startX, startY + titleToArtistSpacing);

                // Draw difficulty (with smaller spacing after artist)
                DrawTextWithShadow(difficulty, infoFont, startX, startY + titleToArtistSpacing + artistToDifficultySpacing);
            }

            // Clean up fonts
            titleFont?.Dispose();
            infoFont?.Dispose();
        }

        private string FindBackgroundImage(string beatmapDirectory)
        {
            // Common image extensions for beatmap backgrounds
            string[] imageExtensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };

            foreach (string extension in imageExtensions)
            {
                string[] files = Directory.GetFiles(beatmapDirectory, extension);
                if (files.Length > 0)
                {
                    // Return the first image found (usually the background)
                    return files[0];
                }
            }

            return null; // No background image found
        }

        private void ClearBackgroundImage()
        {
            if (pictureBox_Background.Image != null)
            {
                pictureBox_Background.Image.Dispose();
                pictureBox_Background.Image = null;
            }
        }

        private void RefreshBeatmapInfo()
        {
            try
            {
                var baseAddresses = new OsuBaseAddresses();
                if (_sreader.TryRead(baseAddresses.Beatmap))
                {
                    // Get osu! songs folder path (usually in the osu! directory)
                    string osuDirectory = GetOsuDirectory();

                    if (!string.IsNullOrEmpty(osuDirectory))
                    {
                        string songsPath = Path.Combine(osuDirectory, "Songs");

                        if (Directory.Exists(songsPath))
                        {
                            string beatmapPath = Path.Combine(songsPath, baseAddresses.Beatmap.FolderName, baseAddresses.Beatmap.OsuFileName);

                            textBox_BeatmapPath.Text = beatmapPath;

                            // Count hitobjects in the beatmap
                            int hitObjectCount = CountHitObjects(beatmapPath);
                            textBox_HitObjectsCount.Text = hitObjectCount.ToString();

                            // Update background image
                            UpdateBeatmapBackgroundImage(baseAddresses.Beatmap);

                            // Silent operation - no debug logging
                        }
                        else
                        {
                            textBox_BeatmapPath.Text = $"Songs folder not found: {songsPath}";
                            textBox_HitObjectsCount.Text = "0";
                            ClearBackgroundImage();
                        }
                    }
                    else
                    {
                        textBox_BeatmapPath.Text = "Unable to locate osu! directory";
                        textBox_HitObjectsCount.Text = "0";
                        ClearBackgroundImage();
                    }
                }
                else
                {
                    textBox_BeatmapPath.Text = "Unable to read beatmap data";
                    textBox_HitObjectsCount.Text = "0";
                    ClearBackgroundImage();
                }
            }
            catch (Exception ex)
            {
                textBox_BeatmapPath.Text = $"Error: {ex.Message}";
                textBox_HitObjectsCount.Text = "0";
                ClearBackgroundImage();
            }
        }



        private void SplitBeatmap()
        {
            try
            {
                string beatmapPath = textBox_BeatmapPath.Text;
                if (string.IsNullOrEmpty(beatmapPath) || !File.Exists(beatmapPath))
                {
                    MessageBox.Show("Beatmap file not found. Make sure:\n\n1. osu! is running\n2. You have a beatmap selected\n3. The beatmap file exists in your Songs folder\n\nThe application automatically detects beatmap changes, so the path should update when you select a beatmap in osu!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int numPieces = (int)trackBar_SplitPoint.Value;

                if (numPieces < 2)
                {
                    MessageBox.Show("Number of pieces must be at least 2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Read the beatmap file
                string[] lines = File.ReadAllLines(beatmapPath);
                var hitObjectsSection = new List<string>();
                var otherSections = new List<string>();
                bool inHitObjects = false;

                foreach (string line in lines)
                {
                    if (line.Trim() == "[HitObjects]")
                    {
                        inHitObjects = true;
                        otherSections.Add(line);
                        continue;
                    }

                    if (inHitObjects && (line.Trim().StartsWith("[") && line.Trim() != "[HitObjects]"))
                    {
                        inHitObjects = false;
                    }

                    if (inHitObjects)
                    {
                        hitObjectsSection.Add(line);
                    }
                    else
                    {
                        otherSections.Add(line);
                    }
                }

                // Filter out empty hitobject lines
                hitObjectsSection = hitObjectsSection.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

                if (hitObjectsSection.Count == 0)
                {
                    MessageBox.Show("No hit objects found in beatmap.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (hitObjectsSection.Count < numPieces)
                {
                    MessageBox.Show($"Not enough hit objects ({hitObjectsSection.Count}) to split into {numPieces} pieces. Each piece must have at least 1 object.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Split by count into N pieces
                List<List<string>> pieces = SplitHitObjectsByCountIntoPieces(hitObjectsSection, numPieces);

                // Create new beatmap files
                string directory = Path.GetDirectoryName(beatmapPath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(beatmapPath);
                string extension = Path.GetExtension(beatmapPath);

                // Get the original difficulty name
                string originalDifficultyName = GetDifficultyName(otherSections);

                var createdFiles = new List<string>();
                var successMessage = $"Beatmap split successfully into {numPieces} pieces!\n\n";

                for (int i = 0; i < pieces.Count; i++)
                {
                    string outputFileName = $"{fileNameWithoutExt} Part {i + 1} of {numPieces}{extension}";
                    string outputPath = Path.Combine(directory, outputFileName);

                    // Create a copy of otherSections for modification
                    var modifiedSections = new List<string>(otherSections);

                    // Update difficulty name for split pieces
                    string partSuffix = GetPartSuffix(i + 1, numPieces);
                    UpdateDifficultyName(modifiedSections, originalDifficultyName, partSuffix);

                    CreateSplitBeatmapFile(modifiedSections, pieces[i], outputPath);
                    createdFiles.Add(Path.GetFileName(outputPath));
                    successMessage += $"Part {i + 1}: {pieces[i].Count} objects\n";
                }

                successMessage += $"\nFiles created:\n{string.Join("\n", createdFiles)}";

                // Automatically create a .osz archive from the beatmap folder and open it (imports into osu!)
                try
                {
                    string oszPath = CreateOsuBeatmapArchive(directory);
                    if (!string.IsNullOrEmpty(oszPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = oszPath,
                            UseShellExecute = true
                        });
                        // Silent success - no message box shown to the user
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating .osz file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Refresh beatmap info
                RefreshBeatmapInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error splitting beatmap: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<List<string>> SplitHitObjectsByCountIntoPieces(List<string> hitObjects, int numPieces)
        {
            var pieces = new List<List<string>>();
            int totalObjects = hitObjects.Count;
            int objectsPerPiece = totalObjects / numPieces;
            int remainder = totalObjects % numPieces;

            int currentIndex = 0;
            for (int i = 0; i < numPieces; i++)
            {
                int currentPieceSize = objectsPerPiece;
                if (i < remainder)
                {
                    currentPieceSize++; // Distribute remainder objects to first pieces
                }

                var piece = hitObjects.Skip(currentIndex).Take(currentPieceSize).ToList();
                pieces.Add(piece);
                currentIndex += currentPieceSize;
            }

            return pieces;
        }



        private void CreateSplitBeatmapFile(List<string> otherSections, List<string> hitObjects, string outputPath)
        {
            var outputLines = new List<string>(otherSections);

            // Insert hit objects section
            for (int i = 0; i < outputLines.Count; i++)
            {
                if (outputLines[i].Trim() == "[HitObjects]")
                {
                    outputLines.InsertRange(i + 1, hitObjects);
                    break;
                }
            }

            File.WriteAllLines(outputPath, outputLines);
        }

        private int CountHitObjects(string beatmapPath)
        {
            if (!File.Exists(beatmapPath))
                return 0;

            string[] lines = File.ReadAllLines(beatmapPath);
            bool inHitObjects = false;
            int count = 0;

            foreach (string line in lines)
            {
                if (line.Trim() == "[HitObjects]")
                {
                    inHitObjects = true;
                    continue;
                }

                if (inHitObjects && line.Trim().StartsWith("[") && line.Trim() != "[HitObjects]")
                {
                    break;
                }

                if (inHitObjects && !string.IsNullOrWhiteSpace(line) && line.Contains(","))
                {
                    count++;
                }
            }

            return count;
        }

        private string GetOsuDirectory()
        {
            try
            {
                // Try to find osu! directory from the process
                var processes = Process.GetProcessesByName("osu!");
                if (processes.Length > 0)
                {
                    string processPath = processes[0].MainModule.FileName;
                    return Path.GetDirectoryName(processPath);
                }
            }
            catch
            {
                // Fallback: try common osu! installation paths
                string[] commonPaths = {
                    @"C:\Program Files\osu!",
                    @"C:\Program Files (x86)\osu!",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu!"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu!")
                };

                foreach (string path in commonPaths)
                {
                    if (Directory.Exists(path))
                        return path;
                }
            }

            return null;
        }

        private string GetDifficultyName(List<string> sections)
        {
            bool inMetadata = false;

            foreach (string line in sections)
            {
                if (line.Trim() == "[Metadata]")
                {
                    inMetadata = true;
                    continue;
                }

                if (inMetadata && line.Trim().StartsWith("[") && line.Trim() != "[Metadata]")
                {
                    break; // End of Metadata section
                }

                if (inMetadata && line.StartsWith("Version:"))
                {
                    return line.Substring(8).Trim(); // Remove "Version:" prefix
                }
            }

            return "Unknown Difficulty"; // Fallback if not found
        }

        private string GetPartSuffix(int partNumber, int totalParts)
        {
            // Always use consistent "Part X/Y" format
            return $"Part {partNumber}/{totalParts}";
        }

        private void UpdateDifficultyName(List<string> sections, string originalName, string suffix)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                if (sections[i].StartsWith("Version:"))
                {
                    sections[i] = $"Version:{originalName} {suffix}";
                    break;
                }
            }
        }

        private string CreateOsuBeatmapArchive(string beatmapDirectory)
        {
            try
            {
                if (string.IsNullOrEmpty(beatmapDirectory) || !Directory.Exists(beatmapDirectory))
                    throw new Exception("Beatmap directory not found.");

                // Use the beatmap folder name for the archive
                string parent = Path.GetDirectoryName(beatmapDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                string folderName = Path.GetFileName(beatmapDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                string zipPath = Path.Combine(parent, folderName + ".zip");
                string oszPath = Path.Combine(parent, folderName + ".osz");

                // If existing zip/osz exist, overwrite
                if (File.Exists(zipPath)) File.Delete(zipPath);
                if (File.Exists(oszPath)) File.Delete(oszPath);

                // Create zip from the entire beatmap folder
                ZipFile.CreateFromDirectory(beatmapDirectory, zipPath, CompressionLevel.Optimal, false);

                // Rename to .osz
                File.Move(zipPath, oszPath);

                return oszPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create .osz archive: {ex.Message}");
            }
        }
    }
}
