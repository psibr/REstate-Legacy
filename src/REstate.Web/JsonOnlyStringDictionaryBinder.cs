using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace REstate.Web
{
    public class JsonOnlyStringDictionaryBinder
        : IModelBinder
    {
        protected StringSerializer StringSerializer { get; }

        public JsonOnlyStringDictionaryBinder(StringSerializer stringSerializer)
        {
            StringSerializer = stringSerializer;
        }

        public object Bind(NancyContext context, Type modelType, object instance,
            BindingConfig configuration, params string[] blackList)
        {
            var result = (instance as Dictionary<string, string>);

            if (result == null)
            {
                var body = context.Request.Body.AsString();

                result = StringSerializer.Deserialize<Dictionary<string, string>>(body);
            }

            return result ?? new Dictionary<string, string>();
        }

        public bool CanBind(Type modelType)
        {
            if (modelType.GetTypeInfo().IsGenericType && (modelType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                || modelType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                if (modelType.GetGenericArguments()[0] == typeof(string) &&
                    modelType.GetGenericArguments()[1] == typeof(string))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
