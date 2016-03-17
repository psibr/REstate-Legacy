namespace REstate.Platform
{
    public class HttpServiceAddressConfiguration
    {
        private string _address;

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;

                if (string.IsNullOrWhiteSpace(Binding))
                    Binding = value;
            }
        }

        public string Binding { get; set; }
    }
}