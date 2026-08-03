using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using UnassignedTicket.OutlookAddIn.Data;

namespace UnassignedTicket.OutlookAddIn.UI
{
    internal sealed class ConnectionSettingsForm : Form
    {
        private readonly TextBox _connectionTextBox;
        private readonly NumericUpDown _timeoutInput;
        private readonly Button _testButton;
        private readonly Button _saveButton;

        internal ConnectionSettingsForm(DatabaseSettings settings)
        {
            Text = "数据库设置";
            Width = 620;
            Height = 390;
            MinimumSize = new Size(560, 340);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            var connectionLabel = NewLabel("PostgreSQL 连接字符串（只保存在本机，并使用当前 Windows 用户加密）", 20, 20, 560);
            _connectionTextBox = new TextBox
            {
                Left = 20,
                Top = 48,
                Width = 560,
                Height = 150,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Text = settings?.ConnectionString ?? string.Empty
            };

            var timeoutLabel = NewLabel("查询超时（秒）", 20, 218, 120);
            _timeoutInput = new NumericUpDown
            {
                Left = 145,
                Top = 215,
                Width = 80,
                Minimum = 3,
                Maximum = 120,
                Value = Math.Max(3, Math.Min(settings?.CommandTimeoutSeconds ?? 15, 120))
            };

            var warning = NewLabel("账号应只授予目标表或视图的 SELECT 权限；如果服务器支持，建议启用 SSL。", 20, 258, 560);
            warning.ForeColor = Color.FromArgb(146, 64, 14);

            _testButton = new Button
            {
                Text = "测试连接",
                Left = 350,
                Top = 310,
                Width = 105,
                Height = 32,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _testButton.Click += TestConnectionAsync;

            _saveButton = new Button
            {
                Text = "保存",
                Left = 465,
                Top = 310,
                Width = 115,
                Height = 32,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _saveButton.Click += SaveAndClose;

            Controls.AddRange(new Control[]
            {
                connectionLabel, _connectionTextBox,
                timeoutLabel, _timeoutInput, warning, _testButton, _saveButton
            });
            AcceptButton = _saveButton;
        }

        internal DatabaseSettings Result { get; private set; }

        private static Label NewLabel(string text, int left, int top, int width)
        {
            return new Label { Text = text, Left = left, Top = top, Width = width, Height = 22 };
        }

        private DatabaseSettings ReadSettings()
        {
            return new DatabaseSettings
            {
                ConnectionString = _connectionTextBox.Text.Trim(),
                CommandTimeoutSeconds = (int)_timeoutInput.Value
            };
        }

        private async void TestConnectionAsync(object sender, EventArgs e)
        {
            DatabaseSettings settings = ReadSettings();
            if (!settings.IsConfigured)
            {
                MessageBox.Show("请先填写 PostgreSQL 连接字符串。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _testButton.Enabled = false;
            _testButton.Text = "连接中…";
            try
            {
                var repository = new DbTicketRepository(settings);
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(settings.CommandTimeoutSeconds)))
                {
                    await repository.TestConnectionAsync(timeout.Token);
                }
                MessageBox.Show("数据库连接成功。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败：" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _testButton.Enabled = true;
                _testButton.Text = "测试连接";
            }
        }

        private void SaveAndClose(object sender, EventArgs e)
        {
            DatabaseSettings settings = ReadSettings();
            if (!settings.IsConfigured)
            {
                MessageBox.Show("PostgreSQL 连接字符串不能为空。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Result = settings;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
