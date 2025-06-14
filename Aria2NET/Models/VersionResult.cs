using System.Text.Json.Serialization;

namespace Aria2NET;

public class VersionResult
{
    /// <summary>
    /// List of enabled features. Each feature is given as a string.
    /// </summary>
    [JsonPropertyName("enabledFeatures")]
    public List<String> EnabledFeatures { get; set; } = new List<String>();

    /// <summary>
    /// Version number of aria2 as a string.
    /// </summary>
    [JsonPropertyName("version")]
    public String Version { get; set; } = null!;
}