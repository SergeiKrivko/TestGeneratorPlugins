using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SshPlugin.Services;
using TestGenerator.Shared.Settings;

namespace SshPlugin.Ui;

public partial class ConnectionOptionsWindow : Window
{
    private SshConnection Connection { get; }
    private readonly bool _isNew;
    
    public ConnectionOptionsWindow(SshConnection connection, bool isNew = false)
    {
        _isNew = isNew;
        Connection = connection;
        
        InitializeComponent();
        Load();
    }

    private void Load()
    {
        NameBox.Text = Connection.Name;
        HostBox.Text = Connection.Host;
        PortBox.Value = Connection.Port;
        UsernameBox.Text = Connection.Username;
        PasswordBox.Text = Connection.Password;
        
        OsBox.SelectedValue = Connection.OperatingSystem;
        HostProgramPathBox.Text = Connection.HostProgramPath;
    }

    private void Save()
    {
        Connection.Name = NameBox.Text;
        Connection.Host = HostBox.Text ?? "";
        Connection.Port = (int?)PortBox.Value ?? 22;
        Connection.Username = UsernameBox.Text ?? "";
        Connection.Password = PasswordBox.Text ?? "";
        
        Connection.HostProgramPath = HostProgramPathBox.Text ?? "";
        Connection.OperatingSystem = OsBox.SelectedValue as string ?? "Linux";
    }

    private async void CheckConnectionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Save();
        
        CheckConnectionButton.IsVisible = false;
        ConnectedMarker.IsVisible = false;
        ErrorMarker.IsVisible = false;
        Spinner.IsVisible = true;
        
        var status = await Connection.Reconnect();

        Spinner.IsVisible = false;
        ConnectedMarker.IsVisible = status;
        ErrorMarker.IsVisible = !status;
        CheckConnectionButton.IsVisible = true;
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_isNew)
        {
            Connection.Disconnect();
            Connection.Settings.Delete();
        }
        Close();
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        Save();
        if (_isNew)
        {
            SshPlugin.Connections.Add(Connection);
        }
        Close();
    }
}