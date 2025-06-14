using System.Text.Json.Serialization;

namespace Aria2NET;

public class RequestResult<T>
{
    [JsonPropertyName("id")]
    public String Id { get; set; } = null!;

    [JsonPropertyName("jsonrpc")]
    public String Jsonrpc { get; set; } = null!;

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("error")]
    public RequestError? Error { get; set; }
}