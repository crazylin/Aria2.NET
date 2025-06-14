using System.Text.Json.Serialization;

namespace Aria2NET;

public class UriResult
{
    /// <summary>
    ///     'used' if the URI is in use. 'waiting' if the URI is still waiting in the queue.
    /// </summary>
    [JsonPropertyName("status")]
    public String Status { get; set; } = null!;

    /// <summary>
    ///     URI
    /// </summary>
    [JsonPropertyName("uri")]
    public String Uri { get; set; } = null!;
}