using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using UnassignedTicket.OutlookAddIn.Models;

namespace UnassignedTicket.OutlookAddIn.Data
{
    internal sealed class DbTicketRepository : ITicketRepository
    {
        private readonly DatabaseSettings _settings;

        internal DbTicketRepository(DatabaseSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task TestConnectionAsync(CancellationToken cancellationToken)
        {
            using (NpgsqlConnection connection = CreateConnection())
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<Ticket>> GetUnassignedTicketsAsync(CancellationToken cancellationToken)
        {
            if (UnassignedTicketQuery.IsPlaceholder)
            {
                throw new InvalidOperationException("查询 SQL 尚未配置，请先替换 UnassignedTicketQuery.cs 中的示例查询。");
            }

            var tickets = new List<Ticket>();
            using (NpgsqlConnection connection = CreateConnection())
            using (NpgsqlCommand command = connection.CreateCommand())
            {
                command.CommandText = UnassignedTicketQuery.Sql;
                command.CommandTimeout = _settings.CommandTimeoutSeconds;

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        Ticket ticket = MapTicket(reader);
                        if (!string.IsNullOrWhiteSpace(ticket.Id))
                        {
                            tickets.Add(ticket);
                        }
                    }
                }
            }

            tickets.Sort((left, right) => right.WaitingTime.CompareTo(left.WaitingTime));
            return tickets;
        }

        private NpgsqlConnection CreateConnection()
        {
            if (!_settings.IsConfigured)
            {
                throw new InvalidOperationException("数据库尚未配置。");
            }

            return new NpgsqlConnection(_settings.ConnectionString);
        }

        private static Ticket MapTicket(DbDataReader reader)
        {
            string id = ReadString(reader, "TICKET_ID");
            string type = ReadString(reader, "TICKET_TYPE");

            return new Ticket
            {
                Id = id,
                Type = NormalizeTicketType(id, type),
                Summary = ReadString(reader, "SUMMARY") ?? "（无摘要）",
                Priority = ReadString(reader, "PRIORITY"),
                Status = ReadString(reader, "STATUS"),
                CreatedAt = ReadDateTime(reader, "CREATED_AT") ?? DateTime.Now,
                GroupAssignedAt = ReadDateTime(reader, "GROUP_ASSIGNED_AT"),
                Url = ReadString(reader, "TICKET_URL")
            };
        }

        private static string NormalizeTicketType(string id, string value)
        {
            string normalized = (value ?? string.Empty)
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty);

            if (string.Equals(normalized, "Incident", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "INC", StringComparison.OrdinalIgnoreCase))
            {
                return "INC";
            }

            if (string.Equals(normalized, "WorkOrder", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "WO", StringComparison.OrdinalIgnoreCase))
            {
                return "WO";
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                if (id.StartsWith("WO", StringComparison.OrdinalIgnoreCase)) return "WO";
                if (id.StartsWith("INC", StringComparison.OrdinalIgnoreCase)) return "INC";
            }

            return normalized.ToUpperInvariant();
        }

        private static int FindOrdinal(DbDataReader reader, string name)
        {
            for (int index = 0; index < reader.FieldCount; index++)
            {
                if (string.Equals(reader.GetName(index), name, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string ReadString(DbDataReader reader, string name)
        {
            int ordinal = FindOrdinal(reader, name);
            return ordinal < 0 || reader.IsDBNull(ordinal)
                ? null
                : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private static DateTime? ReadDateTime(DbDataReader reader, string name)
        {
            int ordinal = FindOrdinal(reader, name);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
            {
                return null;
            }

            object value = reader.GetValue(ordinal);
            if (value is DateTime)
            {
                return (DateTime)value;
            }

            DateTime parsed;
            return DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out parsed)
                ? parsed
                : (DateTime?)null;
        }
    }
}
