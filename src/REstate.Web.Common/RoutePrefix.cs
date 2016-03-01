using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Web
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

    public class AuthRoutePrefix : RoutePrefix
    {
        public AuthRoutePrefix(string prefix) : base(prefix)
        {
        }
    }

    public class InstancesRoutePrefix : RoutePrefix
    {
        public InstancesRoutePrefix(string prefix) : base(prefix)
        {
        }
    }

    public class ConfigurationRoutePrefix
        : RoutePrefix
    {
        public ConfigurationRoutePrefix(string prefix) 
            : base(prefix)
        {
        }
    }
}
