using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using SshPlugin.Services;

namespace SshPlugin.Ui;

public partial class SshConnectionItem : UserControl
{
    public static readonly StyledProperty<SshConnection?> ConnectionProperty =
        AvaloniaProperty.Register<SshConnectionItem, SshConnection?>(nameof(Connection));

    public SshConnection? Connection
    {
        get => GetValue(ConnectionProperty);
        set => SetValue(ConnectionProperty, value);
    }
    
    public SshConnectionItem()
    {
        InitializeComponent();
        ConnectionProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<SshConnection?>>(value =>
        {
            if (value.NewValue.Value != null)
            {
                value.NewValue.Value.Changed += Update;
                Update();
            }
        }));
    }

    private void Update()
    {
        NameBlock.IsVisible = !string.IsNullOrWhiteSpace(Connection?.Name);
        NameBlock.Text = Connection?.Name;
        HostBlock.Text = $"{Connection?.Host}:{Connection?.Port}";

        ConnectedMarker.IsVisible = Connection?.IsConnected == true;
        ButtonDisconnect.IsVisible = Connection?.IsConnected == true;
        NotConnectedMarker.IsVisible = Connection?.IsConnected == false;
        ButtonConnect.IsVisible = Connection?.IsConnected == false;
    }

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Connection == null)
            return;
        Connection.Disconnect();
        Connection.Settings.Delete();
        SshPlugin.Connections.Remove(Connection);
    }
    
    private static Window GetParentWindow(StyledElement element)
    {
        if (element is Window window)
            return window;
        return GetParentWindow(element.Parent ?? throw new Exception());
    }

    private async void ButtonSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Connection == null)
            return;
        var dialog = new ConnectionOptionsWindow(Connection);
        await dialog.ShowDialog(GetParentWindow(this));
    }

    private async void ButtonConnect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Connection == null)
            return;
        Connection.IsEnabled = true;
        await Connection.Init();
    }

    private void ButtonDisconnect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Connection == null)
            return;
        Connection.IsEnabled = false;
        Connection.Disconnect();
    }
}