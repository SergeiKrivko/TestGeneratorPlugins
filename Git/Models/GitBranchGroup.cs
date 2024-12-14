using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Git.Models;

public class GitBranchGroup
{
    public required string Name { get; init; }
    public ObservableCollection<GitBranch> Branches { get; init; } = [];
    public ObservableCollection<GitBranchGroup> Groups { get; init; } = [];
    public ObservableCollection<object> Children { get; } = [];

    public GitBranchGroup()
    {
        Branches.CollectionChanged += OnCollectionChanged;
        Groups.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems ?? new List<object>())
                {
                    Children.Add(item);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems ?? new List<object>())
                {
                    Children.Remove(item);
                }

                break;
            default:
                Children.Clear();
                foreach (var branch in Groups)
                {
                    Children.Add(branch);
                }

                foreach (var branch in Branches)
                {
                    Children.Add(branch);
                }

                break;
        }
    }

    public void Add(GitBranch branch)
    {
        var group = this;
        foreach (var groupName in branch.Name.Split('/').SkipLast(1))
        {
            var existingGroup = Groups.FirstOrDefault(b => b.Name == groupName);
            if (existingGroup != null)
                group = existingGroup;
            else
                group.Groups.Add(group = new GitBranchGroup { Name = groupName });
        }

        var existing = group.Branches.FirstOrDefault(b => b.Name == branch.Name);
        if (existing != null)
            existing.Update(branch);
        else
            group.Branches.Add(branch);
    }

    public GitBranch? CurrentBranch => Branches.FirstOrDefault(b => b.IsCurrent) ??
                                       Groups.Select(g => g.CurrentBranch).FirstOrDefault(g => g != null);
}