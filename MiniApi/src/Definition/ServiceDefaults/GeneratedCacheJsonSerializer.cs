using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Caching.Hybrid;

namespace ServiceDefaults;

/// <summary>Serializes a registered cache type with source-generated JSON metadata.</summary>
/// <remarks>Use default JSON options, not HTTP JSON options, to preserve cache payloads.</remarks>
public sealed class GeneratedCacheJsonSerializer<T>(JsonTypeInfo<T> typeInfo) : IHybridCacheSerializer<T>
{
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        var reader = new Utf8JsonReader(source);
        return JsonSerializer.Deserialize(ref reader, typeInfo)!;
    }

    public void Serialize(T value, IBufferWriter<byte> target)
    {
        using var writer = new Utf8JsonWriter(target);
        JsonSerializer.Serialize(writer, value, typeInfo);
    }
}

// Add application cache types here and register their serializers in AddCache.
// Keep these options independent of the web serializer's naming and encoder settings.
[JsonSerializable(typeof(string[]))]
internal partial class CacheJsonContext : JsonSerializerContext
{
}
