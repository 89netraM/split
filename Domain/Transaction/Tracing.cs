using System.Diagnostics;

namespace Split.Domain.Transaction;

public static class Tracing
{
    public const string SourceName = "Split.Domain.Transaction";
    private const string SourceVersion = "1.0.0";

    internal static ActivitySource Source { get; } = new(SourceName, SourceVersion);
}
