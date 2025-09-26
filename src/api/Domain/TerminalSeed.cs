namespace Api.Domain;

public sealed record TerminalCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Key { get; init; } = string.Empty;
    public string LabelPt { get; init; } = string.Empty;
    public string LabelEn { get; init; } = string.Empty;
    public int Order { get; init; } = 0;
    public bool Enabled { get; init; } = true;
    public IReadOnlyList<Step> Steps { get; init; } = Array.Empty<Step>();

    public static TerminalCommand Seed(string key, string pt, string en, IEnumerable<Step> steps, int order = 0) => new()
    {
        Key = key,
        LabelPt = pt,
        LabelEn = en,
        Order = order,
        Enabled = true,
        Steps = steps.ToList()
    };
}

public abstract record Step
{
    public string Kind { get; init; } = string.Empty;
    public string? Value { get; init; }

    public static Step Print(string text) => new PrintStep { Kind = "print", Value = text };
}

public sealed record PrintStep : Step;


