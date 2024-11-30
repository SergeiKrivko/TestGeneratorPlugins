﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class CreatePluginWindow : Window
{
    public CreatePluginWindow()
    {
        InitializeComponent();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        OptionsView.IsVisible = false;
        SpinnerView.IsVisible = true;

        await PluginsHttpService.Instance.CreatePlugin(new PluginCreate{Key = PluginKeyBox.Text ?? ""});
        
        SpinnerView.IsVisible = false;
        CreatedView.IsVisible = true;
    }
}