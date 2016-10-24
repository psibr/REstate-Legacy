using Newtonsoft.Json;

namespace REstate.Web
{
    public class NewtonsoftJsonSerializer : StringSerializer
    {
        private readonly bool _prettify;

        public NewtonsoftJsonSerializer()
        {
        }

        public NewtonsoftJsonSerializer(bool prettify)
        {
            _prettify = prettify;
        }

        public override string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, _prettify ? Formatting.Indented : Formatting.None);
        }

        public override T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
