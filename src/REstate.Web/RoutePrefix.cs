namespace Psibr.Platform.Nancy
{
    public abstract class RoutePrefix
    {
        protected string Prefix { get; }

        protected RoutePrefix(string prefix)
        {
            Prefix = prefix;
        }

        public override string ToString()
        {
            return Prefix;
        }

        public static implicit operator string(RoutePrefix prefix)
        {
            return prefix.ToString();
        }
    }
}
