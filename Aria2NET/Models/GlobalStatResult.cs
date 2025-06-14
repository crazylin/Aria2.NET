using System.Text.Json.Serialization;

namespace Aria2NET;

public class GlobalStatResult
{
    [JsonPropertyName("downloadSpeed")]
    public Decimal DownloadSpeed { get; set; }

    [JsonPropertyName("numActive")]
    public Int32 NumActive { get; set; }

    [JsonPropertyName("numStopped")]
    public Int32 NumStopped { get; set; }

    [JsonPropertyName("numStoppedTotal")]
    public Int32 NumStoppedTotal { get; set; }

    [JsonPropertyName("numWaiting")]
    public Int32 NumWaiting { get; set; }

    [JsonPropertyName("uploadSpeed")]
    public Decimal UploadSpeed { get; set; }
}