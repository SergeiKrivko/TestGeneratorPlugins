using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using Git.Models;
using Git.Services;
using TestGenerator.Shared.Types;
using Timer = System.Timers.Timer;

namespace Git.UI;

public partial class GitTab : SideTab
{
    public override string TabName => "Git";
    public override string TabKey => "Git";
    public override int TabPriority => 10;

    public override string TabIcon =>
        "M21 10.503H17.812C17.4798 9.21461 16.7288 8.07317 15.6771 7.2582C14.6253 6.44324 13.3325 6.00098 12.002 6.00098C10.6715 6.00098 9.37867 6.44324 8.32693 7.2582C7.2752 8.07317 6.52418 9.21461 6.192 10.503H3C2.60218 10.503 2.22064 10.661 1.93934 10.9424C1.65804 11.2237 1.5 11.6052 1.5 12.003C1.5 12.4008 1.65804 12.7824 1.93934 13.0637C2.22064 13.345 2.60218 13.503 3 13.503H6.19C6.52149 14.7923 7.27234 15.9347 8.32435 16.7505C9.37636 17.5662 10.6698 18.0089 12.001 18.0089C13.3322 18.0089 14.6256 17.5662 15.6777 16.7505C16.7297 15.9347 17.4805 14.7923 17.812 13.503H21C21.197 13.503 21.392 13.4642 21.574 13.3888C21.756 13.3134 21.9214 13.203 22.0607 13.0637C22.1999 12.9244 22.3104 12.759 22.3858 12.577C22.4612 12.395 22.5 12.2 22.5 12.003C22.5 11.806 22.4612 11.611 22.3858 11.429C22.3104 11.247 22.1999 11.0816 22.0607 10.9424C21.9214 10.8031 21.756 10.6926 21.574 10.6172C21.392 10.5418 21.197 10.503 21 10.503ZM12 15.003C11.606 15.003 11.2159 14.9254 10.8519 14.7746C10.488 14.6239 10.1573 14.4029 9.87868 14.1243C9.6001 13.8458 9.37913 13.515 9.22836 13.1511C9.0776 12.7871 9 12.397 9 12.003C9 11.609 9.0776 11.2189 9.22836 10.855C9.37913 10.491 9.6001 10.1603 9.87868 9.88169C10.1573 9.60312 10.488 9.38214 10.8519 9.23137C11.2159 9.08061 11.606 9.00301 12 9.00301C12.7956 9.00301 13.5587 9.31908 14.1213 9.88169C14.6839 10.4443 15 11.2074 15 12.003C15 12.7987 14.6839 13.5617 14.1213 14.1243C13.5587 14.6869 12.7956 15.003 12 15.003Z";

    private IEnumerable<GitGroup> _gitGroups = [];

    private DateTime? _lastUpateTime;

    public GitTab()
    {
        InitializeComponent();
        GitStatusTree.ItemsSource = GitService.Instance.Groups;
        IsVisibleProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(next =>
        {
            if (next.OldValue.Value == false && next.NewValue.Value)
            {
                Update(AAppService.Instance.CurrentProject.Path);
            }
        }));
        AAppService.Instance.Subscribe<string>("projectChanged", Update);
    }

    private async void Update(string path)
    {
        if (!IsVisible || DateTime.UtcNow - _lastUpateTime < TimeSpan.FromMilliseconds(1000))
            return;
        _lastUpateTime = DateTime.UtcNow;
        try
        {
            await GitService.Instance.GitStatus(path);
            await BranchService.Instance.Refresh();
        }
        catch (Exception e)
        {
            Git.Logger.Error(e.Message);
        }
    }

    private IEnumerable<GitFile> Files()
    {
        foreach (var group in _gitGroups)
        {
            foreach (var file in group.Files)
            {
                yield return file;
            }
        }
    }

    private async void CommitButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var message = MessageBox.Text ?? "";
        await AAppService.Instance.RunBackgroundTask("Commit",
                () => GitService.Instance.GitCommit(Files().Where(f => f.Selected).Select(f => f.FullPath), message))
            .Wait();
        Update(AAppService.Instance.CurrentProject.Path);
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Update(AAppService.Instance.CurrentProject.Path);
    }
}