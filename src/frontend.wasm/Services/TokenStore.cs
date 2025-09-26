using Microsoft.JSInterop;

namespace Frontend.Wasm.Services;

public static class TokenStore
{
    private const string Key = "auth_token";
    private static IJSRuntime? _js;

    public static void Configure(IJSRuntime js) => _js = js;

    public static async Task SetAsync(string token)
    {
        if (_js is null) return;
        await _js.InvokeVoidAsync("localStorage.setItem", Key, token);
    }

    public static async Task<string?> GetAsync()
    {
        if (_js is null) return null;
        return await _js.InvokeAsync<string?>("localStorage.getItem", Key);
    }

    public static async Task ClearAsync()
    {
        if (_js is null) return;
        await _js.InvokeVoidAsync("localStorage.removeItem", Key);
    }
}


