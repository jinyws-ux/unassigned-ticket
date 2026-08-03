using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UnassignedTicket.OutlookAddIn.Data
{
    internal sealed class SettingsStore
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ITCC.UnassignedTicket.v1");

        internal string FilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ITCC",
                    "UnassignedTicket",
                    "database.config");
            }
        }

        internal DatabaseSettings Load()
        {
            if (!File.Exists(FilePath))
            {
                return new DatabaseSettings();
            }

            try
            {
                byte[] encrypted = File.ReadAllBytes(FilePath);
                byte[] clear = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
                string[] lines = Encoding.UTF8.GetString(clear).Split(new[] { '\n' }, 3);

                if (lines.Length != 3)
                {
                    throw new InvalidDataException("数据库配置格式无效。");
                }

                int timeout;
                if (!int.TryParse(lines[1], out timeout))
                {
                    timeout = 15;
                }

                return new DatabaseSettings
                {
                    ProviderInvariantName = lines[0].Trim(),
                    CommandTimeoutSeconds = Math.Max(3, Math.Min(timeout, 120)),
                    ConnectionString = lines[2]
                };
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException("数据库配置无法由当前 Windows 用户解密，请重新配置。", ex);
            }
        }

        internal void Save(DatabaseSettings settings)
        {
            if (settings == null || !settings.IsConfigured)
            {
                throw new ArgumentException("Provider 和连接字符串不能为空。", nameof(settings));
            }

            string directory = Path.GetDirectoryName(FilePath);
            Directory.CreateDirectory(directory);

            string serialized = string.Join("\n",
                settings.ProviderInvariantName.Trim(),
                settings.CommandTimeoutSeconds.ToString(),
                settings.ConnectionString.Trim());

            byte[] clear = Encoding.UTF8.GetBytes(serialized);
            byte[] encrypted = ProtectedData.Protect(clear, Entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(FilePath, encrypted);
        }
    }
}
