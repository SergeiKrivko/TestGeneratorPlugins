namespace SshPlugin.Api.Models;

public class CompletedProcess
{
    public required int ExitCode { get; init; }
    public string Stdout { get; init; } = "";
    public string Stderr { get; init; } = "";
    public TimeSpan Time { get; init; }
}