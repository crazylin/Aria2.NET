using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aria2NET;

[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(RequestResult<string>))]
[JsonSerializable(typeof(RequestResult<List<string>>))]
[JsonSerializable(typeof(RequestResult<VersionResult>))]
[JsonSerializable(typeof(RequestResult<DownloadStatusResult>))]
[JsonSerializable(typeof(RequestResult<List<UriResult>>))]
[JsonSerializable(typeof(RequestResult<List<FileResult>>))]
[JsonSerializable(typeof(RequestResult<List<PeerResult>>))]
[JsonSerializable(typeof(RequestResult<List<ServerResult>>))]
[JsonSerializable(typeof(RequestResult<int>))]
[JsonSerializable(typeof(RequestResult<Dictionary<string, string>>))]
[JsonSerializable(typeof(RequestResult<IDictionary<string, string>>))]
[JsonSerializable(typeof(RequestResult<SessionResult>))]
[JsonSerializable(typeof(RequestResult<GlobalStatResult>))]
[JsonSerializable(typeof(RequestResult<List<List<object>>>))]
[JsonSerializable(typeof(RequestResult<RequestError>))]
[JsonSerializable(typeof(MulticallRequest))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(object))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal partial class Aria2NetJsonSerializerContext : JsonSerializerContext
{
} 