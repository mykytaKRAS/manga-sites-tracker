using MangaTracker.Api.Contracts;
using MangaTracker.Core.Entities;
using MangaTracker.Core.Exceptions;
using MangaTracker.Core.Services;

namespace MangaTracker.Api.Endpoints;

public static class MangaEndpoints
{
    public static void MapMangaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/manga")
            .WithTags("Manga");

        group.MapGet("/", async (MangaService service, CancellationToken ct) =>
        {
            var mangas = await service.GetAllAsync(ct);
            return Results.Ok(mangas.Select(ToResponse));
        });

        group.MapGet("/{id:int}", async (int id, MangaService service, CancellationToken ct) =>
        {
            var manga = await service.GetByIdAsync(id, ct);
            return manga is null
                ? Results.NotFound()
                : Results.Ok(ToResponse(manga));
        });

        group.MapPost("/", async (CreateMangaRequest request, MangaService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["title"] = ["Title is required."]
                });
            }

            if (!Uri.TryCreate(request.SourceUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["sourceUrl"] = ["A valid absolute http(s) URL is required."]
                });
            }

            if (!Enum.IsDefined(request.Source))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["source"] = ["Unknown manga source."]
                });
            }

            try
            {
                var manga = await service.AddAsync(request.Title, request.Source, request.SourceUrl, ct);
                return Results.Created($"/api/manga/{manga.Id}", ToResponse(manga));
            }
            catch (DuplicateMangaException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        group.MapPost("/{id:int}/mark-as-read", async (int id, MangaService service, CancellationToken ct) =>
        {
            var updated = await service.MarkAsReadAsync(id, ct);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, MangaService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    private static MangaResponse ToResponse(Manga manga) => new(
        manga.Id,
        manga.Title,
        manga.Source.ToString(),
        manga.SourceUrl,
        manga.LastKnownChapter,
        manga.LastChapterUrl,
        manga.LastCheckedAt,
        manga.HasUnreadChapter);
}