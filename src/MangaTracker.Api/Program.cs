using MangaTracker.Api.Endpoints;
using MangaTracker.Core.Interfaces;
using MangaTracker.Core.Services;
using MangaTracker.Infrastructure.Persistence;
using MangaTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using MangaTracker.Core.Interfaces;
using MangaTracker.Infrastructure.Providers.MangaLib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MangaTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<IMangaSourceProvider, MangaLibProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.cdnlibs.org/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "MangaTracker/1.0 (personal project)");
});

builder.Services.AddScoped<IMangaRepository, MangaRepository>();
builder.Services.AddScoped<MangaService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapMangaEndpoints();

app.Run();