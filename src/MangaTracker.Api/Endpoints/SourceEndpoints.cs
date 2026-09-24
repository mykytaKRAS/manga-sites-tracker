using MangaTracker.Api.Contracts;
using MangaTracker.Core.Entities;
using MangaTracker.Core.Exceptions;
using MangaTracker.Core.Interfaces;
using MangaTracker.Core.Services;

namespace MangaTracker.Api.Endpoints;

public static class SourceEndpoints
{
    public static void MapSourceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sources")
            .WithTags("Sources");

        group.MapGet("/", async (SourceService service, CancellationToken ct) =>
        {
            var sources = await service.GetAllAsync(ct);
            return Results.Ok(sources.Select(ToResponse));
        });

        group.MapGet("/{id:int}", async (int id, SourceService service, CancellationToken ct) =>
        {
            var source = await service.GetByIdAsync(id, ct);
            return source is null ? Results.NotFound() : Results.Ok(ToResponse(source));
        });

        group.MapPost("/", async (
            CreateSourceRequest request,
            SourceService service,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Name is required."]
                });
            }

            if (!Enum.IsDefined(request.Kind))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["kind"] = ["Unknown source kind."]
                });
            }

            if (!Enum.IsDefined(request.NumberSource))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["numberSource"] = ["Unknown chapter number source."]
                });
            }

            try
            {
                var source = await service.AddAsync(
                    request.Name,
                    request.Kind,
                    request.ChapterLinkSelector,
                    request.ChapterNumberPattern,
                    request.NumberSource,
                    ct);

                return Results.Created($"/api/sources/{source.Id}", ToResponse(source));
            }
            catch (DuplicateSourceException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (InvalidSourceException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["rules"] = [ex.Message]
                });
            }
        });

        group.MapPut("/{id:int}", async (
            int id,
            UpdateSourceRequest request,
            SourceService service,
            CancellationToken ct) =>
        {
            if (!Enum.IsDefined(request.NumberSource))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["numberSource"] = ["Unknown chapter number source."]
                });
            }

            try
            {
                var source = await service.UpdateAsync(
                    id,
                    request.ChapterLinkSelector,
                    request.ChapterNumberPattern,
                    request.NumberSource,
                    ct);

                return source is null ? Results.NotFound() : Results.Ok(ToResponse(source));
            }
            catch (InvalidSourceException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["rules"] = [ex.Message]
                });
            }
        });

        group.MapPost("/{id:int}/enabled", async (
            int id,
            SetSourceEnabledRequest request,
            SourceService service,
            CancellationToken ct) =>
        {
            var updated = await service.SetEnabledAsync(id, request.IsEnabled, request.Reason, ct);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, SourceService service, CancellationToken ct) =>
        {
            try
            {
                var deleted = await service.DeleteAsync(id, ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (SourceInUseException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapPost("/test", async (
            TestSourceRequest request,
            IHttpClientFactory httpClientFactory,
            IHtmlChapterParser parser,
            CancellationToken ct) =>
        {
            if (!Uri.TryCreate(request.PageUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["pageUrl"] = ["A valid absolute http(s) URL is required."]
                });
            }

            if (string.IsNullOrWhiteSpace(request.Selector))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["selector"] = ["Selector is required."]
                });
            }

            var client = httpClientFactory.CreateClient("html-source");

            try
            {
                using var response = await client.GetAsync(request.PageUrl, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return Results.Ok(TestResult.Fail(
                        $"Site returned {(int)response.StatusCode} {response.ReasonPhrase}"));
                }

                var html = await response.Content.ReadAsStringAsync(ct);

                var rules = new ChapterExtractionRules(
                    request.Selector,
                    string.IsNullOrWhiteSpace(request.NumberPattern) ? null : request.NumberPattern,
                    request.NumberSource);

                var chapters = await parser.ParseAsync(html, request.PageUrl, rules, ct);

                if (chapters.Count == 0)
                {
                    return Results.Ok(TestResult.Fail(
                        "Selector matched nothing, or chapter numbers could not be parsed."));
                }

                var preview = chapters
                    .OrderByDescending(c => c.Number)
                    .Take(5)
                    .Select(c => new TestChapterPreview(c.Number, c.Url))
                    .ToList();

                return Results.Ok(new TestSourceResponse(
                    true,
                    $"Found {chapters.Count} chapters.",
                    preview));
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                return Results.Ok(TestResult.Fail("Request timed out."));
            }
            catch (HttpRequestException ex)
            {
                return Results.Ok(TestResult.Fail($"Could not reach the site: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return Results.Ok(TestResult.Fail($"Parse error: {ex.Message}"));
            }
        });
    }

    private static SourceResponse ToResponse(MangaSource source) => new(
        source.Id,
        source.Name,
        source.Kind.ToString(),
        source.ChapterLinkSelector,
        source.ChapterNumberPattern,
        source.NumberSource.ToString(),
        source.IsEnabled,
        source.DisabledReason);

    private static class TestResult
    {
        public static TestSourceResponse Fail(string message)
            => new(false, message, []);
    }
}