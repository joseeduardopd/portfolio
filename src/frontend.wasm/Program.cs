using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Frontend.Wasm.App>("#app");

var apiBase = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<Frontend.Wasm.Services.TokenAuthorizationHandler>();
builder.Services.AddScoped(sp =>
{
    var http = new HttpClient(new Frontend.Wasm.Services.TokenAuthorizationHandler(sp.GetRequiredService<IJSRuntime>()))
    {
        BaseAddress = new Uri(apiBase)
    };
    return http;
});

await builder.Build().RunAsync();


