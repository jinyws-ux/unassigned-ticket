namespace UnassignedTicket.OutlookAddIn.Data
{
    internal sealed class DatabaseSettings
    {
        public string ProviderInvariantName { get; set; }
        public string ConnectionString { get; set; }
        public int CommandTimeoutSeconds { get; set; } = 15;

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ProviderInvariantName)
                    && !string.IsNullOrWhiteSpace(ConnectionString);
            }
        }
    }
}
