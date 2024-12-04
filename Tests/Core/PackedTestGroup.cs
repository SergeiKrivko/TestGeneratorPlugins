namespace Tests.Core;

public class PackedTestGroup
{
    public required string Name { get; init; }
    public PackedTestGroup[] Groups { get; init; } = [];
    public string[] Tests { get; init; } = [];
}