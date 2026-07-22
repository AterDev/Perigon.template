using Share;

namespace ApiService.Endpoints;

public sealed class SampleEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/samples", "Samples");
        group.MapGet("/{id:int}", GetById).AllowAnonymous();
        group.MapPost(string.Empty, Create).AllowAnonymous();
        group.MapPut("/{id:int}", Update).AllowAnonymous();
        group.MapDelete("/{id:int}", Delete).AllowAnonymous();
    }

    /// <summary>
    /// Returns a sample resource.
    /// </summary>
    /// <param name="id">Sample identifier.</param>
    /// <param name="localizer">Localized message provider.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <response code="200">Returns the sample data.</response>
    /// <response code="400">Returns a sample bad request response.</response>
    public static IResult GetById(int id, Localizer localizer, HttpContext httpContext)
    {
        if (id <= 0)
        {
            return BadRequest(localizer, httpContext, "sample.id.invalid");
        }

        return Results.Ok(new SampleResponse
        {
            Id = id,
            Name = $"sample-{id}",
            Description = "GET request succeeded.",
            Method = HttpMethods.Get,
        });
    }

    /// <summary>
    /// Creates a sample resource.
    /// </summary>
    /// <param name="request">Sample creation payload.</param>
    /// <param name="localizer">Localized message provider.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <response code="200">Returns the created sample data.</response>
    /// <response code="400">Returns a sample bad request response.</response>
    public static IResult Create(SampleUpsertRequest request, Localizer localizer, HttpContext httpContext)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(localizer, httpContext, "sample.name.required");
        }

        return Results.Ok(new SampleResponse
        {
            Id = 1,
            Name = request.Name.Trim(),
            Description = request.Description,
            Method = HttpMethods.Post,
        });
    }

    /// <summary>
    /// Updates a sample resource.
    /// </summary>
    /// <param name="id">Sample identifier.</param>
    /// <param name="request">Sample update payload.</param>
    /// <param name="mode">Use `problem` to return Problem or `throw` to raise an exception.</param>
    /// <param name="localizer">Localized message provider.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <response code="200">Returns the updated sample data.</response>
    /// <response code="500">Returns a sample problem response or throws an exception.</response>
    public static IResult Update(
        int id,
        SampleUpsertRequest request,
        string? mode,
        Localizer localizer,
        HttpContext httpContext)
    {
        if (string.Equals(mode, "problem", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(localizer, httpContext, "sample.problem", 5001);
        }

        if (string.Equals(mode, "throw", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Sample endpoint threw an exception.");
        }

        return Results.Ok(new SampleResponse
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(request.Name) ? $"sample-{id}" : request.Name.Trim(),
            Description = request.Description ?? "PUT request succeeded.",
            Method = HttpMethods.Put,
        });
    }

    /// <summary>
    /// Deletes a sample resource.
    /// </summary>
    /// <param name="id">Sample identifier.</param>
    /// <param name="force">Set to true to simulate a successful delete.</param>
    /// <param name="localizer">Localized message provider.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <response code="204">Deletes the sample successfully.</response>
    /// <response code="400">Returns a sample bad request response.</response>
    public static IResult Delete(int id, bool force, Localizer localizer, HttpContext httpContext)
    {
        if (!force)
        {
            return BadRequest(localizer, httpContext, "sample.delete.forceRequired");
        }

        return Results.NoContent();
    }
}

public sealed class SampleUpsertRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}

public sealed class SampleResponse
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Method { get; set; }
}