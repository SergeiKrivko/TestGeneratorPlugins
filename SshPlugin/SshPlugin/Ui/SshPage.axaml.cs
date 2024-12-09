using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SshPlugin.Services;

namespace SshPlugin.Ui;

public partial class SshPage : UserControl
{
    public SshPage()
    {
        InitializeComponent();
        ConnectionsList.ItemsSource = SshPlugin.Connections;
    }

    private static Window GetParentWindow(StyledElement element)
    {
        if (element is Window window)
            return window;
        return GetParentWindow(element.Parent ?? throw new Exception());
    }

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new ConnectionOptionsWindow(new SshConnection(Guid.NewGuid()), isNew: true);
        await dialog.ShowDialog(GetParentWindow(this));
    }
}