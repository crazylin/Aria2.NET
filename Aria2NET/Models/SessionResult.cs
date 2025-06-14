using System.Text.Json.Serialization;

namespace Aria2NET;

public class SessionResult
{
    [JsonPropertyName("sessionId")]
    public String SessionId { get; set; } = null!;
}