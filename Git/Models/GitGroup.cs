using System.Collections.ObjectModel;

namespace Git.Models;

public class GitGroup
{
    public required string Name { get; init; }
    public required ObservableCollection<GitFile> Files { get; init; }

    public bool Selected
    {
        get => Files.Count != 0 && Files.All(f => f.Selected);
        set
        {
            foreach (var gitFile in Files)
            {
                gitFile.Selected = value;
            }
        }
    }

    public bool HaveFiles => Files.Count > 0;

    public void Merge(ICollection<GitFile> files)
    {
        foreach (var file in files)
        {
            var existing = Files.FirstOrDefault(f => f.FullPath == file.FullPath);
            if (existing == null)
                Files.Add(file);
            else
                existing.Update(file);
        }
        
        foreach (var file in Files.ToArray())
        {
            if (files.FirstOrDefault(f => f.FullPath == file.FullPath) == null)
            {
                Files.Remove(file);
            }
        }
    }
}