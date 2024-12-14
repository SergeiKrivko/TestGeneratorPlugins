namespace Git.Models;

public class GitBranch
{
    public required string Name { get; init; }
    public string DisplayName => Path.GetFileName(Name);
    public bool IsCurrent { get; set; }
    public bool IsRemote { get; init; }

    public event Action? Updated;

    public void Update(GitBranch other)
    {
        IsCurrent = other.IsCurrent;
        Updated?.Invoke();
    }
}