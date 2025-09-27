using Client;
using Client.Diagnostics;
using Client.Interop;
using Client.Services;
using Client.Services.Physics;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AnimationInterop>();
builder.Services.AddScoped<AudioInterop>();
builder.Services.AddScoped<StorageInterop>();

builder.Services.AddScoped<IRngProvider, RngProvider>();
builder.Services.AddScoped<PhysicsEngine>();
builder.Services.AddScoped<ObstacleSpawner>();

builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<ScoreService>();
builder.Services.AddScoped<AudioService>();

builder.Services.AddScoped(sp => new DebugOverlayService(TimeSpan.FromMilliseconds(16.7), sampleWindowSize: 120));

builder.Services.AddScoped(sp =>
    new InputHandlerService(120d, sp.GetRequiredService<ILogger<InputHandlerService>>()));

builder.Services.AddScoped<GameLoopService>();

await builder.Build().RunAsync();
