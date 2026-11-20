using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ApiService.Serialization;

/// <summary>Application JSON metadata for non-HTTP payloads.</summary>
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class AppSerializerContext : JsonSerializerContext
{
}
