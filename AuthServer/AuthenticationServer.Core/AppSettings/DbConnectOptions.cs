namespace AuthenticationServer.Core.AppSettings
{
    public sealed class DbConnectOptions
    {
        public static string ConfigSectionPath => "DbConnectOptions";
        public string ConnectionString { get; set; }
    }
}
