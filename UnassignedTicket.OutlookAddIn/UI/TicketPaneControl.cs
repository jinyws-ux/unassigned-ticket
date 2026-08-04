using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UnassignedTicket.OutlookAddIn.Data;
using UnassignedTicket.OutlookAddIn.Models;

namespace UnassignedTicket.OutlookAddIn.UI
{
    internal sealed class TicketPaneControl : UserControl
    {
        private readonly SettingsStore _settingsStore = new SettingsStore();
        private readonly System.Windows.Forms.Timer _refreshTimer;
        private readonly Label _statusLabel;
        private readonly Label _totalValue;
        private readonly Label _incValue;
        private readonly Label _woValue;
        private readonly FlowLayoutPanel _ticketList;
        private readonly Button _allButton;
        private readonly Button _incButton;
        private readonly Button _woButton;
        private readonly Button _refreshButton;
        private readonly NotifyIcon _notifyIcon;
        private readonly Action _showPane;
        private List<Ticket> _tickets = new List<Ticket>();
        private HashSet<string> _knownTicketIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private string _filter = "ALL";
        private int _isRefreshing;
        private bool _hasSuccessfulSnapshot;
        private bool _databaseConfigured;
        private bool _isShuttingDown;

        internal TicketPaneControl(Action showPane)
        {
            _showPane = showPane;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(300, 400);

            var header = new Panel { Dock = DockStyle.Top, Height = 94, BackColor = Color.White, Padding = new Padding(14, 12, 14, 8) };
            var title = new Label
            {
                Text = "ITCC 未分配工单",
                Left = 14,
                Top = 13,
                Width = 210,
                Height = 27,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            _statusLabel = new Label
            {
                Text = "准备查询",
                Left = 15,
                Top = 49,
                Width = 245,
                Height = 24,
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoEllipsis = true
            };
            _refreshButton = CreateActionButton("刷新", 250, 12, 58);
            _refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _refreshButton.Click += async (sender, args) => await RefreshTicketsAsync();
            var settingsButton = CreateActionButton("设置", 250, 49, 58);
            settingsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            settingsButton.Click += (sender, args) => OpenSettingsDialog();
            header.Controls.AddRange(new Control[] { title, _statusLabel, _refreshButton, settingsButton });

            var stats = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 78,
                Padding = new Padding(10, 8, 10, 7),
                ColumnCount = 3,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            _totalValue = AddStat(stats, "全部", 0);
            _incValue = AddStat(stats, "INC", 1);
            _woValue = AddStat(stats, "WO", 2);

            var filters = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10, 6, 10, 6) };
            _allButton = CreateFilterButton("全部", "ALL", 10);
            _incButton = CreateFilterButton("INC", "INC", 92);
            _woButton = CreateFilterButton("WO", "WO", 174);
            filters.Controls.AddRange(new Control[] { _allButton, _incButton, _woButton });

            _ticketList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 4, 10, 12),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            _ticketList.SizeChanged += (sender, args) => ResizeTicketCards();

