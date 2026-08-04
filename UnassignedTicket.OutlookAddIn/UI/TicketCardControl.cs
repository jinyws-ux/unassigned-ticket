using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using UnassignedTicket.OutlookAddIn.Models;

namespace UnassignedTicket.OutlookAddIn.UI
{
    internal sealed class TicketCardControl : Panel
    {
        private static readonly Color BorderColor = Color.FromArgb(226, 232, 240);

        internal TicketCardControl(Ticket ticket)
        {
            Ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
            Width = 320;
            Height = 118;
            Margin = new Padding(0, 0, 0, 10);
            Padding = new Padding(12);
            BackColor = Color.White;
            Cursor = IsSafeUrl(ticket.Url) ? Cursors.Hand : Cursors.Default;

            var typeLabel = NewLabel(ticket.Type, 10, 9, 42, 22, true, Color.FromArgb(37, 99, 235));
            var idLabel = NewLabel(ticket.Id, 58, 10, 170, 20, true, Color.FromArgb(15, 23, 42));
            var priorityLabel = NewLabel(ticket.Priority ?? string.Empty, 232, 10, 76, 20, false, PriorityColor(ticket.Priority));
            priorityLabel.TextAlign = ContentAlignment.MiddleRight;

            var summaryLabel = NewLabel(ticket.Summary, 10, 39, 298, 36, false, Color.FromArgb(51, 65, 85));
            summaryLabel.AutoEllipsis = true;

            var waitLabel = NewLabel(FormatWaiting(ticket.WaitingTime), 10, 85, 145, 20, false, Color.FromArgb(100, 116, 139));
            var statusLabel = NewLabel(ticket.Status ?? string.Empty, 165, 85, 143, 20, false, Color.FromArgb(100, 116, 139));
            statusLabel.TextAlign = ContentAlignment.MiddleRight;

            Controls.Add(typeLabel);
            Controls.Add(idLabel);
            Controls.Add(priorityLabel);
            Controls.Add(summaryLabel);
            Controls.Add(waitLabel);
            Controls.Add(statusLabel);

            foreach (Control control in Controls)
            {
                control.Cursor = Cursor;
                control.Click += OpenTicket;
            }
            Click += OpenTicket;
        }

        internal Ticket Ticket { get; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(BorderColor))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private static Label NewLabel(string text, int left, int top, int width, int height, bool bold, Color color)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                ForeColor = color,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, bold ? FontStyle.Bold : FontStyle.Regular),
                AutoEllipsis = true
            };
        }

        private static Color PriorityColor(string priority)
        {
            if (string.IsNullOrWhiteSpace(priority)) return Color.FromArgb(100, 116, 139);
            string value = priority.ToUpperInvariant();
            if (value.Contains("CRITICAL") || value.Contains("P1")) return Color.FromArgb(185, 28, 28);
            if (value.Contains("HIGH") || value.Contains("P2")) return Color.FromArgb(194, 65, 12);
            return Color.FromArgb(100, 116, 139);
        }

        private static string FormatWaiting(TimeSpan waiting)
        {
            if (waiting.TotalMinutes < 1) return "刚刚进入队列";
            if (waiting.TotalHours < 1) return "等待 " + Math.Max(1, (int)waiting.TotalMinutes) + " 分钟";
            if (waiting.TotalDays < 1) return "等待 " + (int)waiting.TotalHours + " 小时";
            return "等待 " + (int)waiting.TotalDays + " 天";
        }

        private void OpenTicket(object sender, EventArgs e)
        {
            if (!IsSafeUrl(Ticket.Url)) return;
            try
            {
                Process.Start(new ProcessStartInfo(Ticket.Url) { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show("无法打开该工单链接。", "ITCC 未分配工单", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static bool IsSafeUrl(string value)
        {
            Uri uri;
            return Uri.TryCreate(value, UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
