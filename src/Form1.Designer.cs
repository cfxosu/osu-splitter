
namespace StructuredOsuMemoryProviderTester
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                // dispose loaded application icon if present
                try
                {
                    // _appIcon is declared in the other partial class file
                    var field = this.GetType().GetField("_appIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var icon = field.GetValue(this) as System.Drawing.Icon;
                        icon?.Dispose();
                        field.SetValue(this, null);
                    }
                }
                catch
                {
                    // ignore errors during dispose
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox_Background = new System.Windows.Forms.PictureBox();
            this.button_SplitBeatmapBottom = new System.Windows.Forms.Button();
            this.button_Close = new System.Windows.Forms.Button();
            this.button_Minimize = new System.Windows.Forms.Button();
            this.panel_CustomTitleBar = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Background)).BeginInit();
            this.label_HitObjectsCount = new System.Windows.Forms.Label();
            this.trackBar_SplitPoint = new StructuredOsuMemoryProviderTester.CustomSlider();
            this.label_SplitPoint = new System.Windows.Forms.Label();
            this.label_SplitValue = new System.Windows.Forms.Label();
            this.label_HintText = new System.Windows.Forms.Label();
            this.textBox_BeatmapPath = new System.Windows.Forms.TextBox();
            this.label_BeatmapPath = new System.Windows.Forms.Label();
            this.textBox_HitObjectsCount = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_SplitPoint)).BeginInit();
            this.SuspendLayout();
            //
            // pictureBox_Background
            //
            this.pictureBox_Background.BackColor = System.Drawing.Color.FromArgb(28, 28, 30);
            this.pictureBox_Background.Location = new System.Drawing.Point(3, 3);
            this.pictureBox_Background.Name = "pictureBox_Background";
            this.pictureBox_Background.Size = new System.Drawing.Size(414, 224);
            this.pictureBox_Background.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Background.TabIndex = 0;
            this.pictureBox_Background.TabStop = false;
            this.pictureBox_Background.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            //
            // label_HitObjectsCount
            //
            this.label_HitObjectsCount.AutoSize = true;
            this.label_HitObjectsCount.Location = new System.Drawing.Point(250, 216);
            this.label_HitObjectsCount.Name = "label_HitObjectsCount";
            this.label_HitObjectsCount.Size = new System.Drawing.Size(64, 15);
            this.label_HitObjectsCount.TabIndex = 35;
            this.label_HitObjectsCount.Text = "HitObjects:";
            this.label_HitObjectsCount.Visible = false;
            //
            // trackBar_SplitPoint
            //
            this.trackBar_SplitPoint.Location = new System.Drawing.Point(20, 241);
            this.trackBar_SplitPoint.Maximum = 18;
            this.trackBar_SplitPoint.Minimum = 2;
            this.trackBar_SplitPoint.Name = "trackBar_SplitPoint";
            this.trackBar_SplitPoint.Size = new System.Drawing.Size(380, 45);
            this.trackBar_SplitPoint.TabIndex = 31;
            this.trackBar_SplitPoint.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar_SplitPoint.Value = 4;
            this.trackBar_SplitPoint.ValueChanged += new System.EventHandler(this.trackBar_SplitPoint_ValueChanged);
            //
            // label_SplitValue
            //
            this.label_SplitValue.AutoSize = false;
            this.label_SplitValue.Font = new System.Drawing.Font("SF Pro Display", 28F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_SplitValue.ForeColor = System.Drawing.Color.White;
            this.label_SplitValue.Location = new System.Drawing.Point(180, 280);
            this.label_SplitValue.Name = "label_SplitValue";
            this.label_SplitValue.Size = new System.Drawing.Size(60, 40);
            this.label_SplitValue.TabIndex = 0;
            this.label_SplitValue.Text = "4";
            this.label_SplitValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_SplitValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.label_SplitValue.Click += new System.EventHandler(this.label_SplitValue_Click);
            this.label_SplitValue.MouseEnter += new System.EventHandler(this.label_SplitValue_MouseEnter);
            this.label_SplitValue.MouseLeave += new System.EventHandler(this.label_SplitValue_MouseLeave);
            //
            // label_HintText
            //
            this.label_HintText.AutoSize = true;
            this.label_HintText.Font = new System.Drawing.Font("SF Pro Display", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_HintText.ForeColor = System.Drawing.Color.FromArgb(203, 203, 187);
            this.label_HintText.Location = new System.Drawing.Point(125, 325);
            this.label_HintText.Name = "label_HintText";
            this.label_HintText.Size = new System.Drawing.Size(140, 15);
            this.label_HintText.TabIndex = 32;
            this.label_HintText.Text = "Click to edit • Drag to adjust";
            this.label_HintText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_HintText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            //
            // label_SplitPoint
            //
            this.label_SplitPoint.AutoSize = true;
            this.label_SplitPoint.Font = new System.Drawing.Font("SF Pro Display", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_SplitPoint.ForeColor = System.Drawing.Color.FromArgb(203, 203, 187);
            this.label_SplitPoint.Location = new System.Drawing.Point(175, 220);
            this.label_SplitPoint.Name = "label_SplitPoint";
            this.label_SplitPoint.Size = new System.Drawing.Size(420, 15);
            this.label_SplitPoint.TabIndex = 30;
            this.label_SplitPoint.Text = "Split Into";
            this.label_SplitPoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_SplitPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            //
            // textBox_BeatmapPath
            //
            this.textBox_BeatmapPath.Location = new System.Drawing.Point(70, 184);
            this.textBox_BeatmapPath.Name = "textBox_BeatmapPath";
            this.textBox_BeatmapPath.ReadOnly = true;
            this.textBox_BeatmapPath.Size = new System.Drawing.Size(244, 23);
            this.textBox_BeatmapPath.TabIndex = 29;
            this.textBox_BeatmapPath.Visible = false;
            //
            // label_BeatmapPath
            //
            this.label_BeatmapPath.AutoSize = true;
            this.label_BeatmapPath.Location = new System.Drawing.Point(6, 187);
            this.label_BeatmapPath.Name = "label_BeatmapPath";
            this.label_BeatmapPath.Size = new System.Drawing.Size(58, 15);
            this.label_BeatmapPath.TabIndex = 28;
            this.label_BeatmapPath.Text = "";
            //
            // textBox_HitObjectsCount
            //
            this.textBox_HitObjectsCount.Location = new System.Drawing.Point(320, 213);
            this.textBox_HitObjectsCount.Name = "textBox_HitObjectsCount";
            this.textBox_HitObjectsCount.ReadOnly = true;
            this.textBox_HitObjectsCount.Size = new System.Drawing.Size(70, 23);
            this.textBox_HitObjectsCount.TabIndex = 36;
            this.textBox_HitObjectsCount.Visible = false;
            //
            // button_SplitBeatmapBottom
            //
            this.button_SplitBeatmapBottom.BackColor = System.Drawing.Color.FromArgb(10, 132, 255);
            this.button_SplitBeatmapBottom.FlatAppearance.BorderSize = 0;
            this.button_SplitBeatmapBottom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_SplitBeatmapBottom.Font = new System.Drawing.Font("SF Pro Display", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_SplitBeatmapBottom.ForeColor = System.Drawing.Color.White;
            this.button_SplitBeatmapBottom.Location = new System.Drawing.Point(20, 366);
            this.button_SplitBeatmapBottom.Name = "button_SplitBeatmapBottom";
            this.button_SplitBeatmapBottom.Size = new System.Drawing.Size(380, 50);
            this.button_SplitBeatmapBottom.TabIndex = 37;
            this.button_SplitBeatmapBottom.Text = "Split Beatmap";
            this.button_SplitBeatmapBottom.UseVisualStyleBackColor = false;
            this.button_SplitBeatmapBottom.Click += new System.EventHandler(this.button_SplitBeatmapBottom_Click);
            this.button_SplitBeatmapBottom.MouseEnter += new System.EventHandler(this.button_SplitBeatmapBottom_MouseEnter);
            this.button_SplitBeatmapBottom.MouseLeave += new System.EventHandler(this.button_SplitBeatmapBottom_MouseLeave);
            //
            // button_Close
            //
            this.button_Close.BackColor = System.Drawing.Color.FromArgb(28, 28, 30);
            this.button_Close.FlatAppearance.BorderSize = 0;
            this.button_Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Close.Font = new System.Drawing.Font("SF Pro Display", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Close.ForeColor = System.Drawing.Color.White;
            this.button_Close.Location = new System.Drawing.Point(375, 4);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(25, 25);
            this.button_Close.TabIndex = 38;
            this.button_Close.Text = "×";
            this.button_Close.UseVisualStyleBackColor = false;
            this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
            this.button_Close.MouseEnter += new System.EventHandler(this.button_Close_MouseEnter);
            this.button_Close.MouseLeave += new System.EventHandler(this.button_Close_MouseLeave);
            //
            // button_Minimize
            //
            this.button_Minimize.BackColor = System.Drawing.Color.FromArgb(28, 28, 30);
            this.button_Minimize.FlatAppearance.BorderSize = 0;
            this.button_Minimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Minimize.Font = new System.Drawing.Font("SF Pro Display", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Minimize.ForeColor = System.Drawing.Color.White;
            this.button_Minimize.Location = new System.Drawing.Point(345, 4);
            this.button_Minimize.Name = "button_Minimize";
            this.button_Minimize.Size = new System.Drawing.Size(25, 25);
            this.button_Minimize.TabIndex = 39;
            this.button_Minimize.Text = "−";
            this.button_Minimize.UseVisualStyleBackColor = false;
            this.button_Minimize.Click += new System.EventHandler(this.button_Minimize_Click);
            this.button_Minimize.MouseEnter += new System.EventHandler(this.button_Minimize_MouseEnter);
            this.button_Minimize.MouseLeave += new System.EventHandler(this.button_Minimize_MouseLeave);
            //
            // panel_CustomTitleBar
            //
            this.panel_CustomTitleBar.BackColor = System.Drawing.Color.Transparent;
            this.panel_CustomTitleBar.Controls.Add(this.button_Close);
            this.panel_CustomTitleBar.Controls.Add(this.button_Minimize);
            this.panel_CustomTitleBar.Location = new System.Drawing.Point(3, 3);
            this.panel_CustomTitleBar.Name = "panel_CustomTitleBar";
            this.panel_CustomTitleBar.Size = new System.Drawing.Size(414, 34);
            this.panel_CustomTitleBar.TabIndex = 40;
            this.panel_CustomTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_CustomTitleBar_MouseDown);
            this.panel_CustomTitleBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_CustomTitleBar_MouseMove);
            this.panel_CustomTitleBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_CustomTitleBar_MouseUp);
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 440);
            this.Controls.Add(this.panel_CustomTitleBar);
            this.Controls.Add(this.textBox_HitObjectsCount);
            this.Controls.Add(this.label_HitObjectsCount);
            this.Controls.Add(this.label_HintText);
            this.Controls.Add(this.label_SplitValue);
            this.Controls.Add(this.trackBar_SplitPoint);
            this.Controls.Add(this.label_SplitPoint);
            this.Controls.Add(this.textBox_BeatmapPath);
            this.Controls.Add(this.label_BeatmapPath);
            this.Controls.Add(this.pictureBox_Background);
            this.Controls.Add(this.button_SplitBeatmapBottom);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            // Base window title
            this.Text = "osu-splitter";
            this.BackColor = System.Drawing.Color.FromArgb(28, 28, 30);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Background)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_SplitPoint)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_Background;
        private System.Windows.Forms.TextBox textBox_BeatmapPath;
        private System.Windows.Forms.Label label_BeatmapPath;
        private System.Windows.Forms.Label label_SplitPoint;
        private StructuredOsuMemoryProviderTester.CustomSlider trackBar_SplitPoint;
        private System.Windows.Forms.Label label_SplitValue;
        private System.Windows.Forms.Label label_HintText;
        private System.Windows.Forms.Label label_HitObjectsCount;
        private System.Windows.Forms.TextBox textBox_HitObjectsCount;
        private System.Windows.Forms.Button button_SplitBeatmapBottom;
        private System.Windows.Forms.Button button_Close;
        private System.Windows.Forms.Button button_Minimize;
        private System.Windows.Forms.Panel panel_CustomTitleBar;
    }
}
