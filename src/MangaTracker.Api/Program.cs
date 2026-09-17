using MangaTracker.Api.Endpoints;
using MangaTracker.Core.Interfaces;
using MangaTracker.Core.Services;
using MangaTracker.Infrastructure.Persistence;
using MangaTracker.Infrastructure.Persistence.Repositories;
using MangaTracker.Infrastructure.Providers;
using MangaTracker.Infrastructure.Providers.Html;
using MangaTracker.Infrastructure.Providers.MangaLib;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MangaTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<MangaLibProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.cdnlibs.org/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "MangaTracker/1.0 (personal project)");
});

builder.Services.AddHttpClient<MangaShiProvider>(ConfigureHtmlClient);
builder.Services.AddHttpClient<MangaBluePeriodProvider>(ConfigureHtmlClient);
builder.Services.AddHttpClient<MangaRecordOfRagnarokProvider>(ConfigureHtmlClient);
builder.Services.AddHttpClient<MangaHunterProvider>(ConfigureHtmlClient);

builder.Services.AddScoped<IMangaRepository, MangaRepository>();
builder.Services.AddScoped<MangaService>();

builder.Services.AddScoped<IMangaSourceProvider>(sp => sp.GetRequiredService<MangaLibProvider>());
builder.Services.AddScoped<IMangaSourceProvider>(sp => sp.GetRequiredService<MangaShiProvider>());
builder.Services.AddScoped<IMangaSourceProvider>(sp => sp.GetRequiredService<MangaBluePeriodProvider>());
builder.Services.AddScoped<IMangaSourceProvider>(sp => sp.GetRequiredService<MangaRecordOfRagnarokProvider>());
builder.Services.AddScoped<IMangaSourceProvider>(sp => sp.GetRequiredService<MangaHunterProvider>());

builder.Services.AddScoped<IMangaSourceProviderFactory, MangaSourceProviderFactory>();
builder.Services.AddScoped<MangaUpdateChecker>();

static void ConfigureHtmlClient(HttpClient client)
{
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Add("User-Agent", "MangaTracker/1.0 (personal project)");
}

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