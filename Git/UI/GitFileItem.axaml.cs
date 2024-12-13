using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using Git.Models;

namespace Git.UI;

public partial class GitFileItem : UserControl
{
    public static readonly StyledProperty<GitFile?> GitFileProperty =
        AvaloniaProperty.Register<GitFileItem, GitFile?>(nameof(GitFile));

    public GitFile? GitFile
    {
        get => GetValue(GitFileProperty);
        set => SetValue(GitFileProperty, value);
    }

    public GitFileItem()
    {
        InitializeComponent();
        GitFileProperty.Changed.Subscribe(
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<GitFile?>>(next =>
            {
                Load();
                if (next.NewValue.Value != null)
                    next.NewValue.Value.Updated += Load;
            }));
    }

    private void Load()
    {
        FilenameBlock.Text = GitFile?.Name;
        FilenameBlock.Foreground = GitFile?.Color;
        DirectoryBlock.Text = GitFile?.Directory;
        CheckBox.IsChecked = GitFile?.Selected;
    }

    private void CheckBox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (GitFile == null)
            return;
        GitFile.Selected = CheckBox.IsChecked ?? false;
    }
}