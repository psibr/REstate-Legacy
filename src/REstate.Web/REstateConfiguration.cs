namespace REstate.Web
{
    public class RootConfig
    {
        public REstateConfiguration REstateConfiguration { get; set; }
    }

    public class REstateConfiguration
    {
        public ConnectionStrings ConnectionStrings { get; set; }

        public AuthenticationSettings Authentication { get; set; }
    }

    public class AuthenticationSettings
    {
        public bool UseAuthentication { get; set; } = true;

        public string ClaimsPrincipalResourceName { get; set; }

        public string CookieName { get; set; }

        public string SchedulerApiKey { get; set; }

        public long TokenLifeSpanMinutes { get; set; } = 30;

        public TokenEncryptionCertificateSettings TokenEncryptionCertificate { get; set; }
    }

    public class TokenEncryptionCertificateSettings
    {
        public string FileName { get; set; }

        public string PrivateKeyPassword { get; set; }
    }

    public class ConnectionStrings
    {
        public string Engine { get; set; }

        private string _Scheduler;
        public string Scheduler
        {
            get
            {
                return _Scheduler ?? Engine;
            }
            set
            {
                _Scheduler = value;
            }
        }

        private string _Auth;
        public string Auth
        {
            get
            {
                return _Auth ?? Engine;
            }
            set
            {
                _Auth = value;
            }
        }

    }
}
