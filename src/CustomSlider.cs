using System;
using System.Drawing;
using System.Windows.Forms;

namespace StructuredOsuMemoryProviderTester
{
    public class CustomSlider : TrackBar
    {
        private Color _trackColor = Color.FromArgb(60, 60, 65);
        private Color _progressColor = Color.FromArgb(10, 132, 255); // Blue color
        private Color _thumbColor = Color.White;
        private Color _thumbBorderColor = Color.FromArgb(100, 100, 105);
        private bool _isHovering = false;

        // Properties to customize colors
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; Invalidate(); }
        }

        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; Invalidate(); }
        }

        public Color ThumbBorderColor
        {
            get => _thumbBorderColor;
            set { _thumbBorderColor = value; Invalidate(); }
        }

        public CustomSlider()
        {
            // Enable double buffering for smoother rendering
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);

            // Set default size
            this.Height = 45;
            this.TickStyle = TickStyle.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Get the dimensions
            Rectangle trackRect = GetTrackRectangle();
            Rectangle thumbRect = GetThumbRectangle();

            // Calculate progress ratio
            float progressRatio = (float)(Value - Minimum) / (Maximum - Minimum);

            // Draw track background (full length)
            using (var trackBrush = new SolidBrush(TrackColor))
            {
                e.Graphics.FillRectangle(trackBrush, trackRect);
            }

            // Draw progress track (filled portion)
            if (progressRatio > 0)
            {
                Rectangle progressRect = new Rectangle(
                    trackRect.X,
                    trackRect.Y,
                    (int)(trackRect.Width * progressRatio),
                    trackRect.Height
                );

                using (var progressBrush = new SolidBrush(ProgressColor))
                {
                    e.Graphics.FillRectangle(progressBrush, progressRect);
                }
            }

            // Draw glow effect when hovering
            if (_isHovering)
            {
                Rectangle glowRect = new Rectangle(
                    thumbRect.X - 2,
                    thumbRect.Y - 2,
                    thumbRect.Width + 4,
                    thumbRect.Height + 4
                );

                using (var glowBrush = new SolidBrush(Color.FromArgb(40, Color.LightBlue)))
                {
                    e.Graphics.FillEllipse(glowBrush, glowRect);
                }
            }

            // Draw thumb
            using (var thumbBrush = new SolidBrush(ThumbColor))
            using (var thumbBorderPen = new Pen(ThumbBorderColor, 1))
            {
                e.Graphics.FillEllipse(thumbBrush, thumbRect);
                e.Graphics.DrawEllipse(thumbBorderPen, thumbRect);
            }

            // Draw inner highlight on thumb for 3D effect
            Rectangle highlightRect = new Rectangle(
                thumbRect.X + 2,
                thumbRect.Y + 2,
                thumbRect.Width - 4,
                thumbRect.Height / 2 - 1
            );

            using (var highlightBrush = new SolidBrush(Color.FromArgb(60, Color.White)))
            {
                e.Graphics.FillEllipse(highlightBrush, highlightRect);
            }
        }

        private Rectangle GetTrackRectangle()
        {
            // Calculate track dimensions (centered vertically)
            int trackHeight = 4;
            int trackY = (Height - trackHeight) / 2;
            int trackMargin = 16; // Increased margin to account for larger thumb size when hovering

            return new Rectangle(
                trackMargin,
                trackY,
                Width - (trackMargin * 2),
                trackHeight
            );
        }

        private Rectangle GetThumbRectangle()
        {
            // Calculate thumb position based on current value
            Rectangle trackRect = GetTrackRectangle();
            float progressRatio = (float)(Value - Minimum) / (Maximum - Minimum);

            int thumbSize = _isHovering ? 24 : 20; // Pop effect: larger when hovering
            int thumbX = trackRect.X + (int)(trackRect.Width * progressRatio) - (thumbSize / 2);
            int thumbY = (Height - thumbSize) / 2;

            // Ensure thumb stays within bounds, but allow it to reach the full track width
            thumbX = Math.Max(trackRect.X - thumbSize / 2, Math.Min(thumbX, trackRect.Right - thumbSize / 2 + 1));

            return new Rectangle(thumbX, thumbY, thumbSize, thumbSize);
        }

        // Override to trigger repaint when value changes
        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            Invalidate();
        }

        // Handle mouse events for better interaction
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            UpdateValueFromMouse(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                UpdateValueFromMouse(e.X);
                // Don't check hover effects while dragging to prevent repeated resizing
                return;
            }

            // Check if mouse is over the thumb for hover effect (only when not dragging)
            Rectangle thumbRect = GetThumbRectangle();
            bool isMouseOverThumb = thumbRect.Contains(e.Location);

            if (isMouseOverThumb != _isHovering)
            {
                _isHovering = isMouseOverThumb;
                Invalidate(); // Trigger repaint to show/hide pop effect
            }
        }

        private void UpdateValueFromMouse(int mouseX)
        {
            Rectangle trackRect = GetTrackRectangle();

            // Use the full width of the slider control for calculation
            float ratio = (float)(mouseX - 0) / Width;
            ratio = Math.Max(0, Math.Min(1, ratio));

            int newValue = (int)(Minimum + ratio * (Maximum - Minimum));

            // Ensure we can always reach the maximum value when clicking at the far right
            if (mouseX >= Width - 10) // If clicking in the last 10 pixels, set to maximum
            {
                newValue = Maximum;
            }
            else if (mouseX <= 10) // If clicking in the first 10 pixels, set to minimum
            {
                newValue = Minimum;
            }

            if (newValue != Value)
            {
                Value = newValue;
            }
        }

        // Handle mouse leave to reset hover state when mouse exits slider
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_isHovering)
            {
                _isHovering = false;
                Invalidate(); // Trigger repaint to return to normal size
            }
        }
    }
}
