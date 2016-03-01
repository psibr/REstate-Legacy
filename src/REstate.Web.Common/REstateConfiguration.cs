namespace REstate.Web
{
    public class REstateConfiguration
    {
        public string EncryptionPassphrase { get; set; }

        public string HmacPassphrase { get; set; }

        public string AuthBaseUrl { get; set; } = string.Empty;

        public string ClaimsPrincipalResourceName { get; set; } = "server.User";

        public byte[] HmacSaltBytes { get; set; }

        public byte[] EncryptionSaltBytes { get; set; }
    }
}