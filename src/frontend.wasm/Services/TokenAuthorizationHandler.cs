using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Frontend.Wasm.Services;

public sealed class TokenAuthorizationHandler : DelegatingHandler
{
    private readonly IJSRuntime js;

    public TokenAuthorizationHandler(IJSRuntime js)
    {
        this.js = js;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await js.InvokeAsync<string?>("localStorage.getItem", "auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch { }
        return await base.SendAsync(request, cancellationToken);
    }
}


