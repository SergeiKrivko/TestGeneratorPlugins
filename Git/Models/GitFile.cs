using Avalonia.Media;

namespace Git.Models;

public class GitFile
{
    public enum GitFileStatus
    {
        Unknown,
        Modified,
        Added,
        Deleted,
        Renamed,
        Copied,
        Ignored,
    }
    
    public required string FullPath { get; init; }
    public required string RelativePath { get; set; }
    public string Name => Path.GetFileName(FullPath);
    public string? Directory => Path.GetDirectoryName(RelativePath);

    public GitFileStatus Status { get; set; }

    public bool IsStaged { get; set; }
    
    public bool Selected { get; set; }

    public IImmutableSolidColorBrush Color => Status switch
    {
        GitFileStatus.Deleted => Brushes.Gray,
        GitFileStatus.Added => Brushes.GreenYellow,
        GitFileStatus.Modified => Brushes.RoyalBlue,
        GitFileStatus.Copied => Brushes.RoyalBlue,
        GitFileStatus.Renamed => Brushes.RoyalBlue,
        _ => Brushes.OrangeRed
    };

    public event Action? Updated; 

    public void Update(GitFile other)
    {
        Status = other.Status;
        RelativePath = other.RelativePath;
        Updated?.Invoke();
    }
}