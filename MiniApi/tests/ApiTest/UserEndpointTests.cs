using System.Text;
using System.Text.Json.Nodes;

namespace ApiTest;

public class UserEndpointTests
{
    [Test]
    [Category("Integration")]
    public async Task Login_ExposesUserRouteAndOpenApiContract()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        await GlobalHooks.NotificationService!.WaitForResourceHealthyAsync("ApiService", timeout.Token);
        using var client = GlobalHooks.App!.CreateHttpClient("ApiService");

        using var swagger = await client.GetAsync("/swagger/index.html", timeout.Token);
        await Assert.That(swagger.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var swaggerHtml = await swagger.Content.ReadAsStringAsync(timeout.Token);
        await Assert.That(swaggerHtml.Contains("swagger-ui", StringComparison.OrdinalIgnoreCase)).IsTrue();
        using var swaggerScript = await client.GetAsync("/swagger/swagger-ui-bundle.js", timeout.Token);
        await Assert.That(swaggerScript.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var swaggerConfig = await client.GetStringAsync("/swagger/index.js", timeout.Token);
        await Assert.That(swaggerConfig.Contains("../openapi/v1.json", StringComparison.Ordinal)).IsTrue();

        using var invalidLogin = new StringContent(
            "{\"userName\":\"\",\"password\":\"\"}", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/users/login", invalidLogin, timeout.Token);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(response.Content.Headers.ContentType?.MediaType)
            .IsEqualTo("application/problem+json");
        var error = JsonNode.Parse(await response.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(error["status"]!.GetValue<int>()).IsEqualTo(400);
        await Assert.That(error["traceId"]).IsNotNull();
        await Assert.That(error["errors"]!["UserName"]).IsNotNull();
        await Assert.That(error["errors"]!["Password"]).IsNotNull();

        using var tooLongLogin = new StringContent(
            $"{{\"userName\":\"{new string('u', 51)}\",\"password\":\"123456\"}}",
            Encoding.UTF8,
            "application/json");
        using var tooLongResponse = await client.PostAsync("/api/users/login", tooLongLogin, timeout.Token);
        await Assert.That(tooLongResponse.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(tooLongResponse.Content.Headers.ContentType?.MediaType)
            .IsEqualTo("application/problem+json");
        var tooLongError = JsonNode.Parse(await tooLongResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(tooLongError["errors"]!["UserName"]).IsNotNull();

        using var invalidPassword = new StringContent(
            "{\"userName\":\"user\",\"password\":\"wrong-password\"}",
            Encoding.UTF8,
            "application/json");
        using var invalidPasswordResponse = await client.PostAsync(
            "/api/users/login",
            invalidPassword,
            timeout.Token);
        await Assert.That(invalidPasswordResponse.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(invalidPasswordResponse.Content.Headers.ContentType?.MediaType)
            .IsEqualTo("application/problem+json");
        var businessError = JsonNode.Parse(
            await invalidPasswordResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(businessError["detail"]!.GetValue<string>()).IsEqualTo("密码错误");
        await Assert.That(businessError["errors"]!.AsObject().Count).IsEqualTo(0);

        using var unknownUser = new HttpRequestMessage(HttpMethod.Post, "/api/users/login")
        {
            Content = new StringContent("{\"userName\":\"not-existing-user\",\"password\":\"123456\"}", Encoding.UTF8, "application/json"),
        };
        unknownUser.Headers.AcceptLanguage.ParseAdd("en-US");
        using var unknownResponse = await client.SendAsync(unknownUser, timeout.Token);
        await Assert.That(unknownResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        var unknownError = JsonNode.Parse(await unknownResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(unknownError["detail"]!.GetValue<string>()).IsEqualTo("Not Found User");
        await Assert.That(unknownError["perigon.localizationArguments"]).IsNull();

        using var plainAccept = new HttpRequestMessage(HttpMethod.Post, "/api/users/login")
        {
            Content = new StringContent("{\"userName\":\"user\",\"password\":\"wrong-password\"}", Encoding.UTF8, "application/json"),
        };
        plainAccept.Headers.Accept.ParseAdd("text/plain");
        using var plainAcceptResponse = await client.SendAsync(plainAccept, timeout.Token);
        await Assert.That(plainAcceptResponse.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(plainAcceptResponse.Content.Headers.ContentType?.MediaType).IsEqualTo("application/problem+json");
        var plainAcceptError = JsonNode.Parse(await plainAcceptResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(plainAcceptError["traceId"]).IsNotNull();
        await Assert.That(plainAcceptError["errors"]!.AsObject().Count).IsEqualTo(0);

        using var validLogin = new StringContent(
            "{\"userName\":\"user\",\"password\":\"123456\"}",
            Encoding.UTF8,
            "application/json");
        using var validResponse = await client.PostAsync("/api/users/login", validLogin, timeout.Token);
        await Assert.That(validResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var userInfo = JsonNode.Parse(await validResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(userInfo["userName"]!.GetValue<string>()).IsEqualTo("user");

        using var missingResponse = await client.GetAsync("/__missing_route", timeout.Token);
        await Assert.That(missingResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(missingResponse.Content.Headers.ContentType?.MediaType).IsEqualTo("application/problem+json");
        var missingError = JsonNode.Parse(await missingResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(missingError["errors"]!.AsObject().Count).IsEqualTo(0);
        await Assert.That(missingError["traceId"]).IsNotNull();

        using var missingPlainRequest = new HttpRequestMessage(HttpMethod.Get, "/__missing_route");
        missingPlainRequest.Headers.Accept.ParseAdd("text/plain");
        using var missingPlainResponse = await client.SendAsync(missingPlainRequest, timeout.Token);
        await Assert.That(missingPlainResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        var missingPlainError = JsonNode.Parse(await missingPlainResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(missingPlainError["errors"]!.AsObject().Count).IsEqualTo(0);
        await Assert.That(missingPlainError["traceId"]).IsNotNull();

        using var malformedLogin = new StringContent("{", Encoding.UTF8, "application/json");
        using var malformedResponse = await client.PostAsync("/api/users/login", malformedLogin, timeout.Token);
        await Assert.That(malformedResponse.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(malformedResponse.Content.Headers.ContentType?.MediaType).IsEqualTo("application/problem+json");
        var malformedError = JsonNode.Parse(await malformedResponse.Content.ReadAsStringAsync(timeout.Token))!;
        await Assert.That(malformedError["status"]!.GetValue<int>()).IsEqualTo(400);
        await Assert.That(malformedError["errors"]!.AsObject().Count).IsEqualTo(0);

        var json = await client.GetStringAsync("/openapi/v1.json", timeout.Token);
        var document = JsonNode.Parse(json)!;
        var paths = document["paths"]!;
        var operation = paths["/api/users/login"]!["post"]!;
        await Assert.That(operation["summary"]!.GetValue<string>()).IsEqualTo("用户登录");
        await Assert.That(operation["tags"]![0]!.GetValue<string>()).IsEqualTo("User");
        var requestBody = operation["requestBody"]!;
        await Assert.That(requestBody["description"]).IsNull();
        await Assert.That(requestBody["content"]!["application/json"]!["schema"]!["$ref"]!.GetValue<string>())
            .IsEqualTo("#/components/schemas/UserLoginDto");
        var requestSchema = document["components"]!["schemas"]!["UserLoginDto"]!;
        await Assert.That(requestSchema["required"]!.AsArray()
            .Any(item => item!.GetValue<string>() == "userName")).IsTrue();
        await Assert.That(requestSchema["required"]!.AsArray()
            .Any(item => item!.GetValue<string>() == "password")).IsTrue();
        await Assert.That(operation["responses"]!["200"]!["content"]!["application/json"]!["schema"]!["$ref"]!.GetValue<string>())
            .IsEqualTo("#/components/schemas/UserInfoDto");
        await Assert.That(operation["responses"]!["400"]!["content"]!["application/problem+json"]!["schema"]!["$ref"]!.GetValue<string>())
            .IsEqualTo("#/components/schemas/ErrorResult");
        foreach (var status in new[] { "403", "404" })
        {
            await Assert.That(operation["responses"]![status]!["content"]!["application/problem+json"]!["schema"]!["$ref"]!.GetValue<string>())
                .IsEqualTo("#/components/schemas/ErrorResult");
        }
        var errorSchema = document["components"]!["schemas"]!["ErrorResult"]!;
        await Assert.That(errorSchema["properties"]!["errors"]).IsNotNull();
        await Assert.That(errorSchema["properties"]!["traceId"]).IsNotNull();
        await Assert.That(paths["/api/auth/login"]).IsNull();
    }
}
