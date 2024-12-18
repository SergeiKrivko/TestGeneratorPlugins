﻿using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SshPlugin.Models;
using SshPlugin.Services;
using TestGenerator.Shared.Settings;

namespace SshPlugin.Ui;

public partial class ConnectionOptionsWindow : Window
{
    private SshConnection Connection { get; }
    private readonly bool _isNew;

    private static ObservableCollection<OperatingSystemModel> OperatingSystems { get; } =
    [
        new OperatingSystemModel { Key = "win-x64", DisplayName = "Windows" },
        new OperatingSystemModel { Key = "linux-x64", DisplayName = "Linux" },
        new OperatingSystemModel { Key = "osx-x64", DisplayName = "macOS" }
    ];
    
    public ConnectionOptionsWindow(SshConnection connection, bool isNew = false)
    {
        _isNew = isNew;
        Connection = connection;
        
        InitializeComponent();
        OsBox.ItemsSource = OperatingSystems;
        Load();
    }

    private void Load()
    {
        NameBox.Text = Connection.Name;
        HostBox.Text = Connection.Host;
        PortBox.Value = Connection.Port;
        UsernameBox.Text = Connection.Username;
        AuthMethodBox.SelectedIndex = Connection.AuthenticationMethod == AuthenticationMethod.PrivateKey ? 1 : 0;
        PasswordBox.IsVisible = Connection.AuthenticationMethod == AuthenticationMethod.Password;
        PrivateKeyPathBox.IsVisible = Connection.AuthenticationMethod == AuthenticationMethod.PrivateKey;
        SelectPrivateKeyButton.IsVisible = Connection.AuthenticationMethod == AuthenticationMethod.PrivateKey;
        PasswordBox.Text = Connection.Password;
        PrivateKeyPathBox.Text = Connection.PrivateKeyFile;

        AutoConnectCheckbox.IsChecked = Connection.AutoConnect;
        OsBox.SelectedValue = OperatingSystems.FirstOrDefault(s => s.Key == Connection.OperatingSystem);
        HostProgramPathBox.Text = Connection.HostProgramPath;
    }

    private void Save()
    {
        Connection.Name = NameBox.Text;
        Connection.Host = HostBox.Text ?? "";
        Connection.Port = (int?)PortBox.Value ?? 22;
        Connection.Username = UsernameBox.Text ?? "";
        Connection.AuthenticationMethod = AuthMethodBox.SelectedIndex == 1
            ? AuthenticationMethod.PrivateKey
            : AuthenticationMethod.Password;
        Connection.Password = PasswordBox.Text ?? "";
        Connection.PrivateKeyFile = PrivateKeyPathBox.Text ?? "";

        Connection.AutoConnect = AutoConnectCheckbox.IsChecked == true;
        Connection.HostProgramPath = HostProgramPathBox.Text ?? "";
        Connection.OperatingSystem = (OsBox.SelectedValue as OperatingSystemModel)?.Key ?? "linux-x64";
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
        
        Load();
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
            InitNewConnection();
        }
        Close();
    }

    private async void InitNewConnection()
    {
        await Connection.Init();
    }

    private void AuthMethodBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        PasswordBox.IsVisible = AuthMethodBox.SelectedIndex == 0;
        PrivateKeyPathBox.IsVisible = AuthMethodBox.SelectedIndex == 1;
        SelectPrivateKeyButton.IsVisible = AuthMethodBox.SelectedIndex == 1;
    }

    private async void SelectPrivateKeyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Выберите файл",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            PrivateKeyPathBox.Text = files[0].Path.LocalPath;
        }
    }
}