using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Perigon.AspNetCore.Models;

// <perigon-aot-json:usings>
using CommonMod.Models.UserDtos;
// </perigon-aot-json:usings>

namespace ApiService;

// <perigon-aot-json:generated>
[JsonSerializable(typeof(UserInfoDto))]
[JsonSerializable(typeof(UserLoginDto))]
// </perigon-aot-json:generated>
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ErrorResult))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
