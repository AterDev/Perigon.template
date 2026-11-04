using System.Text.Json.Serialization;
using Perigon.AspNetCore.Models;
using ApiService.Endpoints;

namespace ApiService;

[JsonSerializable(typeof(SampleUpsertRequest))]
[JsonSerializable(typeof(SampleResponse))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(ErrorResult))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(bool?))]
public partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
