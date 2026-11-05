using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Perigon.AspNetCore.Models;

/// <summary>Standard problem details with field-level errors when validation fails.</summary>
public sealed class ErrorResult : HttpValidationProblemDetails
{
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}
