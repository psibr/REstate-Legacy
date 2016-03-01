using Newtonsoft.Json;

namespace REstate.Services.JsonNet
{
    public class NewtonsoftJsonSerializer
        : IJsonSerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload);
        }
    }
}
