using System.Text.Json.Serialization;

namespace Aria2NET;

public class ServerResult
{
    [JsonPropertyName("index")]
    public String Index { get; set; } = null!;

    [JsonPropertyName("servers")]
    public List<ServerResultServer> Servers { get; set; } = new List<ServerResultServer>();
}

public class ServerResultServer
{
    [JsonPropertyName("currentUri")]
    public String CurrentUri { get; set; } = null!;

    [JsonPropertyName("downloadSpeed")]
    public Decimal DownloadSpeed { get; set; }

    [JsonPropertyName("uri")]
    public String Uri { get; set; } = null!;
}