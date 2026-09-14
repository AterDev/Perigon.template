using System.Text.Json.Serialization;
using Perigon.AspNetCore.Models;
using ApiService.Endpoints;

namespace ApiService;

[JsonSerializable(typeof(SampleUpsertRequest))]
[JsonSerializable(typeof(SampleResponse))]
[JsonSerializable(typeof(ErrorResult))]
public partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
