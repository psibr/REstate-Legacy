namespace REstate.Web
{
    public class RootConfig
    {
        public REstateConfiguration REstateConfiguration { get; set; }
    }

    public class REstateConfiguration
    {
        public ConnectionStrings ConnectionStrings { get; set; }

        public AuthenticationSettings AuthenticationSettings { get; set; }
    }

    public class AuthenticationSettings
    {
        public bool UseAuthentication { get; set; } = true;

        public string ClaimsPrincipalResourceName { get; set; }

        public string CookieName { get; set; }

        public string SchedulerApiKey { get; set; }

        public long TokenLifeSpanMinutes { get; set; } = 30;
    }

    public class ConnectionStrings
    {
        public string EngineConnectionString { get; set; }

        private string _SchedulerConnectionString;
        public string SchedulerConnectionString
        {
            get
            {
                return _SchedulerConnectionString ?? EngineConnectionString;
            }
            set
            {
                _SchedulerConnectionString = value;
            }
        }

        private string _AuthConnectionString;
        public string AuthConnectionString
        {
            get
            {
                return _AuthConnectionString ?? EngineConnectionString;
            }
            set
            {
                _AuthConnectionString = value;
            }
        }

    }
}
