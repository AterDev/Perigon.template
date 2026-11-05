using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Perigon.AspNetCore.Models;
using CommonMod.Models.UserDtos;

namespace ApiService;

[JsonSerializable(typeof(UserLoginDto))]
[JsonSerializable(typeof(UserInfoDto))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ErrorResult))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(bool?))]
public partial class ApiJsonSerializerContext : JsonSerializerContext
{
}
