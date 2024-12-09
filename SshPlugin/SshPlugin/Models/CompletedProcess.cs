using TestGenerator.Shared.Types;

namespace SshPlugin.Models;

public class CompletedProcess : ICompletedProcess
{
    public required int ExitCode { get; init; }
    public string Stdout { get; init; } = "";
    public string Stderr { get; init; } = "";
    public TimeSpan Time { get; init; }
}