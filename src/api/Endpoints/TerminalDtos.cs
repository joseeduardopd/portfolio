using System.Text.Json;
using Api.Data;

namespace Api.Endpoints;

public sealed record TerminalCommandReadDto(Guid Id, string Key, string LabelPt, string LabelEn, int Order, bool Enabled, List<string> Steps);
public sealed record TerminalCommandCreateDto(string Key, string LabelPt, string LabelEn, int Order, bool Enabled, List<string>? Steps);
public sealed record TerminalCommandUpdateDto(string? Key, string? LabelPt, string? LabelEn, int? Order, bool? Enabled, List<string>? Steps);
public sealed record OrderPatch(int Order);
public sealed record EnabledPatch(bool Enabled);

public static class TerminalMapper
{
    public static TerminalCommandReadDto ToReadDto(TerminalCommandEntity e)
    {
        List<string> steps;
        try
        {
            steps = string.IsNullOrWhiteSpace(e.Steps)
                ? new List<string>()
                : (JsonSerializer.Deserialize<List<string>>(e.Steps) ?? new List<string>());
        }
        catch
        {
            steps = new List<string>();
        }
        return new TerminalCommandReadDto(e.Id, e.Key, e.LabelPt, e.LabelEn, e.Order, e.Enabled, steps);
    }
}


