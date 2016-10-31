using Nancy.Json.Simple;

namespace REstate.Web
{
    public class SimpleJsonSerializer : StringSerializer
    {

        public SimpleJsonSerializer()
        {
            SimpleJson.CurrentJsonSerializerStrategy = new NancySerializationStrategy();
        }

        public override string Serialize(object o)
        {
            return SimpleJson.SerializeObject(o);
        }

        public override T Deserialize<T>(string value)
        {
            return SimpleJson.DeserializeObject<T>(value);
        }
    }
}
