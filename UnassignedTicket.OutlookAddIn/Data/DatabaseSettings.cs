namespace UnassignedTicket.OutlookAddIn.Data
{
    internal sealed class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public int CommandTimeoutSeconds { get; set; } = 15;

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ConnectionString);
            }
        }
    }
}
