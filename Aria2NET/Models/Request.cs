using System.Text.Json.Serialization;

namespace Aria2NET;

public class Request
{
    [JsonPropertyName("id")]
    public String Id { get; set; } = null!;

    [JsonPropertyName("jsonrpc")]
    public String Jsonrpc { get; set; } = null!;

    [JsonPropertyName("method")]
    public String Method { get; set; } = null!;

    [JsonPropertyName("params")]
    public IList<Object?>? Parameters { get; set; }
}

public class MulticallRequest
{
    [JsonPropertyName("methodName")]
    public String MethodName { get; set; } = null!;

    [JsonPropertyName("params")]
    public IList<Object> Parameters { get; set; } = new List<Object>();
}