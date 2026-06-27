using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cropper.Blazor.Extensions;
using InitiativeTracker.Components;
using InitiativeTracker.Infrastructure;
using InitiativeTracker.Infrastructure.Extensions;
using InitiativeTracker.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.Options;
using Serilog;

[assembly: InternalsVisibleTo("InitiativeTracker.Tests")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(nameof(AppOptions)));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddLogging(c =>
{
    c.ClearProviders()
        .AddSerilog();
});

builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWarmUp>()
    .AddClasses(classes => classes.AssignableTo<IWarmUp>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddCropper();

builder.Services
    .AddHttpClients(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddApplication();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB for large images
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

await app.WarmUp();
app.Lifetime.ApplicationStopping.Register(_ => app.TearDown(), null);

var appOptions = app.Services.GetRequiredService<IOptionsMonitor<AppOptions>>();
if (appOptions.CurrentValue.OpenBrowserOnStart)
    OpenBrowser(appOptions.CurrentValue.BrowserUrl);

app.Run();

static void OpenBrowser(string url)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        Process.Start("xdg-open", url);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        Process.Start("open", url);
    }
    // throw 
}