            Controls.Add(_ticketList);
            Controls.Add(filters);
            Controls.Add(stats);
            Controls.Add(header);

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Text = "ITCC 未分配工单",
                Visible = true
            };
            _notifyIcon.BalloonTipClicked += (sender, args) => _showPane?.Invoke();

            _refreshTimer = new System.Windows.Forms.Timer { Interval = 5 * 60_000 };
            _refreshTimer.Tick += async (sender, args) => await RefreshTicketsAsync();
            Load += async (sender, args) =>
            {
                _refreshTimer.Start();
                await RefreshTicketsAsync();
            };
            UpdateFilterButtons();
            RenderTickets();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_isShuttingDown)
            {
                _isShuttingDown = true;
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private static Button CreateActionButton(string text, int left, int top, int width)
        {
            return new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = width,
                Height = 29,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85)
            };
        }

        private static Label AddStat(TableLayoutPanel parent, string caption, int column)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3), BackColor = Color.White };
            var value = new Label
            {
                Text = "0",
                Dock = DockStyle.Top,
                Height = 31,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            var label = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            panel.Controls.Add(label);
            panel.Controls.Add(value);
            parent.Controls.Add(panel, column, 0);
            return value;
        }

        private Button CreateFilterButton(string text, string filter, int left)
        {
            var button = new Button
            {
                Text = text,
                Left = left,
                Top = 6,
                Width = 74,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Tag = filter
            };
            button.Click += (sender, args) =>
            {
                _filter = (string)((Button)sender).Tag;
                UpdateFilterButtons();
                RenderTickets();
            };
            return button;
        }

        private async System.Threading.Tasks.Task RefreshTicketsAsync()
        {
            if (_isShuttingDown) return;
            if (Interlocked.Exchange(ref _isRefreshing, 1) == 1) return;

            _refreshButton.Enabled = false;
            _statusLabel.Text = "正在查询数据库…";
            _statusLabel.ForeColor = Color.FromArgb(37, 99, 235);
            try
            {
                DatabaseSettings settings = _settingsStore.Load();
                _databaseConfigured = settings.IsConfigured;
                if (!settings.IsConfigured)
                {
                    _tickets.Clear();
                    _statusLabel.Text = "尚未配置数据库，请点击“设置”";
                    _statusLabel.ForeColor = Color.FromArgb(180, 83, 9);
                    RenderTickets();
                    return;
                }

                var repository = new DbTicketRepository(settings);
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(settings.CommandTimeoutSeconds + 2)))
                {
                    IReadOnlyList<Ticket> result = await repository.GetUnassignedTicketsAsync(timeout.Token);
                    if (_isShuttingDown) return;
                    List<Ticket> refreshedTickets = result.ToList();
                    NotifyAboutNewTickets(refreshedTickets);
                    _tickets = refreshedTickets;
                }

                _statusLabel.Text = "更新于 " + DateTime.Now.ToString("HH:mm:ss");
                _statusLabel.ForeColor = Color.FromArgb(22, 101, 52);
                RenderTickets();
            }
            catch (OperationCanceledException)
            {
                if (!_isShuttingDown) ShowRefreshError("数据库查询超时，保留上次结果");
            }
            catch (Exception ex)
            {
                if (!_isShuttingDown) ShowRefreshError(ex.Message);
            }
            finally
            {
                if (!_isShuttingDown && !_refreshButton.IsDisposed)
                {
                    _refreshButton.Enabled = true;
                }
                Interlocked.Exchange(ref _isRefreshing, 0);
            }
        }

        private void ShowRefreshError(string message)
        {
            _statusLabel.Text = message;
            _statusLabel.ForeColor = Color.FromArgb(185, 28, 28);
            RenderTickets();
        }

        internal void OpenSettingsDialog()
        {
            DatabaseSettings settings;
            try
            {
                settings = _settingsStore.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "数据库设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                settings = new DatabaseSettings();
            }

            using (var form = new ConnectionSettingsForm(settings))
            {
                if (form.ShowDialog(FindForm()) != DialogResult.OK) return;
                try
                {
                    _settingsStore.Save(form.Result);
                    _databaseConfigured = true;
                    _knownTicketIds.Clear();
                    _hasSuccessfulSnapshot = false;
                    _ = RefreshTicketsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败：" + ex.Message, "数据库设置", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void NotifyAboutNewTickets(IReadOnlyList<Ticket> refreshedTickets)
        {
            var currentIds = new HashSet<string>(
                refreshedTickets.Where(ticket => !string.IsNullOrWhiteSpace(ticket.Id)).Select(ticket => ticket.Id),
                StringComparer.OrdinalIgnoreCase);

            if (_hasSuccessfulSnapshot)
            {
                List<Ticket> newTickets = refreshedTickets
                    .Where(ticket => !string.IsNullOrWhiteSpace(ticket.Id) && !_knownTicketIds.Contains(ticket.Id))
                    .ToList();

                if (newTickets.Count > 0)
                {
                    string message = newTickets.Count == 1
                        ? newTickets[0].Id + "：" + Truncate(newTickets[0].Summary, 120)
                        : "发现 " + newTickets.Count + " 个新的未分配工单：" +
                          string.Join("、", newTickets.Take(3).Select(ticket => ticket.Id)) +
                          (newTickets.Count > 3 ? " 等" : string.Empty);

                    _notifyIcon.BalloonTipTitle = "ITCC 新增未分配工单";
                    _notifyIcon.BalloonTipText = message;
                    _notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
                    _notifyIcon.ShowBalloonTip(10000);
                }
            }

            _knownTicketIds = currentIds;
            _hasSuccessfulSnapshot = true;
        }

        private static string Truncate(string value, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return "（无摘要）";
            return value.Length <= maximumLength ? value : value.Substring(0, maximumLength - 1) + "…";
        }

        private void UpdateFilterButtons()
        {
            StyleFilterButton(_allButton, _filter == "ALL");
            StyleFilterButton(_incButton, _filter == "INC");
            StyleFilterButton(_woButton, _filter == "WO");
        }

        private static void StyleFilterButton(Button button, bool active)
        {
            button.BackColor = active ? Color.FromArgb(37, 99, 235) : Color.White;
            button.ForeColor = active ? Color.White : Color.FromArgb(51, 65, 85);
            button.FlatAppearance.BorderColor = active ? Color.FromArgb(37, 99, 235) : Color.FromArgb(203, 213, 225);
        }

        private void RenderTickets()
        {
            _ticketList.SuspendLayout();
            try
            {
                while (_ticketList.Controls.Count > 0)
                {
                    Control previous = _ticketList.Controls[0];
                    _ticketList.Controls.RemoveAt(0);
                    previous.Dispose();
                }
                _totalValue.Text = _tickets.Count.ToString();
                _incValue.Text = _tickets.Count(ticket => ticket.Type == "INC").ToString();
                _woValue.Text = _tickets.Count(ticket => ticket.Type == "WO").ToString();

                IEnumerable<Ticket> visible = _filter == "ALL"
                    ? _tickets
                    : _tickets.Where(ticket => ticket.Type == _filter);

                if (!visible.Any())
                {
                    if (!_databaseConfigured)
                    {
                        var configureButton = new Button
                        {
                            Text = "配置 PostgreSQL 数据库",
                            Width = 280,
                            Height = 42,
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(37, 99, 235),
                            ForeColor = Color.White,
                            Margin = new Padding(0, 24, 0, 0)
                        };
                        configureButton.Click += (sender, args) => OpenSettingsDialog();
                        _ticketList.Controls.Add(configureButton);
                    }
                    else
                    {
                        _ticketList.Controls.Add(new Label
                        {
                            Text = _tickets.Count == 0 ? "当前没有可显示的未分配工单" : "当前筛选下没有工单",
                            Width = 280,
                            Height = 90,
                            TextAlign = ContentAlignment.MiddleCenter,
                            ForeColor = Color.FromArgb(100, 116, 139),
                            Margin = new Padding(0, 20, 0, 0)
                        });
                    }
                }
                else
                {
                    foreach (Ticket ticket in visible)
                    {
                        _ticketList.Controls.Add(new TicketCardControl(ticket));
                    }
                }

                ResizeTicketCards();
            }
            finally
            {
                _ticketList.ResumeLayout();
            }
        }

        private void ResizeTicketCards()
        {
            int width = Math.Max(250, _ticketList.ClientSize.Width - _ticketList.Padding.Horizontal - 4);
            foreach (Control control in _ticketList.Controls)
            {
                control.Width = width;
            }
        }
    }
}
