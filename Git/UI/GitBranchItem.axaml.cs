using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using Git.Models;

namespace Git.UI;

public partial class GitBranchItem : UserControl
{
    public static readonly StyledProperty<GitBranch?> GitBranchProperty =
        AvaloniaProperty.Register<GitBranchItem, GitBranch?>(nameof(GitBranch));

    public GitBranch? GitBranch
    {
        get => GetValue(GitBranchProperty);
        set => SetValue(GitBranchProperty, value);
    }
    
    public GitBranchItem()
    {
        InitializeComponent();
        GitBranchProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<GitBranch?>>(next =>
            {
                Load();
                if (next.NewValue.Value != null)
                    next.NewValue.Value.Updated += Load;
            }));
    }

    private void Load()
    {
        IsCurrentMarker.IsVisible = GitBranch?.IsCurrent == true;
        NameBlock.Text = GitBranch?.DisplayName;
    }
}