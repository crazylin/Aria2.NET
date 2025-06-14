using System.Text.Json.Serialization;

namespace Aria2NET;

public class RequestError
{
    [JsonPropertyName("code")]
    public Int64 Code { get; set; }

    [JsonPropertyName("message")]
    public String? Message { get; set; }
}