using Microsoft.AspNetCore.DataProtection;
using MusicStoreShowcase.Application;
using MusicStoreShowcase.Extensions.Services;
using MusicStoreShowcase.Infrastructure;
using MusicStoreShowcase.Infrastructure.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
        });
builder.Services.AddDiServices();
builder.Services.ConfigureOptions(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Songs}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();