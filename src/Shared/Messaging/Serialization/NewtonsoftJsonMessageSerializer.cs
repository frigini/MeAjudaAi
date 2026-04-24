using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MeAjudaAi.Shared.Messaging.Serialization;

public sealed class NewtonsoftJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, Settings);

    public T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);
}
