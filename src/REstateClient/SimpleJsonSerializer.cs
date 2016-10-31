using REstate;
using REstateClient.Json.Simple;

namespace REstateClient
{
    internal class SimpleJsonSerializer : StringSerializer
    {

        public SimpleJsonSerializer()
        {
            SimpleJson.CurrentJsonSerializerStrategy = new REstateClientSerializationStrategy();

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
